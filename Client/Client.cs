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
                Name = string.Empty;
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
            bool status = false;
            while (!status || Name.Length <= 0)
            {
                Console.Write("[The name must be longer than 0 characters]\n> ");
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
                while (thread.ThreadState == ThreadState.Background
                    || thread.ThreadState == ThreadState.Running)
                {
                    thread.Abort();
                }
            }
        }

        private void SendMessage()
        {
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