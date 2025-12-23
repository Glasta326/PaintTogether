using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaintTogetherServer.Core;
using PaintTogetherServer.Common.SvLogger;

namespace PaintTogetherServer.Common.Utilities
{
    public static class NetUtils
    {
        /// <summary>
        /// Broadcasts a packet created by the server to every client
        /// </summary>
        public static void BroadcastServerPacket(CommonKeys.ServerPacketTypes _packetType, byte[] _data)
        {
            WorkerThread.WorkQueue.Add(new InfoPacket(CommonKeys.ServerPacketID, (byte)_packetType, _data));
            SvLogger.SvLogger.LogInfo($"Server packet was broadcast: [{_packetType}]", true);
        }

        /// <summary>
        /// Sends a packet created by the server to a specified client
        /// </summary>
        public static void SendServerPacket(CommonKeys.ServerPacketTypes _packetType, PaintClient _target, byte[] _data)
        {
            BinaryWriter writer = new BinaryWriter(_target.Stream, System.Text.Encoding.UTF8, true);

            writer.Write(CommonKeys.ServerPacketID);
            writer.Write((byte)_packetType);
            writer.Write(_data.Length);
            writer.Write(_data);
            writer.Flush();

            SvLogger.SvLogger.LogInfo($"Server packet [{_packetType}] was sent to [{_target.ID}]", true);
        }
    }
}