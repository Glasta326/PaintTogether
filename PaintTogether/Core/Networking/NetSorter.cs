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

        public static string MyUsername { get; set; }

        private static Thread SenderThread;

        private static Thread ReaderThread;

        private static CancellationTokenSource cts = new CancellationTokenSource();

        private static int PacketsToCatchupOn = 0;

        public static bool IsCatchingUp => PacketsToCatchupOn > 0;

        /// <summary>
        /// True when this client is activley connected to a server and ready to send packets.
        /// </summary>
        public static bool IsConnected => !cts.IsCancellationRequested && !IsCatchingUp;

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

                if (packet.Type.Length > 1)
                {
                    clLogger.LogInfo($"Sent packet [Type: {packet.Type}]");
                }
            }

            clLogger.LogInfo($"Server disconnected.");
            return;
        }

        public static ConcurrentQueue<RecievePacket> IncomingPackets = new ConcurrentQueue<RecievePacket>();
        public static void ReadFromServer()
        {
            clLogger.LogInfo($"Reader thread started");
            BinaryReader reader = new BinaryReader(Client.GetStream(), System.Text.Encoding.UTF8, true);
            int packetCount = 0;
            try
            {
                while (Client.Connected && !cts.IsCancellationRequested)
                {
                    byte dataOwner = reader.ReadByte();
                    byte typeLength = reader.ReadByte();
                    string packetType = Encoding.UTF8.GetString(reader.ReadBytes(typeLength));
                    int dataLen = reader.ReadInt32();
                    byte[] data = reader.ReadBytes(dataLen);
                    RecievePacket thisPacket = new RecievePacket(dataOwner, packetType, data);
                    IncomingPackets.Enqueue(thisPacket);
                }
            }
            catch (System.IO.EndOfStreamException)
            {
                clLogger.LogInfo($"Disconnected from server.");
            }

            return;
        }

        // Deque 1 packet per frame.
        // This SUCKS but is the only real option because Undo() packets rely on having already drawn something.
        // Basically, what can happen is the program recieves a draw() packet, and then an undo() packet
        // however, it's impossible to know what to undo() untill the drawpacket has been actually drawn
        // also what can happen is the draw() is applied, but isnt actually drawn untill the next frame, where in that time the undo() packet was executed on the thread

        // Potentially this could be fixed by simply just deleting the previous undoable packet if it hasn't already been drawn somehow
        // But otheriwse this is the only option
        public static void DequeueMostRecentPacket()
        {
            if (!IncomingPackets.TryDequeue(out var thisPacket))
            {
                return;
            }
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
                if (!ReadSpecialPacket(thisPacket))
                {
                    clLogger.LogWarning($"Recieved unknown packet: [Type: {thisPacket.Type}, Owner: {thisPacket.Owner}, Data length: {thisPacket.ByteData.Length}]");
                }
            }

            if (PacketsToCatchupOn > 0)
            {
                if (PacketsToCatchupOn > 0)
                {
                    clLogger.LogInfo($"Catching up. {PacketsToCatchupOn} Packets left.", true);
                }
                else
                {
                    clLogger.LogInfo($"Catchup complete.", true);
                }
                PacketsToCatchupOn--;
            }
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
                    string readUsername = reader.ReadString();

                    // ignore us
                    // TODO: this might be why we arent getting the reconnected flah
                    if (clientID == Myself.ClientID)
                    {
                        break;
                    }

                    // Server announces a user we've seen before so we can just re-flag the "IsConnected" bool
                    if (PaintUser.UserRegistry.TryGetValue(clientID, out PaintUser existingUser))
                    {
                        existingUser.IsConnected = true;
                        clLogger.LogInfo($"[ID: {existingUser.ClientID}, GUID: {existingUser.UserID}, Username: {existingUser.UserName}] has joined.");

                        // Update username if changed.
                        if (readUsername != existingUser.UserName)
                        {
                            clLogger.LogInfo($"[ID: {existingUser.ClientID}, GUID: {existingUser.UserID}, Username: {existingUser.UserName}] Changed username to {readUsername}");
                            existingUser.UpdateUsername(readUsername);
                        }
                    }
                    // Brand new user
                    else
                    {
                        _ = new PaintUser(userGuid, clientID, readUsername);
                        clLogger.LogInfo($"Created new user: [ID: {clientID}, GUID: {userGuid}, Username: {readUsername}]");
                    }
                    break;

                case CommonKeys.ServerPacketTypes.AnnounceUserDisconnecting:
                    clientID = reader.ReadByte();

                    // This was causing issues when catching up as we'd read the part where the sever logged that we disconnected 
                    if (clientID == Myself.ClientID)
                    {
                        break;
                    }
                    if (PaintUser.UserRegistry.TryGetValue(clientID, out PaintUser leavingUser))
                    {
                        leavingUser.IsConnected = false;
                        clLogger.LogInfo($"[ID: {leavingUser.ClientID}, GUID: {leavingUser.UserID}, Username: {leavingUser.UserName}] has left.");
                    }
                    break;

                case CommonKeys.ServerPacketTypes.AnnounceServerClosing:
                    cts.Cancel();
                    clLogger.LogInfo("Server closed.");
                    break;

                case CommonKeys.ServerPacketTypes.WhisperInformClientID:
                    // this might be resetting history!!!
                    // TODO : look into this
                    clientID = reader.ReadByte();
                    Myself = new PaintUser(MyGuid, clientID, MyUsername);
                    clLogger.LogInfo($"Created myself as: [ID: {clientID}, GUID: {MyGuid}, Username: {MyUsername}]");

                    // Create catchup request now we've logged in
                    SendPacket catchup = NetUtils.CreateSpecialPacket(CommonKeys.SpecialPacketTypes.CatchupRequest, []);
                    OutgoingPackets.Add(catchup);
                    break;

                case CommonKeys.ServerPacketTypes.WhisperInformCatchupBegin:
                    int packets = reader.ReadInt32();
                    clLogger.LogInfo($"Catching up with server. {packets} Packets to catch up on.");
                    PacketsToCatchupOn = packets;
                    break;

                default:
                    clLogger.LogWarning($"Recieved unknown server packet: [Type: {thisPacket.Type}, Owner: {thisPacket.Owner}, Data length: {thisPacket.ByteData.Length}]");
                    break;

            }

        }

        public static bool ReadSpecialPacket(RecievePacket thisPacket)
        {
            BinaryReader reader = new BinaryReader(thisPacket.GetStream());
            PaintUser owner = PaintUser.UserRegistry[thisPacket.Owner];

            switch (thisPacket.Type)
            {
                case CommonKeys.SpecialPacketTypes.UndoAction:
                    owner.ActionBuffer.Enqueue(owner.UndoMostRecent);
                    clLogger.LogInfo($"Recieved UNDO packet for: [ClientID: {thisPacket.Owner}, Username: {owner.UserName}]");
                    return true;

                case CommonKeys.SpecialPacketTypes.RedoAction:
                    owner.ActionBuffer.Enqueue(owner.RedoMostRecent);
                    clLogger.LogInfo($"Recieved REDO packet for: [ClientID: {thisPacket.Owner}, Username: {owner.UserName}]");
                    return true;

                case CommonKeys.SpecialPacketTypes.LayerAdd:

                    return true;

                case CommonKeys.SpecialPacketTypes.LayerDelete:

                    return true;

                default:
                    clLogger.LogInfo($"Failed to read special packet: {thisPacket.Type}");
                    return false;
            }

            return false;
        }






    }
}