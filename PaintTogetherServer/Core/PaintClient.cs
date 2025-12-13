using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core
{
    public class PaintClient
    {
        public TcpClient tcp;
        public int? name;
        public string? ip;
        public NetworkStream stream => tcp.GetStream();

        public PaintClient(TcpClient _client)
        {
            tcp = _client;
        }
    }
}