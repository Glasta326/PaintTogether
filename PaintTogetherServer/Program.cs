using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using PaintTogetherServer.Common.Utilities;
using PaintTogetherServer.Common.SvLogger;
using PaintTogetherServer.Core;
using PaintTogetherServer.Core.ActionHistory;
namespace PaintTogetherServer
{
    public static partial class Program
    {
        public static TcpListener Listener;

        /// <summary>
        /// User dictionary indexed by GUID. <br/>
        /// This is persistent data and only ever lost on server restart. <br/>
        /// When a client first joins, they give us a GUID, and we assign them a user in this dictionary <br/>
        /// If the client disconnects, we remove their ConnectionID from the Clients dictionary, but that user still remains in the user dict <br/>
        /// When a client connects and sends an already existing GUID (implying this client re-connected and was here before), <br/>
        /// We create a new entry in the Clients dict with that GUID's associated byte ID.<br/>
        /// </summary>
        public static ConcurrentDictionary<Guid, PaintUser> Users = new ConcurrentDictionary<Guid, PaintUser>();

        /// <summary>
        /// Dictionary of activley connected users
        /// </summary>
        public static ConcurrentDictionary<uint, PaintClient> Clients = new ConcurrentDictionary<uint, PaintClient>();

        private static byte ClientCounter = 0;

        static async Task Main(string[] args)
        {
            SvLogger.Init(); // Get logging going asap
            // Returns true if certain args were passed that prevent program execution
            if (HandleArgs(args))
            {
                SvLogger.Unload();
                return;
            }

            StartServer();

            // Seperate task that reads when the server operator is attempting to input actions to the server
            CancellationTokenSource cts = new CancellationTokenSource();
            _ = Task.Run(() =>
            {
                while (!cts.IsCancellationRequested)
                {
                    Console.WriteLine($"Enter here: ");
                    string r = Console.ReadLine();
                    r ??= "_";
                    string[] types = ["end", "End", "END"];
                    if (types.Contains(r.ToLower()))
                    {
                        cts.Cancel();
                        Listener.Stop();
                    }
                }
            });



            try
            {
                // Server loop, constantly look for joining clients and allocate a handler task for them
                while (!cts.Token.IsCancellationRequested)
                {
                    TcpClient incoming = await Listener.AcceptTcpClientAsync();
                    SvLogger.LogInfo($"New incoming connection on: [{((IPEndPoint)incoming.Client.RemoteEndPoint).Address}]");
                    _ = HandleClient(incoming, ClientCounter);

                    if (ClientCounter < byte.MaxValue)
                    {
                        ClientCounter++;
                    }
                    else
                    {
                        SvLogger.LogWarning($"SERVER HAS REACHED MAXIMUM USERS. RUNNING IN SUSPENDED MODE");
                    }
                }
            }
            // This triggers when the listener is stopped, and hopefully, also when the loop ends
            catch (SocketException e)
            {
                SvLogger.LogInfo($"Server loop exited succesfully");
            }


            Unload();
            SvLogger.Unload(); // Last thing we do is stop logging
        }

        static void StartServer()
        {
            string enabled = SvLogger.VerboseLogging ? "enabled." : "disabled.";
            SvLogger.LogInfo($"Verbose logging {enabled}");

            EventReplay.Init();

            // If threadcount is < 0, that means we use default to using however many processors this computer has
            int workerCount = ThreadCount < 0 ? Environment.ProcessorCount : ThreadCount;
            for (int i = 0; i < workerCount; i++)
            {
                WorkerThread.Workers.Add(new WorkerThread(i));
            }
            SvLogger.LogInfo($"Created {workerCount} worker threads");

            Listener = new TcpListener(IPAddress.Any, ListenerPort);
            Listener.Start();
            SvLogger.LogInfo($"Server started on port: {ListenerPort}");
        }


        /// <summary>
        /// Read incoming data from each client and enqueue the client's actions to the <see cref="WorkerThread.WorkQueue"/>
        /// </summary>
        public static async Task HandleClient(TcpClient client, byte ID)
        {
            PaintClient pc = new PaintClient(client, ID);

            using var reader = new BinaryReader(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);
            using var writer = new BinaryWriter(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // Ok first check we even have room for this guy
            if (ClientCounter >= MaxUsers)
            {
                SvLogger.LogWarning($"Client [{pc.ip}] was rejected due to maxmimum connection count reached: {ClientCounter}");
                writer.Write((short)CommonKeys.ServerPacketTypes.ServerConnectionLimitReached);
                pc.tcp.Close();
                return;
            }

            // Most important thing is to valid the user is actually on the same version as everyone else.
            // We expect the first packet to be a string with the client's version
            // If the client version is in any way different to our version, we dont allow this person in.
            string clVersion = reader.ReadString();
            if (clVersion != VERSION)
            {
                SvLogger.LogWarning($"Client [{pc.ip}] was rejected due to a version mismatch: Server = [{VERSION}], Client = [{clVersion}]");
                writer.Write((short)CommonKeys.ServerPacketTypes.VersionMismatch);
                pc.tcp.Close();
                return;
            }

            // Next we get the client's """username""", really its just the guid so we can identify if we've seen them before
            string _ = reader.ReadString();
            Guid clGuid = Guid.Empty;
            try
            {
                clGuid = new Guid(_);
            }
            catch (FormatException)
            {
                SvLogger.LogWarning($"Client [{pc.ip}] was rejected due to a bad GUID: [{_}]");
                writer.Write((short)CommonKeys.ServerPacketTypes.BadUID);
                pc.tcp.Close();
                return;
            }

            // If we've seen this GUID before, that means this client is attempting to log in as an already existing user
            // We assign this new client connection the id's this user has and broadcast the id of the rejoining user
            if (Users.TryGetValue(clGuid, out PaintUser userInfo))
            {
                pc = new PaintClient(pc.tcp, userInfo.ClientID);
                if (!Clients.TryAdd(userInfo.ClientID, pc))
                {
                    SvLogger.LogWarning($"Could not create new PaintClient from existing PaintUser");
                }

                NetUtils.BroadcastServerPacket(CommonKeys.ServerPacketTypes.ExistingUserConnecting, [userInfo.ClientID]);
            }
            // If we've never seen this GUID before, this means this is a new user and we need to asign them a new user slot
            else
            {
                // First we need the username, Request the client to send it to us
                //NetUtils.SendServerPacket(CommonKeys.ServerPacketTypes.RequestUsername, pc, []);
                string userName = clGuid.ToString();

                if (!Users.TryAdd(clGuid, new PaintUser(clGuid, ID, userName)))
                {
                    SvLogger.LogWarning($"Could not create new PaintUser");
                }
                if (!Clients.TryAdd(ID, pc))
                {
                    SvLogger.LogWarning($"Could not create new PaintClient from new PaintUser");
                }

            }


            SvLogger.LogInfo($"Client [IP: {pc.ip}, ID: {pc.ID}, GUID: {clGuid}] has joined with username: {Users[clGuid].UserName}");

            try
            {
                while (pc.tcp.Connected)
                {
                    byte packetType = reader.ReadByte();
                    int length = reader.ReadInt32();
                    byte[] data = reader.ReadBytes(length);

                    WorkerThread.WorkQueue.Add
                    (
                        new InfoPacket(ID, packetType, data)
                    );

                    await Task.Delay(1); // what the fuck???
                    // I'm assuming this is some kind of compiler optimisation.
                    // if you have an async task, but don't actually write await anywhere, then it wont be async??????????/
                    // im just doing this await task delay for zero purpose other than to force the compiler to realise its async
                }
            }
            catch (EndOfStreamException)
            {
                if (Clients.TryRemove(ID, out PaintClient? _))
                {
                    SvLogger.LogInfo($"Client [IP: {pc.ip}, ID: {pc.ID}, Username: {Users[clGuid].UserName}] has disconnected ");
                }
                else
                {
                    throw new Exception($"Could not remove client [IP: {pc.ip}, ID: {pc.ID}, Username: {Users[clGuid].UserName}]");
                }
            }
        }

        static void Unload()
        {
            WorkerThread.Workers.Clear();
            foreach (var ky in Clients.Keys)
            {
                Clients[ky].tcp.Close();
            }
        }

        /// <summary>
        /// Sets program flags based on user params
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool HandleArgs(string[] args)
        {
            if (args.Length == 0) { return false; }

            // Deal with annoying case differences
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = args[i].ToLower();
            }

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        case "--h":
                        case "--help":
                            // My poor poor indentation
                            string s = @"
    Usage: ./PaintTogetherServer [Options...]
    Example: ./PaintTogetherServer --threadCount 8 --port 7777

    [Format]
        --optionName, --alternate <input>   : Information about the option. [Accepted value range] {Default value}
    Options:
        --help, --h                         : Displays this information about PaintTogetherServer and available options.
        --verbose, --v                      : Enables verbose logging. Writes in extreme detail to the log file {False}
        --threadCount <count>               : Overrides the number of worker threads the server will allocate for handling client operations. [0-255] {-1}
        --port <number>                     : Overrides the port the server listen for clients on. [0-65535] {12504}";
                            SvLogger.LogInfo(s);
                            return true; // If the user specified help in any way, *return* true which tells main not to run the actual server
                                         // We dont parse any other params at all if help was a specified param

                        case "--verbose":
                        case "--v":
                            SvLogger.VerboseLogging = true;
                            break;

                        case "--threadcount":
                            if (!int.TryParse(args[i + 1], out int _threadCount) || _threadCount < 0 || _threadCount > byte.MaxValue)
                            {
                                SvLogger.LogWarning($"Could not parse value for {args[i]}");
                                i++; // Prevent trying to read the specified threadcount param that comes after
                                break;
                            }
                            ThreadCount = _threadCount; // Why can't i just do out threadCount...
                            i++;
                            break;

                        case "--port":
                            if (!ushort.TryParse(args[i + 1], out ushort _port) || _port < 0 || _port > ushort.MaxValue)
                            {
                                SvLogger.LogWarning($"Could not parse value for {args[i]}");
                                i++;
                                break;
                            }
                            ListenerPort = _port;
                            i++;
                            break;

                        default:
                            SvLogger.LogWarning($"Could not identify option: {args[i]}");
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    SvLogger.LogWarning($"Value parameter: {args[i]} was passed without specifying value!");
                }
            }

            return false;
        }
    }
}




