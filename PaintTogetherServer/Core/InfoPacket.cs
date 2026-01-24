using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core
{
    /// <summary>
    /// Represents an operation or information sent by a connected client.
    /// </summary>
    /// <param name="_OwnerID">The ID of this client found in <see cref="Program.Clients"/></param>
    /// <param name="_Type">What type of packet this is</param>
    /// <param name="_Data">The actual data inside of this packet</param>
    public sealed class InfoPacket(byte _OwnerID, byte[] _Type, byte[] _Data, byte[]? _Blacklist = null)
    {
        /// <summary>
        /// The ID in <see cref="Program.Clients"/> of this client. <br/>
        /// Because this comes from the server's list of who is who, this value is always authoratitvley true.
        /// </summary>
        public readonly byte OwnerID = _OwnerID;

        /// <summary>
        /// What this packet actually is. Ex: Painttogether.tools.lineTool, PaintTogether.undosytem.undoAction, ect
        /// Stored as a byte array for networking purposes but is really a string
        /// </summary>
        public readonly byte[] Type = _Type;

        /// <summary>
        /// How long the <see cref="Data"/> array is.
        /// </summary>
        public int Length => Data.Length;

        /// <summary>
        /// The actual data inside of this packet. Tool parameters, mouse positions, ect
        /// </summary>
        public readonly ReadOnlyMemory<byte> Data = new ReadOnlyMemory<byte>(_Data);

        /// <summary>
        /// List of ClientID's to NOT SEND this packet to.
        /// </summary>
        public readonly List<byte> ClientBlacklist = new List<byte>(_Blacklist ??= []);

        /// <summary>
        /// Blacklist a user so they DO NOT recieve this packet
        /// </summary>
        public void BlacklistUser(byte ClientID) => ClientBlacklist.Add(ClientID);
    }
}