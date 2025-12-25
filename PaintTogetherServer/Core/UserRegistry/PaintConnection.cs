using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core.UserRegistry
{
    public class PaintConnection
    {
        public PaintConnection(TcpClient _tcp)
        {
            tcp = _tcp;
            Writer = new BinaryWriter(tcp.GetStream(), System.Text.Encoding.UTF8, true);
        }

        /// <summary>
        /// The actual TCP connection to this client
        /// </summary>
        public TcpClient tcp;

        // Shut up
#pragma warning disable
        /// <summary>
        /// Shortcut to the the IP address of this client
        /// </summary>
        public string? ip => ((IPEndPoint)tcp.Client.RemoteEndPoint).Address.ToString();
#pragma warning restore

        /// <summary>
        /// Shortcut to get the stream of this client
        /// </summary>
        public NetworkStream Stream => tcp.GetStream();

        public object streamLock = new();

        public BinaryWriter Writer;
    }
}