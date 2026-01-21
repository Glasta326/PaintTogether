using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.Networking
{
    // Represents some data coming from or going to the server
    public sealed class RecievePacket(byte _Owner, string _Type, byte[] _Data)
    {
        // The net ID of who owns this packet
        public byte Owner { get; } = _Owner;

        // The packet descriptor. What this data actually *is*
        public string Type { get; } = _Type;

        // The data inside this packet
        public byte[] ByteData { get; } = _Data;

        private MemoryStream _Stream = new MemoryStream(_Data);

        // Get the MemoryStream to this packet's data
        public MemoryStream GetStream() => _Stream;
    }
}