using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        const int port = 1234;
        static void Main(string[] args)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                serverSocket.Bind(ipPoint);
                serverSocket.Listen(10);
                Console.WriteLine("Server ready");
                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();
                    Thread newClient = new Thread(new ParameterizedThreadStart(ConnectingClient));
                    newClient.Start(clientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static void ConnectingClient(Object clientObj)
        {
            /*
             * 1) get client name
             * 2) add him to list of clients thread
             * 3) registry it in listeners
             * 4) start client thread
             * 5) write line on console about method status 
             */
            // 1) get client name
            Socket client = (Socket)clientObj;
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = client.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (client.Available > 0);
            string name = builder.ToString();
            // 2) add him to list of clients thread
            // 3) registry it in listeners
            // 4) start client thread
            // 5) write line on console about method status
            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + name + " connected");
        }
    }
}
