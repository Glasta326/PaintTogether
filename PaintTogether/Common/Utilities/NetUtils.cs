using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PaintTogether.Core.Networking;

namespace PaintTogether.Common.Utilities
{
    public static class NetUtils
    {
        public static SendPacket CreateSpecialPacket(string type, byte[] data)
        {
            SendPacket packet = new SendPacket(NetSorter.Myself.ClientID, type);
            BinaryWriter writer = new BinaryWriter(packet.GetStream());
            writer.Write(data);
            return packet;
        }

        /// <summary>
        /// Creates a <see cref="SendPacket"/> and automatically pushes it to <see cref="NetSorter.OutgoingPackets"/>
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public static void QuickSendPacket(byte owner, string type, byte[] data = null)
        {
            SendPacket packet = new SendPacket(owner, type);
            if (data is not null)
            {
                BinaryWriter writer = new BinaryWriter(packet.GetStream());
                writer.Write(data);
            }
            NetSorter.OutgoingPackets.Add(packet);
        }
    }
}