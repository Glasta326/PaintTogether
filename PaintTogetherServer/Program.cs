using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PaintTogetherServer;



class Program
{
    static async Task Main(string[] args)
    {

        
        TcpListener listener = new TcpListener(IPAddress.Any, 12504);
        listener.Start();
        Console.WriteLine("Server started.");
        Console.WriteLine($"IP: 192.168.0.153");
        Console.WriteLine($"Port: 12504");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            Console.WriteLine($"{ip}");
        }

        
    }


}
