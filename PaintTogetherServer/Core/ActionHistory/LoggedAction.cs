using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core.ActionHistory
{
    /// <summary>
    /// Represents some action a client performed that in some-way modifies the canvas state, and thus is required for catching up later-joining clients.
    /// </summary>
    /// <param name="_EntryNum">Entry ID in the event list</param>
    /// <param name="_TimeStamp">timestamp for when this action was logged by the server</param>
    /// <param name="_Packet">the InfoPacket this is based on</param>
    /// <param name="_Owner">the ID of the user who performed this action</param>
    public class LoggedAction(ulong _EntryNum, DateTime _TimeStamp, InfoPacket _Packet, byte _Owner)
    {
        readonly ulong EntryNum = _EntryNum;

        readonly DateTime TimeStamp = _TimeStamp;

        readonly InfoPacket Packet = _Packet;

        readonly byte Owner = _Owner;
    }
}