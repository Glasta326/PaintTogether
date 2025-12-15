using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core
{
    public struct InfoPacket
    {
        public byte[] Data;

        public uint OwnerID;

        public InfoPacket(uint _ownerID, byte[] _Data)
        {
            OwnerID = _ownerID;
            Data = _Data;
        }
    }
}