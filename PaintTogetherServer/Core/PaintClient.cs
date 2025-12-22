using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core
{
    /// <summary>
    /// Represents a connected client.
    /// </summary>
    public class PaintClient(TcpClient _tcp, uint _ID)
    {
        /// <summary>
        /// The actual TCP connection to this client
        /// </summary>
        public TcpClient tcp = _tcp;
        
        /// <summary>
        /// The username specified by this client when joining. Represents this client to users
        /// </summary>
        public string UserName;

        /// <summary>
        /// The ID generated for this client by the server. This ID is trustworthy and is the final authority on who this client is.
        /// </summary>
        public uint ID = _ID;

        /// <summary>
        /// Shortcut to the the IP address of this client
        /// </summary>
        public string? ip => ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
        
        /// <summary>
        /// Shortcut to get the stream of this client
        /// </summary>
        public NetworkStream Stream => tcp.GetStream();
    }
}