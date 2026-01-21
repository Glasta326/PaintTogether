using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PaintTogether.Common;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Core.Networking.Registry;
using PaintTogether.Core.Users;

namespace PaintTogether.Core.Networking
{
    /// <summary>
    /// Handles all Incoming and outgoing network traffic, and delegates incoming commands to the appropriate function
    /// </summary>
    public static class NetSorter
    {
        public static TcpClient Client { get; private set; }

        public static PaintUser Myself { get; private set; }

        public static Guid MyGuid { get; set; }

        public static String MyUsername { get; set; }

        private static Thread SenderThread;

        private static Thread ReaderThread;

        private static CancellationTokenSource cts = new CancellationTokenSource();

        /// <summary>
        /// True when this client is activley connected to a server.
        /// </summary>
        public static bool IsConnected => !cts.IsCancellationRequested;

        // Create our tcp client and initalise both network threads
        public static bool Init()
        {

            // Attempt to connect to server
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
            catch (System.Net.Sockets.SocketException)
            {
                clLogger.LogInfo($"Server [IP: {ip}, Port: {12504}] was not found. Running in offline mode.");
                Client?.Dispose();
                cts.Cancel();

                // Not connected to any server so we're running offline, just use ID 0
                Myself = new PaintUser(MyGuid, 0, MyUsername);

                return false;
            }
            catch (System.IO.EndOfStreamException)
            {
                clLogger.LogInfo($"Server [IP: {ip}, Port: {12504}] was closed.");
                return false;
            }
            return true;
        }

        public static BlockingCollection<SendPacket> OutgoingPackets = new(new ConcurrentQueue<SendPacket>());
        public static void SendToServer()
        {
            clLogger.LogInfo($"Sender thread started");
            BinaryWriter writer = new BinaryWriter(Client.GetStream(), System.Text.Encoding.UTF8, true);

            // Preliminary data the server needs from us or we will get force-disconnected.
            writer.Write(LoggableData.ClientVersionInfo());
            writer.Flush();

            writer.Write(MyGuid.ToString());
            writer.Flush();

            writer.Write(MyUsername.ToString());
            writer.Flush();

            foreach (SendPacket packet in OutgoingPackets.GetConsumingEnumerable())
            {
                writer.Write((byte)packet.Type.Length);
                writer.Write(packet.Type);
                writer.Write(packet.ByteData.Length);
                writer.Write(packet.ByteData);
                writer.Flush();
            }

            clLogger.LogInfo($"Server disconnected.");
            return;
        }

        public static void ReadFromServer()
        {
            clLogger.LogInfo($"Reader thread started");
            BinaryReader reader = new BinaryReader(Client.GetStream(), System.Text.Encoding.UTF8, true);
            int packetCount = 0;
            while (Client.Connected && !cts.IsCancellationRequested)
            {
                byte dataOwner = reader.ReadByte();
                byte typeLength = reader.ReadByte();
                string packetType = Encoding.UTF8.GetString(reader.ReadBytes(typeLength));
                int dataLen = reader.ReadInt32();
                byte[] data = reader.ReadBytes(dataLen);
                RecievePacket thisPacket = new RecievePacket(dataOwner, packetType, data);

                // Handle server-sourced packets seperately
                if (thisPacket.Owner == CommonKeys.ServerPacketID)
                {
                    ReadServerPacket(thisPacket);
                }

                // Attempt to find the tool or other class for this packet type and invoke it's method
                else if (NetRegistry.TryGet(thisPacket.Type, out INetApplicable e))
                {
                    e.RecieveNetCall(thisPacket);
                }

                else
                {
                    clLogger.LogWarning($"Recieved unknown packet: [Type: {thisPacket.Type}, Owner: {thisPacket.Owner}, Data length: {thisPacket.ByteData.Length}]");
                }


                continue;


                // Ideally now, we have WHO owns the data, if its the server we have custom logic, but otherwise we ideally do something like:
                // NetapplicableRegistry[dataType].InvokeTool(reader)
                // and we dont have to worry about any of the logic, we simply tell the registry to get the right id class and call InvokeTool(), which has logic to handle the expected data format and apply it

                if (dataOwner == 255)
                {
                    clLogger.LogInfo($"Recieved packet from SERVER: [type: {2}, len: {dataLen}, data: {reader.ReadBytes(dataLen)[0]}]");
                    continue;
                }

                int _x = reader.ReadInt32();
                int _y = reader.ReadInt32();

                // Reset drawing index
                Main.mouseLerp = 0f;
                Main.otherMousePos.Push(new Point(_x, _y));
                packetCount++;
                clLogger.LogInfo($"Recieved packet: [type: {packetType}, len: {dataLen}, owner: {dataOwner}, no: {packetCount}]", true);


            }
            clLogger.LogInfo($"Server was disconnected.");
            return;
        }


        public static void ReadServerPacket(RecievePacket thisPacket)
        {
            byte packetType = (byte)thisPacket.Type[0];

            BinaryReader reader = new BinaryReader(thisPacket.GetStream());

            byte clientID;

            switch ((CommonKeys.ServerPacketTypes)packetType)
            {
                case CommonKeys.ServerPacketTypes.RejectServerConnectionLimitReached:

                    break;

                case CommonKeys.ServerPacketTypes.RejectBadGUID:

                    break;

                case CommonKeys.ServerPacketTypes.RejectVersionMismatch:

                    break;

                case CommonKeys.ServerPacketTypes.RejectUserAlreadyConnected:

                    break;

                case CommonKeys.ServerPacketTypes.RejectUserUnknown:

                    break;

                case CommonKeys.ServerPacketTypes.RequestUsername:

                    break;

                case CommonKeys.ServerPacketTypes.AnnounceUserConnecting:
                    clientID = reader.ReadByte();
                    string readGuid = reader.ReadString();
                    Guid userGuid = new Guid(readGuid);
                    string username = reader.ReadString();

                    PaintUser newUser = new PaintUser(userGuid, clientID, username);
                    clLogger.LogInfo($"Created new user: [ID: {clientID}, GUID: {userGuid}, Username: {username}]");
                    break;

                case CommonKeys.ServerPacketTypes.AnnounceUserDisconnecting:
                    clientID = reader.ReadByte();
                    if (PaintUser.UserRegistry.TryGetValue(clientID, out PaintUser value))
                    {
                        value.IsConnected = false;
                    }
                    break;

                case CommonKeys.ServerPacketTypes.AnnounceServerClosing:
                    cts.Cancel();
                    break;

                case CommonKeys.ServerPacketTypes.WhisperInformClientID:
                    clientID = reader.ReadByte();
                    Myself = new PaintUser(MyGuid, clientID, MyUsername);
                    clLogger.LogInfo($"Created myself as: [ID: {clientID}, GUID: {MyGuid}, Username: {MyUsername}]");
                    break;

                default:
                    clLogger.LogWarning($"Recieved unknown server packet: [Type: {thisPacket.Type}, Owner: {thisPacket.Owner}, Data length: {thisPacket.ByteData.Length}]");
                    break;

            }

        }

        public static void Update()
        {

        }







    }
}