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
    class StartServer
    {
        public static void Main(string[] args)
        {
            ServerBis server = new ServerBis(args[0], int.Parse(args[1]), 3);
            server.ConnectingClients();
        }
    }
    class ServerBis
    {
        private readonly Socket serverSocket;
        private static List<ServerRoom> roomList;

        public ServerBis(string host, int port, int backlog)
        {
            roomList = new List<ServerRoom>() { new ServerRoom("main_room") };
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
            foreach(ServerRoom room in roomList)
            {
                if(room.Participants.Contains(room.Participants.Find(user => user.ClientName == name)))
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddClient(ClientThread newClient)
        {
            if (newClient.Equals(null))
            {
                Console.WriteLine($"Something went wrong with {newClient.ClientName}! Can't send him message history.");
                throw new ArgumentNullException("Got null-value in ClientThread argument");
            }
            else if (newClient.ClientSocket.Connected)
            {
                ServerRoom tmpRoom = roomList.Find(room => room.RoomName == "main_room");
                tmpRoom?.Participants.Add(newClient);
            }
        }

        public static void CreateRoom(ClientThread client, string roomName)
        {
            if (!roomList.Contains(roomList.Find(r => r.RoomName == roomName)))
            {
                roomList.Add(new ServerRoom(roomName));
            }
            roomList.Find(r => r.RoomName == roomName).Participants.Add(client);
            foreach (ServerRoom room in roomList)
            {
                if (room.RoomName == roomName)
                {
                    continue;
                }
                room.Participants.Remove(client);
            }
            SendHistory(client);
        }

        public static void SendHistory(ClientThread newClient)
        {
            if (newClient.Equals(null))
            {
                Console.WriteLine($"Something went wrong with {newClient.ClientName}! Can't send him message history.");
                throw new ArgumentNullException("Got null-value in ClientThread argument");
            }
            else if (newClient.ClientSocket.Connected)
            {
                ServerRoom room = roomList.Find(r => r.RoomName == newClient.ClientRoom);
                ClientThread client = room.Participants.Find(cl => cl.Equals(newClient));
                client.WriteMessage(Encoding.Unicode.GetBytes("\nActive users: "));
                foreach (ClientThread clientTh in room.Participants)
                {
                    client.WriteMessage(Encoding.Unicode.GetBytes($"{clientTh.ClientName}; "));
                }
                client.WriteMessage(Encoding.Unicode.GetBytes("\nMessage history:\n"));
                foreach (string message in room.RoomMessageHistroy)
                {
                    client.WriteMessage(Encoding.Unicode.GetBytes($"{message}\n"));
                }
            }
        }

        public static void SendMessageToOthers(ClientThread sdClient, string message)
        {
            if (sdClient.ClientSocket.Connected)
            {
                ServerRoom room = roomList.Find(r => r.RoomName == sdClient.ClientRoom);
                room.RoomMessageHistroy.Add($"{DateTime.Now.ToShortTimeString(), 8} {sdClient.ClientRoom, 10} {sdClient.ClientName, 10}: {message}");
                Console.WriteLine(room.RoomMessageHistroy[room.RoomMessageHistroy.Count - 1]);
                foreach (ClientThread client in room.Participants)
                {
                    if (client.Equals(sdClient))
                    {
                        continue;
                    }
                    client.WriteMessage(Encoding.Unicode.GetBytes(room.RoomMessageHistroy[room.RoomMessageHistroy.Count - 1]));
                }
            }
        }

        public static void RemoveClient(ClientThread rmClient)
        {
            // ! список активных пользователей выводить по команде админа
            ServerRoom room = roomList.Find(r => r.RoomName == rmClient.ClientRoom);
            if (room.Participants.Remove(rmClient))
            {
                Console.WriteLine("\nActive users:");
                foreach (ClientThread client in room.Participants)
                {
                    Console.Write($"{client.ClientName}; ");
                }
            }
        }
    }
}
