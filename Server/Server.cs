using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        private readonly Socket serverSocket;
        private static List<ClientThread> clientsList;
        private static List<string> history;

        public Server(string host, int port, int backlog)
        {
            clientsList = new List<ClientThread>();
            history = new List<string>();
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(host), port);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipPoint);
            serverSocket.Listen(backlog);
            Console.WriteLine("Server ready");
        }

        public void ConnectingClients()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                Thread newClient = new Thread(new ParameterizedThreadStart(AcceptClient));
                newClient.Start(clientSocket);
            }
        }

        private void AcceptClient(object clientObj)
        {
            Socket client = (Socket)clientObj;
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = client.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (client.Available > 0);
            clientsList.Add(new ClientThread(client, builder.ToString()));
            Thread singleClient = new Thread(new ThreadStart(clientsList[clientsList.Count - 1].StartClientListening));
            singleClient.Start();
        }

        public static void SendHistory(ClientThread newClient)
        {
            foreach (string message in history)
            {
                clientsList.Find(client => client.Equals(newClient)).WriteMessage(message);
            }
        }

        public static void SendMessageToOthers(ClientThread sdClient, string message)
        {
            history.Add($"{DateTime.Now.ToShortTimeString()} : {sdClient.ClientName} : {message}\n");
            foreach (ClientThread client in clientsList)
            {
                if (client.Equals(sdClient) || message.Equals(string.Empty))
                {
                    continue;
                }
                client.WriteMessage(sdClient.ClientName + " : " + message);
            }
        }

        public static void RemoveClient(ClientThread rmClient)
        {
            clientsList.Remove(rmClient);
            foreach(ClientThread client in clientsList)
            {
                Console.WriteLine(client.ClientName);
            }
        }
    }

    class Launcher
    {
        static void Main()
        {
            Server server = new Server("127.0.0.1", 1234, 3);
            server.ConnectingClients();
        }
    }
}
