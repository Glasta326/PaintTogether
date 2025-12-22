using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using PaintTogetherServer.Common.SvLogger;
using PaintTogetherServer.Core;
using PaintTogetherServer.Core.ActionHistory;
namespace PaintTogetherServer
{
    public static partial class Program
    {
        public static TcpListener Listener;

        /// <summary>
        /// Client dictionary indexed by client ID.
        /// </summary>
        public static ConcurrentDictionary<uint, PaintClient> Clients = new ConcurrentDictionary<uint, PaintClient>();

        public static List<WorkerThread> Workers = new List<WorkerThread>();

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

            uint clientCounter = 0;
            try
            {
                // Server loop, constantly look for joining clients and allocate a handler task for them
                while (!cts.Token.IsCancellationRequested)
                {
                    TcpClient client = await Listener.AcceptTcpClientAsync();
                    PaintClient pc = new PaintClient(client, clientCounter);
                    SvLogger.LogInfo($"Client joined on [{pc.ip}]");

                    // Clients are stored in a dict, so we create a new entry with the client's ID, and the client itself.
                    // Worth noting the reason i do this is so the data packets sent the worker threads can be really simple:
                    // The packet is basically just [ID],[DATA]
                    // the workers can just compare integers to see if id's match
                    // maybe my logic is flawed but this is also good if i never need to look up clients by ID
                    Clients.TryAdd(pc.ID, pc);
                    _ = HandleClient(pc.ID);

                    clientCounter++;
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
            EventReplay.Init();

            // If threadcount is < 0, that means we use default to using however many processors this computer has
            int workerCount = ThreadCount < 0 ? Environment.ProcessorCount : ThreadCount;
            for (int i = 0; i < workerCount; i++)
            {
                Workers.Add(new WorkerThread(i));
            }
            SvLogger.LogInfo($"Created {workerCount} worker threads");


            Listener = new TcpListener(IPAddress.Any, ListenerPort);
            Listener.Start();
            SvLogger.LogInfo($"Server started on port: {ListenerPort}");
        }


        /// <summary>
        /// Read incoming data from each client and enqueue the client's actions to the <see cref="WorkerThread.WorkQueue"/>
        /// </summary>
        public static async Task HandleClient(uint ID)
        {
            // Oh shit we just got a new client
            PaintClient pc = Clients[ID];
            using var reader = new BinaryReader(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // Most important thing is to valid the user is actually on the same version as everyone else.
            // We expect the first packet to be a string with the client's version
            // If the client version is in any way different to our version, we dont allow this person in.
            string clVersion = reader.ReadString();
            if (clVersion != VERSION)
            {
                SvLogger.LogWarning($"Client [{pc.ip}] was rejected due to a version mismatch: Server = [{VERSION}], Client = [{clVersion}]");
                pc.tcp.Close();
                return;
            }

            // Next, we expect another string for the user's username
            // Same deal, if we don't get one, disconnect the user
            // TODO: username checks, if anyone already has this username, if username is blank, ect
            string _userName = reader.ReadString();
            pc.UserName = _userName;

            SvLogger.LogInfo($"Client [IP: {pc.ip}, ID: {pc.ID}] has joined with username: {pc.UserName}");

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

            SvLogger.LogInfo($"Client [IP: {pc.ip}, ID: {pc.ID}, Username: {pc.UserName}] has disconnected ");
        }

        static void Unload()
        {
            Workers.Clear();
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
        --threadCount <count>               : Overrides the number of worker threads the server will allocate for handling client operations. [0-255] {-1}
        --port <number>                     : Overrides the port the server listen for clients on. [0-65535] {12504}";
                            SvLogger.LogInfo(s);
                            return true; // If the user specified help in any way, *return* true which tells main not to run the actual server
                                         // We dont parse any other params at all if help was a specified param

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




