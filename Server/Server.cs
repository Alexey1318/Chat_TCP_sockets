using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(host), port);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipPoint);
                serverSocket.Listen(backlog);
                Console.WriteLine("Server ready");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                if (serverSocket.Connected)
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                    serverSocket.Close();
                }
            }
        }

        public void ConnectingClients()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept();
                    Thread newClient = new Thread(new ParameterizedThreadStart(AcceptClient));
                    newClient.Start(clientSocket);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                }
            }
        }

        private void AcceptClient(object clientObj)
        {
            if (clientObj is Socket client)
            {
                try
                {
                    ClientThread clientThread = new ClientThread(client);
                    Thread newClient = new Thread(new ThreadStart(clientThread.StartClientListening));
                    newClient.Start();

                    //StringBuilder builder = new StringBuilder();
                    //int bytes = 0;
                    //byte[] data = new byte[256];
                    //do
                    //{
                    //    bytes = client.Receive(data);
                    //    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    //} while (client.Available > 0);
                    //string clientName = builder.ToString();
                    //while (clientsList.Contains(clientsList.Find(cl => cl.ClientName == clientName)))
                    //{
                    //    byte[] data = Encoding.Unicode.GetBytes($"Client with name {clientName} already connected. Enter ");
                    //    client.Send
                    //}
                    //clientsList.Add(new ClientThread(client, clientName));
                    //Thread singleClient = new Thread(new ThreadStart(clientsList[clientsList.Count - 1].StartClientListening));
                    //singleClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("Something went wrong. Client wasn't connected");
            }
        }

        public static bool CheckClient(string name)
        {
            return clientsList.Contains(clientsList.Find(cl => cl.ClientName == name));
        }

        public static void AddClient(ClientThread newClient)
        {
            clientsList.Add(newClient);
        }

        public static void SendHistory(ClientThread newClient)
        {
            if (newClient.Equals(null)) 
            {
                Console.WriteLine($"Something went wrong with {newClient.ClientName}! Can't send him message history.");
                throw new ArgumentNullException("Got null-value in ClientThread argument");
            }
            else
            {
                ClientThread client = clientsList.Find(cl => cl.Equals(newClient));
                client.WriteMessage(Encoding.Unicode.GetBytes("\nActive users: "));
                foreach (ClientThread clientTh in clientsList)
                {
                    client.WriteMessage(Encoding.Unicode.GetBytes($"{clientTh.ClientName}; "));
                }
                client.WriteMessage(Encoding.Unicode.GetBytes("\nMessage history:\n"));
                foreach (string message in history)
                {
                    client.WriteMessage(Encoding.Unicode.GetBytes($"{message}\n"));
                }
            }
        }

        public static void SendMessageToOthers(ClientThread sdClient, string message)
        {
            history.Add($"[{DateTime.Now.ToShortTimeString()}]{sdClient.ClientName}: {message}");
            Console.WriteLine(history[history.Count - 1]);
            foreach (ClientThread client in clientsList)
            {
                if (client.Equals(sdClient))
                { 
                    continue;
                }
                client.WriteMessage(Encoding.Unicode.GetBytes(history[history.Count - 1]));
            }
        }

        public static void RemoveClient(ClientThread rmClient)
        {
            clientsList.Remove(rmClient);
            Console.WriteLine("\nActive users:");
            foreach (ClientThread client in clientsList)
            {
                Console.Write($"{client.ClientName}; ");
            }
        }
    }

    class Launcher
    {
        static void Main(string[] args)
        {
            Server server = new Server(args[0], int.Parse(args[1]), 3);
            server.ConnectingClients();
        }
    }
}
