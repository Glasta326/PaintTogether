using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PaintTogetherServer.Common.SvLogger;
using PaintTogetherServer.Core;

namespace PaintTogetherServer
{
    public static partial class Program
    {
        public static TcpListener Listener;

        public static List<PaintClient> Clients = new List<PaintClient>();

        public static List<WorkerThread> Workers = new List<WorkerThread>();

        private volatile static bool Running = true;

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


            // Constantly look for joining clients and allocate a listener task for them
            while (Running)
            {
                TcpClient client = await Listener.AcceptTcpClientAsync();
                PaintClient pc = new PaintClient(client);
                pc.ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                SvLogger.LogInfo($"Client joined on [{pc.ip}]");
                Clients.Add(pc);
                _ = HandleClient(pc);
            }

            Unload();
            SvLogger.Unload(); // Last thing we do is stop logging
        }

        static void StartServer()
        {
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
        public static async Task HandleClient(PaintClient pc)
        {
            using var reader = new BinaryReader(pc.stream, System.Text.Encoding.UTF8, leaveOpen: true);
            using var writer = new BinaryWriter(pc.stream, System.Text.Encoding.UTF8, leaveOpen: true);

            // read some specific information from the client somehow to give us a username
            string name = reader.ReadString();
            SvLogger.LogInfo($"Name : {name}");
            while (true)
            {
                // Read stream from client and enque packets to the work queue for the worker threads to handle

                int x = reader.ReadInt32();
                int y = reader.ReadInt32();   
                Point p = new Point(x,y);
                WorkerThread.WorkQueue.Add(p);
            }
        }

        static void Unload()
        {
            Workers.Clear();
            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].tcp.Close();
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




