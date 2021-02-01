using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientThread
    {
        public readonly string ClientName;
        private readonly Socket clientSocket;

        private readonly Action<ClientThread> GetMessageHistory;
        private readonly Action<ClientThread> DisconnectClient;
        private readonly Action<ClientThread, string> SendToOthers;

        public ClientThread(Socket socket, string name)
        {
            ClientName = name;
            clientSocket = socket;
            SendToOthers += Server.SendMessageToOthers;
            DisconnectClient += Server.RemoveClient;
            GetMessageHistory += Server.SendHistory;
        }

        public void StartClientListening()
        {
            try
            {
                var readerTh = Task.Run(() => ReadMessage());
                GetMessageHistory(this);
                SendToOthers(this, "connected");
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
                    int bytes = 0;
                    byte[] data = new byte[256];
                    do
                    {
                        bytes = clientSocket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (clientSocket.Available > 0);
                    string clientMessage = builder.ToString();
                    builder.Clear();
                    if (clientMessage.Equals("exit"))
                    {
                        SendToOthers(this, "disconnected");
                        DisconnectClient(this);
                        break;
                    }
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}]{ClientName} {clientMessage}");
                    SendToOthers(this, clientMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                SendToOthers(this, "disconnected");
                DisconnectClient(this);
            }
        }

        public void WriteMessage(string mes)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(mes);
                if (clientSocket.Connected)
                {
                    clientSocket.Send(data);
                }
                else
                {
                    throw new Exception("client couldn't get message");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                SendToOthers(this, "disconnected");
                DisconnectClient(this);
            }
        }
    }
}
