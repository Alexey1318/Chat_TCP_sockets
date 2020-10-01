using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class ClientThread
    {
        public string clientName { get; private set; }
        private Socket clientSocket;

        private delegate void ResendMessage(ClientThread client, string message);
        private delegate void ClientReporter(ClientThread client);
        private event ResendMessage sendToOthers;
        private event ClientReporter serverReport;

        public ClientThread(Socket socket, string name)
        {
            clientName = name;
            clientSocket = socket;
            sendToOthers += Server.SendMessageToOthers;
            serverReport += Server.RemoveClient;
        }

        public void StartClientListening()
        {
            var threadReader = Task.Run(() => ReadMessage());
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
                    string clientMessage = clientName + ": " + builder.ToString();
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + "\n" + clientMessage);
                    sendToOthers(this, clientMessage);
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
                serverReport(this);
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
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                serverReport(this);
            }
        }
    }
}
