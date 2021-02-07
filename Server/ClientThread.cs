using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server
{
    class ClientThread
    {
        public string ClientName { get; private set; }
        public string ClientRoom { get; private set; }
        public Socket ClientSocket { get; private set; }

        private readonly Action<ClientThread> GetMessageHistory;
        private readonly Action<ClientThread> ConnectClient;
        private readonly Action<ClientThread> DisconnectClient;
        private readonly Action<ClientThread, string> SendToOthers;
        private readonly Func<string, bool> Check;

        public ClientThread(Socket socket)
        {
            ClientSocket = socket;
            ClientRoom = "main_room";
            SendToOthers += Server.SendMessageToOthers;
            ConnectClient += Server.AddClient;
            DisconnectClient += Server.RemoveClient;
            GetMessageHistory += Server.SendHistory;
            Check += Server.CheckClient;
        }

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
                        bytes = ClientSocket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (ClientSocket.Available > 0);
                    if (Check(builder.ToString()))
                    {
                        WriteMessage(BitConverter.GetBytes(false));
                    }
                    else 
                    {
                        WriteMessage(BitConverter.GetBytes(true));
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

        private void StopClientListening()
        {
            if (ClientSocket != null && ClientSocket.Connected)
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }
        }

        private void ReadMessage()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                while (ClientSocket.Connected)
                {
                    int bytes = 0;
                    byte[] data = new byte[256];
                    do
                    {
                        bytes = ClientSocket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (ClientSocket.Available > 0);
                    string clientMessage = builder.ToString();
                    builder.Clear();
                    if (!CheckCommand(clientMessage))
                    {
                        SendToOthers(this, clientMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                SendToOthers(this, "disconnected");
                DisconnectClient(this);
            }
        }

        public void WriteMessage(byte[] data)
        {
            try
            {
                if (ClientSocket.Connected)
                {
                    ClientSocket.Send(data);
                }
                else
                {
                    throw new Exception("client can't receive the message");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                SendToOthers(this, "disconnected");
                DisconnectClient(this);
            }
        }

        private bool CheckCommand(string text)
        {
            Match matchCom = Regex.Match(text, @"^(c_){1}\w+(($)||(\s+\w+$))");
            if (matchCom.Value.Length > 0)
            {
                Match matchArg = Regex.Match(text, @"\s+\w+$");
                switch (matchCom.Value)
                {
                    case "c_exit":
                        SendToOthers(this, "disconnected");
                        StopClientListening();
                        DisconnectClient(this);
                        return true;
                    case "c_room":
                        string tempRoomName = Regex.Replace(matchArg.Value, @"\s", String.Empty);
                        SendToOthers(this, $"{ClientName} change room to {tempRoomName}");
                        ClientRoom = tempRoomName;
                        SendToOthers(this, $"connected to {ClientRoom}");
                        return true;
                }
            }
            return false;
        }
    }
}
