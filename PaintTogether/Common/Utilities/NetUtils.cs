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
    }
}