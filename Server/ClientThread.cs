using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class ClientThread
    {
        public string ClientName { get; }
        private readonly Socket clientSocket;

        private Action<ClientThread> GetMessageHistory;
        private Action<ClientThread> DisconnectClient;
        private Action<ClientThread, string> ClientToClients;

        public ClientThread(Socket socket, string name)
        {
            ClientName = name;
            clientSocket = socket;
            ClientToClients += Server.SendMessageToOthers;
            DisconnectClient += Server.RemoveClient;
            GetMessageHistory += Server.SendHistory;
        }

        public void StartClientListening()
        {
            try
            {
                var threadReader = Task.Run(() => ReadMessage());
                GetMessageHistory(this);
                ClientToClients(this, "connected");
            }
            catch(ArgumentNullException e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                throw;
            }
        }

        private void ReadMessage()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                while (true)
                {
                    // получить текстовое сообщение
                    int bytes = 0;
                    byte[] data = new byte[256];
                    do
                    {
                        bytes = clientSocket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (clientSocket.Available > 0);
                    // привести его к строке
                    string clientMessage = builder.ToString();
                    builder.Clear();
                    // если пришло 'exit' - сообщить остальным слушателям об отключении
                    // и отключить этого клиента; прервть цикл приема сообщений
                    if (clientMessage.Equals("exit"))
                    {
                        ClientToClients(this, "disconnected");
                        DisconnectClient(this);
                        break;
                    }
                    // выводит полученное сообщение на СЕРВЕРНУЮ часть
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]{ClientName} {clientMessage}");
                    // переслать остальным клиентам полученное сообщение
                    ClientToClients(this, clientMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ClientToClients(this, "disconnected");
                DisconnectClient(this);
            }
        }

        public void WriteMessage(string mes)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(mes);
                if (clientSocket.Connected)
                    clientSocket.Send(data);
                else
                    throw new Exception("client couldn't get message");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                ClientToClients(this, "disconnected");
                DisconnectClient(this);
            }
        }
    }
}
