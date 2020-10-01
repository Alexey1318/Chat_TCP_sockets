using System;
using System.Net.Sockets;
using System.Text;

namespace TcpClientChat
{
    class TcpClientChat
    {
        TcpClient client;
        public TcpClientChat(string host, int port)
        {
            client = new TcpClient();
            client.Connect(host, port);
        }
        public void SendMessage()
        {
            byte[] data = new byte[256];
            StringBuilder response = new StringBuilder();
            NetworkStream stream = client.GetStream();
            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                response.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);
        }
    }
    class Launcher
    {
        static void Main(string[] args)
        {
        }
    }
}
