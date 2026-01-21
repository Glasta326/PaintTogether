using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.Networking
{
    public interface INetApplicable
    {
        /// <summary>
        /// Externally-called method to enqueue some action forwarded from the server into this class. <br/>
        /// Example:
        ///     A recieved packet from the server requesting the line tool to draw between 0,0 and 10,10.
        ///     Then, this method is auto-invoked by the Networker reader thread, and enqueues a draw command into the Line tool to execute when the program runs the next frame
        /// </summary>
        /// <param name="dataPacket"></param>
        public void RecieveNetCall(RecievePacket dataPacket);
    }
}