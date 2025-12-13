using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core
{
    public struct InfoPacket
    {
        public object[] data;

        public int owner;

        public InfoPacket(int _owner, object[] _data)
        {
            owner = _owner;
            data = _data;
        }
    }
}