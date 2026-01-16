using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaintTogetherServer.Core;
using PaintTogetherServer.Common.SvLogger;
using PaintTogetherServer.Core.UserRegistry;

namespace PaintTogetherServer.Common.Utilities
{
    public static class NetUtils
    {
        /// <summary>
        /// Broadcasts a packet created by the server to every client
        /// </summary>
        public static void BroadcastServerPacket(CommonKeys.ServerPacketTypes _packetType, byte[] _data)
        {
            WorkerThread.WorkQueue.Add(new InfoPacket(CommonKeys.ServerPacketID, [(byte)_packetType], _data));
            SvLogger.SvLogger.LogInfo($"Server packet was broadcast: [{_packetType}]", true);
        }

        /// <summary>
        /// Sends a packet created by the server to a specified client
        /// </summary>
        public static void SendServerPacket(CommonKeys.ServerPacketTypes _packetType, PaintUser _target, byte[] _data)
        {
            if (_target.Connection is null)
            {
                SvLogger.SvLogger.LogWarning($"Attempted to send server packet to disconnected user: [ClientID: {_target.ClientID}, UserID: {_target.UserID}]");
                return;
            }
            BinaryWriter writer = new BinaryWriter(_target.Connection.Stream, System.Text.Encoding.UTF8, true);

            writer.Write(CommonKeys.ServerPacketID);
            writer.Write((byte)1); // We are sending packets directly from the server here, which are always one byte long for the packet type
            writer.Write([(byte)_packetType]);
            writer.Write(_data.Length);
            writer.Write(_data);
            writer.Flush();

            SvLogger.SvLogger.LogInfo($"Server packet [{_packetType}] was sent to [ClientID: {_target.ClientID}, UserID: {_target.UserID}]", true);
        }

        // Writes a server packet into the writer. Perferrable use the PaintUser override if possible as this has reduced logging
        public static void SendServerPacket(this BinaryWriter writer, CommonKeys.ServerPacketTypes _packetType, byte[]? _data = null)
        {
            if (!writer.BaseStream.CanWrite)
            {
                SvLogger.SvLogger.LogWarning($"Could not write to stream!");
                return;
            }

            // Because i cant have arrays as optional params
            _data ??= [];

            writer.Write(CommonKeys.ServerPacketID);
            writer.Write((byte)1); // We are sending packets directly from the server here, which are always one byte long for the packet type
            writer.Write([(byte)_packetType]);
            writer.Write(_data.Length);
            writer.Write(_data);
            writer.Flush();
        }
    }
}