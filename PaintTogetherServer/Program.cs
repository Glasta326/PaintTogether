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
using PaintTogetherServer.Core.UserRegistry;
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
       // public static ConcurrentDictionary<Guid, PaintUser> Users = new ConcurrentDictionary<Guid, PaintUser>();

        /// <summary>
        /// Dictionary of activley connected users
        /// </summary>
        //public static ConcurrentDictionary<byte, PaintClient> Clients = new ConcurrentDictionary<byte, PaintClient>();

        public static byte UserCounter = 0;
        public static object CounterLock = new();

        public static PaintUsers RegisteredUsers = new PaintUsers();

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

                    string[] endStrings = ["end", "stop", "terminate"];
                    if (endStrings.Contains(r.ToLower()))
                    {
                        cts.Cancel();
                        Listener.Stop();
                    }

                    string[] listUsersStrings = ["listUsers", "list users", "userlist", "ls"];
                    if (listUsersStrings.Contains(r.ToLower()))
                    {
                        Console.WriteLine($"[ID] [GUID] [IP] [Username] [Connected]");
                        foreach (var usr in RegisteredUsers._UsersById.Values)
                        {
                            string ip = usr.IsConnected ? usr.Connection.ip : "NULL";
                            Console.WriteLine($"[{usr.ClientID}, {usr.UserID}, {ip}, {usr.UserName}, {usr.IsConnected}]");
                        }
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
                    _ = HandleClient(incoming);

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
        public static async Task HandleClient(TcpClient client)
        {
            PaintConnection pc = new PaintConnection(client);

            using var reader = new BinaryReader(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);
            using var writer = new BinaryWriter(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);

            # region User checks

            // Ok first check we even have room for this guy
            if (RegisteredUsers.Count >= MaxUsers)
            {
                SvLogger.LogWarning($"Client [{pc.ip}] was rejected due to maxmimum connection count reached: {RegisteredUsers.Count}");
                writer.Write((short)CommonKeys.ServerPacketTypes.RejectServerConnectionLimitReached);
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
                writer.Write((short)CommonKeys.ServerPacketTypes.RejectVersionMismatch);
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
                writer.Write((short)CommonKeys.ServerPacketTypes.RejectBadGUID);
                pc.tcp.Close();
                return;
            }


            byte thisUserID;

            // If we've seen this GUID before, that means this client is attempting to log in as an already existing user
            // We assign this new client connection the id's this user has and broadcast the id of the rejoining user
            if (RegisteredUsers.TryGetValue(clGuid, out PaintUser? user))
            {
                // This means whoever is trying to join is username that already exists
                // Reject them and tell them that user is already connected
                if (user.IsConnected)
                {
                    SvLogger.LogWarning($"Client [{pc.ip}] was rejected due to trying to log on on a GUID that is already connected: [{clGuid}]");
                    writer.Write((short)CommonKeys.ServerPacketTypes.RejectUserAlreadyConnected);
                    pc.tcp.Close();
                    return;
                }

                // Set the user who we've now recognised's conection to be the one we're handling now
                user.Connection = pc;
            }
            // If we've never seen this GUID before, this means this is a new user and we need to asign them a new user slot
            else
            {
                // First we need the username, Request the client to send it to us
                //NetUtils.SendServerPacket(CommonKeys.ServerPacketTypes.RequestUsername, pc, []);
                string userName = clGuid.ToString();

                lock (CounterLock)
                {
                    thisUserID = UserCounter;
                    UserCounter++;
                }

                PaintUser toAdd = new PaintUser(clGuid, thisUserID, userName);
                toAdd.Connection = pc;
                if (!RegisteredUsers.TryAdd(toAdd))
                {
                    SvLogger.LogWarning($"Something went wrong. Could not add user: [GUID: {toAdd.UserID}, ID: {toAdd.ClientID}, UserCounter: {UserCounter}]");
                    writer.Write((short)CommonKeys.ServerPacketTypes.RejectUserUnknown);
                    pc.tcp.Close();
                }
            }

            #endregion

            // Reference to the user this task looks after
            PaintUser thisUser = RegisteredUsers[clGuid];
            SvLogger.LogInfo($"Client [IP: {thisUser.Connection.ip}, ID: {thisUser.ClientID}, GUID: {thisUser.UserID}] has joined with username: {thisUser.UserName}");

            // TODO: Might need to do something better? unsure
            // First, directly inform the user who they are. Client is expecting this so we can just send the single id byte
            NetUtils.SendServerPacket(CommonKeys.ServerPacketTypes.WhisperInformClientID, thisUser, [thisUser.ClientID]);
            
            // AFTER directly telling the client, we can broadcast it to everyone that USER with ID and GUID and USERNAME has joined
            MemoryStream ms = new MemoryStream();
            BinaryWriter b = new BinaryWriter(ms);
            b.Write(thisUser.ClientID);
            b.Write(thisUser.UserID.ToString());
            b.Write(thisUser.UserName);
            byte[] payload = ms.ToArray();
            NetUtils.BroadcastServerPacket(CommonKeys.ServerPacketTypes.AnnounceUserConnecting, payload);


            try
            {
                while (thisUser.IsConnected)
                {
                    // Read the type of packet we've recieived (byte)
                    byte[] msgType = new byte[1];
                    await thisUser.Connection.Stream.ReadExactlyAsync(msgType);

                    // Read the length of the data in this packet (int32)
                    byte[] msgLengthBytes = new byte[4];
                    await thisUser.Connection.Stream.ReadExactlyAsync(msgLengthBytes);
                    int msgLength = BitConverter.ToInt32(msgLengthBytes);

                    // Read the byte array data
                    byte[] msgData = new byte[msgLength];
                    await thisUser.Connection.Stream.ReadExactlyAsync(msgData);

                    SvLogger.LogInfo($"Recived packet: [Type: {msgType[0]}, Length: {msgLength}]", true);

                    WorkerThread.WorkQueue.Add
                    (
                        new InfoPacket(thisUser.ClientID, msgType[0], msgData)
                    );
                }
            }
            catch (EndOfStreamException)
            {
                SvLogger.LogInfo($"Client [IP: {thisUser.Connection.ip}, ID: {thisUser.ClientID}, Username: {thisUser.UserName}] has disconnected ");
                thisUser.Connection.tcp.Close();
                thisUser.Connection = null;
            }
        }

        static void Unload()
        {
            WorkerThread.Workers.Clear();
            RegisteredUsers.Unload();
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




