using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Core.Networking.Registry;

namespace PaintTogether.Core.Networking
{
    /// <summary>
    /// Handles all Incoming and outgoing network traffic, and delegates incoming commands to the appropriate function
    /// </summary>
    public static class NetSorter
    {
        public static TcpClient Client { get; private set; }

        private static Thread SenderThread;

        private static Thread ReaderThread;

        // Create our tcp client and initalise both network threads
        public static bool Init()
        {
            string ip = "86.20.41.142";
            try
            {
                // TODO: proper ui for server browsing or similar, and not just forcing an attmept to connect to this hardcoded ip when starting and defaulting to singleplayer if errored
                Client = new TcpClient(ip, 12504);
                
                SenderThread = new Thread(SendToServer) { IsBackground = true };
                SenderThread.Start();

                ReaderThread = new Thread(ReadFromServer) { IsBackground = true };
                ReaderThread.Start();
            }
            catch (System.Exception)
            {
                clLogger.LogInfo($"Server [IP: {ip}, Port: {12504}] was not found.");
                Client?.Dispose();
                return false;
            }
            return true;
        }

        public static ConcurrentQueue<int> OutgoingDrawCommands { get; private set; } = new ConcurrentQueue<int>();
        public static void SendToServer()
        {
            clLogger.LogInfo($"Sender thread started");
            BinaryWriter writer = new BinaryWriter(Client.GetStream(), System.Text.Encoding.UTF8, true);

            // The server expects a version string from us.
            writer.Write(LoggableData.ClientVersionInfo());
            writer.Flush();

            // Next, the server expects our login id
            // For now, we just have a GUID from a file randomized slightly via processID
            // TODO: base if off the username or something we need a proper startup screen ui thing so the user can select all this
            string path = Path.Combine(CommonKeys.MainDirectory, "GUID.txt");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, Guid.NewGuid().ToString());
            }
            Guid myGuid = new Guid(File.ReadAllText(path));
            var x = myGuid.ToByteArray();
            x[0] = (byte)Environment.ProcessId;
            myGuid = new Guid(x);
            writer.Write(myGuid.ToString());
            writer.Flush();

            while (Client.Connected)
            {
                using var ms = new MemoryStream();
                using var w = new BinaryWriter(ms);

                w.Write(MouseData.MousePosPoint().X);
                w.Write(MouseData.MousePosPoint().Y);

                byte[] data = ms.ToArray();
                byte packetType = 8;

                writer.Write(packetType);
                writer.Write(data.Length);
                writer.Write(data);
                writer.Flush();

                Thread.Sleep(100);
            }
            clLogger.LogInfo($"Server disconnected.");
            return;
        }

        public static ConcurrentQueue<int> IncomingDrawCommands { get; private set; } = new ConcurrentQueue<int>();
        public static void ReadFromServer()
        {
            clLogger.LogInfo($"Reader thread started");
            BinaryReader reader = new BinaryReader(Client.GetStream(), System.Text.Encoding.UTF8, true);
            int packetCount = 0;
            while (Client.Connected)
            {
                byte dataOwner = reader.ReadByte();
                byte dataType = reader.ReadByte();
                int dataLen = reader.ReadInt32();

                // Ideally now, we have WHO owns the data, if its the server we have custom logic, but otherwise we ideally do something like:
                // NetapplicableRegistry[dataType].InvokeTool(reader)
                // and we dont have to worry about any of the logic, we simply tell the registry to get the right id class and call InvokeTool(), which has logic to handle the expected data format and apply it

                if (dataOwner == 255)
                {
                    clLogger.LogInfo($"Recieved packet from SERVER: [type: {dataType}, len: {dataLen}, data: {reader.ReadBytes(dataLen)[0]}]");
                    continue;
                }

                int _x = reader.ReadInt32();
                int _y = reader.ReadInt32();

                // Reset drawing index
                Main.mouseLerp = 0f;
                Main.otherMousePos.Push(new Point(_x, _y));
                packetCount++;
                clLogger.LogInfo($"Recieved packet: [type: {dataType}, len: {dataLen}, owner: {dataOwner}, no: {packetCount}]", true);

                // This is the future plan. Have it handled dynamically
                if (NetRegistry.TryGet(dataType, out var e))
                {
                    e.RecieveNetCall(dataOwner, reader);
                }
            }
            clLogger.LogInfo($"Server disconnected.");
            return;
        }


        public static void Update()
        {

        }







    }
}