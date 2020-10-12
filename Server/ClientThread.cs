using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class ClientThread
    {
        public string ClientName { get; private set; }
        private readonly Socket clientSocket;

        private delegate void GetHistory(ClientThread client);
        private delegate void ResendMessage(ClientThread client, string message);
        private delegate void ClientReporter(ClientThread client);
        private event GetHistory ChatHistory;
        private event ResendMessage SendToOthers;
        private event ClientReporter ServerReport;

        public ClientThread(Socket socket, string name)
        {
            ClientName = name;
            clientSocket = socket;
            SendToOthers += Server.SendMessageToOthers;
            ServerReport += Server.RemoveClient;
            ChatHistory += Server.SendHistory;
        }

        public void StartClientListening()
        {
            var threadReader = Task.Run(() => ReadMessage());
            ChatHistory(this);
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
<<<<<<< HEAD
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + "\n" + clientMessage);
                    SendToOthers(this, clientMessage);
=======
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + " : " + clientName +" : " + clientMessage);
                    sendToOthers(this, clientMessage);
>>>>>>> 53b4b26e2062d5598f7756f1732103a19f502249
                    if (clientMessage == string.Empty)
                    {
                        throw new Exception("empty message was received; disconnecting client");
                    }
                    builder.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
<<<<<<< HEAD
                ServerReport(this);
=======
                sendToOthers(this, "disconnected");
                serverReport(this);
>>>>>>> 53b4b26e2062d5598f7756f1732103a19f502249
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
                Console.WriteLine(e.Message);
<<<<<<< HEAD
                //sendToOthers(this, "disconnected");
                ServerReport(this);
=======
                sendToOthers(this, "disconnected");
                serverReport(this);
>>>>>>> 53b4b26e2062d5598f7756f1732103a19f502249
            }
        }
    }
}
