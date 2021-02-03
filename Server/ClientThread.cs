using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class ClientThread
    {
        public string ClientName { get; private set; }
        private readonly Socket clientSocket;

        private readonly Action<ClientThread> GetMessageHistory;
        private readonly Action<ClientThread> ConnectClient;
        private readonly Action<ClientThread> DisconnectClient;
        private readonly Action<ClientThread, string> SendToOthers;
        private readonly Func<string, bool> Check;

        public ClientThread(Socket socket)
        {
            clientSocket = socket;
            SendToOthers += Server.SendMessageToOthers;
            ConnectClient += Server.AddClient;
            DisconnectClient += Server.RemoveClient;
            GetMessageHistory += Server.SendHistory;
            Check += Server.CheckClient;
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public void StartClientListening()
        {
            try
            {
                int bytes = 0;
                byte[] data = new byte[256];
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        bytes = clientSocket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (clientSocket.Available > 0);
                    if (Check(builder.ToString()))
                    {
                        byte[] a = BitConverter.GetBytes(false);
                        foreach(byte b in a)
                        {
                            Console.WriteLine(b);
                        }
                        this.WriteMessage(a);
                    }
                    else 
                    {
                        this.WriteMessage(BitConverter.GetBytes(true));
                        ClientName = builder.ToString(); 
                        ConnectClient(this);
                        var readerTh = Task.Run(() => ReadMessage());
                        GetMessageHistory(this);
                        SendToOthers(this, "connected");
                        break;
                    }
                }
                
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

        public void WriteMessage(byte[] data)
        {
            try
            {
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
