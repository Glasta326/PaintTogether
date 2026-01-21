using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.Networking
{
    public sealed class SendPacket(byte _Owner, string _Type)
    {
        // The net ID of who owns this packet
        public byte Owner { get; } = _Owner;

        // The packet descriptor. What this data actually *is*
        public string Type { get; } = _Type;

        // The data inside this packet
        public byte[] ByteData => _Stream.ToArray();

        private MemoryStream _Stream = new MemoryStream();

        // Get the MemoryStream to this packet's data
        public MemoryStream GetStream() => _Stream;
    }
}