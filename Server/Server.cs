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
            // список клиентов
            clientsList = new List<ClientThread>();
            // история сообщений (для рассылки новым клиентам)
            history = new List<string>();
            // запуск сервера на порту
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(host), port);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipPoint);
            // backlog - максимальная длина очереди ожидаюжих подключений
            serverSocket.Listen(backlog);
            Console.WriteLine("Server ready");
        }

        public void ConnectingClients()
        {
            while (true)
            {
                // В исключении только вывод сообщения, т.к. объекты сокета и потока локальны и, после
                // выхода из блока try (если возникнет исключение), будут удалены сборщиком мусора
                try
                {
                    // ? принять запрос на прослушивание
                    Socket clientSocket = serverSocket.Accept();
                    // запустить побочный поток для подключения нового клиента к чату
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
            // если приведение типов прошло успешно
            if (clientObj is Socket client)
            {
                // получить имя клиента (= регистрация на время сессии)
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                byte[] data = new byte[256];
                do
                {
                    bytes = client.Receive(data);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (client.Available > 0);
                // добавить клиента в список подключенний
                clientsList.Add(new ClientThread(client, builder.ToString()));
                // запустить поток, отвечающий за прием/отправку сообщений одного конкретного клиента
                Thread singleClient = new Thread(new ThreadStart(clientsList[clientsList.Count - 1].StartClientListening));
                singleClient.Start();
            }
            else
            {
                Console.WriteLine("Something went wrong. Client wasn't connected");
            }
        }

        public static void SendHistory(ClientThread newClient)
        {
            // отправить новому пользователю список активных пользователей
            clientsList.Find(cl => cl.Equals(newClient)).WriteMessage("\nActive users: ");
            foreach (ClientThread clientTh in clientsList)
            {
                clientsList.Find(cl => cl.Equals(newClient)).WriteMessage($"{clientTh.ClientName}; ");
            }
            // отправить новому пользователю историю сообщения
            clientsList.Find(cl => cl.Equals(newClient)).WriteMessage("\nMessage history:");
            foreach (string message in history)
            {
                clientsList.Find(client => client.Equals(newClient)).WriteMessage(message + "\n");
            }
        }

        public static void SendMessageToOthers(ClientThread sdClient, string message)
        {
            if (message.Length == 0)
                return;
            history.Add($"[{DateTime.Now.ToShortTimeString()}]{sdClient.ClientName}: {message}");
            foreach (ClientThread client in clientsList)
            {
                if (client.Equals(sdClient) || message.Length == 0)
                {
                    continue;
                }
                client.WriteMessage(history[history.Count - 1]);
            }
        }

        public static void RemoveClient(ClientThread rmClient)
        {
            clientsList.Remove(rmClient);
            Console.WriteLine("Active users:");
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
