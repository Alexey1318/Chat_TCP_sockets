using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Client
    {
        private readonly Socket clientSocket;
        private readonly Thread sender;
        private readonly Thread receiver;
        public string Name { get; private set; }

        public Client(string address, int port)
        {
            try
            {
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
                InterruptThread(sender);
                InterruptThread(receiver);
            }
        }

        private void Registry()
        {
            Console.Write("Connected. Write your name: ");
            Name = Console.ReadLine();
            while (!(Name.Length > 0))
            {
                Console.Write("Please, enter your name: ");
                Name = Console.ReadLine();
            }
            byte[] data = Encoding.Unicode.GetBytes(Name);
            try
            {
                clientSocket.Send(data);
            }
            catch (SocketException e)
            {
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
                CloseConnection(clientSocket);
            }
        }

        private void CloseConnection(Socket socket)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        private void InterruptThread(Thread thread)
        {
            if (thread.IsAlive)
            {
                while (thread.ThreadState == ThreadState.Background
                    || thread.ThreadState == ThreadState.Running)
                {
                    thread.Abort();
                }
            }
        }

        private void SendMessage()
        {
            Console.WriteLine($"Welcome to the chat, {Name}!");
            try
            {
                string message = Console.ReadLine();
                while (clientSocket.Connected)
                {
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    clientSocket.Send(data);
                    if (message.Equals("exit"))
                    {
                        break;
                    }
                    message = Console.ReadLine();
                }
            }
            catch (Exception e)
            {
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
                Console.WriteLine($"{e.Source}.{e.TargetSite} throws an exception: {e.Message}");
            }
            finally
            {
                CloseConnection(clientSocket);
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