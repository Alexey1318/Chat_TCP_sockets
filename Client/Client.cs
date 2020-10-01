using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        private Socket clientSocket;
        private Thread sender;
        private Thread receiver;
        private string name;

        public Client(string address, int port)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(iPEndPoint);
            Registry();

            sender = new Thread(new ThreadStart(SendMessage));
            receiver = new Thread(new ThreadStart(ReceiveMessage));

            sender.Start();
            receiver.Start();
        }

        private void Registry()
        {
            Console.Write("Connected. Write your name: ");
            name = Console.ReadLine();
            if (!name.Equals(null))
            {
                byte[] data = Encoding.Unicode.GetBytes(name);
                clientSocket.Send(data);
            }
        }

        private void SendMessage()
        {
            Console.WriteLine("Welcome to the chat, {0}!", name);
            try
            {
                string message = Console.ReadLine();
                while (true)
                {
                    if (message.Equals("exit"))
                    { 
                        throw new Exception("exit word was received; closing connection..."); 
                    }
                    else
                    {
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        clientSocket.Send(data);
                        message = Console.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
        }

        private void ReceiveMessage()
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
                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + clientMessage);
                    if (clientMessage == string.Empty)
                    {
                        throw new Exception("empty message");
                    }
                    builder.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
        }
    }

    public class ClientLauncher
    {
        static void Main(string[] args)
        {
            Client client = new Client("127.0.0.1", 1234);
        }
    }
}