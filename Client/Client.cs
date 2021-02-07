using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Client
{
    class Client
    {
        private readonly Socket clientSocket;
        private readonly Thread sender;
        private readonly Thread receiver;
        public string Name { get; private set; }
        public string CurrentRoom { get; private set; }

        public Client(string address, int port)
        {
            try
            {
                Name = string.Empty;
                CurrentRoom = "main_room";
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Parse(address), port));
                Registry();
                sender = new Thread(new ThreadStart(SendMessage));
                receiver = new Thread(new ThreadStart(ReceiveMessage));
                sender.Start();
                receiver.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                CloseConnection(clientSocket);
                InterruptThread(receiver);
                InterruptThread(sender);
            }
        }

        private void Registry()
        {
            bool status = false;
            while (!status || Name.Length <= 0)
            {
                Console.Write("\n[The name must be longer than 0 characters]\n> ");
                Name = Console.ReadLine();
                try
                {
                    clientSocket.Send(Encoding.Unicode.GetBytes(Name));
                    byte[] answer = new byte[1];
                    do
                    {
                        clientSocket.Receive(answer);
                        status = BitConverter.ToBoolean(answer, 0);
                    } while (clientSocket.Available > 0);
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                    CloseConnection(clientSocket);
                }
                Console.Write(status ? $"Welcome, {Name}!" : $"User with name {Name} ale already connected.");
            }
        }

        private void CloseConnection(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void InterruptThread(Thread thread)
        {
            if (thread != null && thread.IsAlive)
            {
                try
                {
                    thread.Join(1000);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                }
            }
        }

        private void SendMessage()
        {
            try
            {
                string message;
                while (clientSocket.Connected)
                {
                    message = Console.ReadLine();
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    clientSocket.Send(data);
                    CheckCommand(message); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client.SendMessage");
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
            }
            finally
            {
                CloseConnection(clientSocket);
            }
        }

        private void ReceiveMessage()
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                while (clientSocket.Connected)
                {
                    int bytes = 0;
                    byte[] data = new byte[256];
                    do
                    {
                        bytes = clientSocket.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (clientSocket.Available > 0);
                    string clientMessage = builder.ToString();
                    Console.WriteLine(clientMessage);
                    if (clientMessage == string.Empty)
                    {
                        break;
                    }
                    builder.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client.ReceiveMessage");
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
            }
            finally
            {
                CloseConnection(clientSocket);
            }
        }

        private void CheckCommand(string text)
        {
            Match matchCom = Regex.Match(text, @"^(c_){1}\w+(($)||(\s+\w+$))");
            if (matchCom.Value.Length > 0) {
                Match matchArg = Regex.Match(text, @"\s+\w+$");
                switch (matchCom.Value)
                {
                    case "c_exit":
                        InterruptThread(receiver);
                        InterruptThread(sender);
                        CloseConnection(this.clientSocket);
                        break;
                    case "c_room":
                        CurrentRoom = Regex.Replace(matchArg.Value, @"\s", String.Empty);
                        Console.WriteLine($"Welcome to {CurrentRoom}!");
                        break;
                }
            }
        }
    }

    public class ClientLauncher
    {
        static void Main(string[] args)
        {
            _ = new Client(args[0], int.Parse(args[1]));
        }
    }
}