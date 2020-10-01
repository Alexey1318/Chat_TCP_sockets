using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    class TcpServer
    {
        TcpListener server;
        public TcpServer(string host, int port)
        {
            IPAddress localAddr = IPAddress.Parse(host);
            server = new TcpListener(localAddr, port);
            server.Start();
            Console.WriteLine("Server ready");
        }

        public void WaitingForClients()
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Ожидание подключений... ");
                    // получаем входящее подключение
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Подключен клиент. Выполнение запроса...");
                    // получаем сетевой поток для чтения и записи
                    NetworkStream stream = client.GetStream();
                    // сообщение для отправки клиенту
                    string response = "Привет мир";
                    // преобразуем сообщение в массив байтов
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    // отправка сообщения
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Отправлено сообщение: {0}", response);
                    // закрываем поток
                    stream.Close();
                    // закрываем подключение
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
            }
        }
    }
    class Launcher
    {
        static void Main(string[] args)
        {
        }
    }
}
