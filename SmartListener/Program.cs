using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;

namespace DemoService
{
    class Program
    {
        static void Main()
        {
            try
            {
                var ipAddress = "127.0.0.1";
                var port = 7777;
                var service = new AsyncService(ipAddress, port);

                service.Run();

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
    public class AsyncService
    {
        private static object lockObject = new object();
        private IPAddress ipAddress;
        private int port;

        public AsyncService(string ipAddress, int port)
        {
            this.port = port;
            this.ipAddress = IPAddress.Parse(ipAddress);
        }

        public async void Run()
        {
            var listener = new TcpListener(this.ipAddress, this.port);
            listener.Start();

            Console.Write("SmartListener service is now running");
            Console.WriteLine(" on port " + this.port);
            Console.WriteLine("Hit <enter> to stop service\n");

            while (true)
            {
                try
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    var t = Process(tcpClient);
                    await t;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task Process(TcpClient tcpClient)
        {
            var clientEndPoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine($"Received connection request from {clientEndPoint}");

            try
            {
                var networkStream = tcpClient.GetStream();
                var reader = new StreamReader(networkStream);

                while (true)
                {
                    var request = await reader.ReadLineAsync();
                    if (request == null) break; // Client closed connection

                    FileWriter(request);
                }

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (tcpClient.Connected) tcpClient.Close();
            }
        }

        private void FileWriter(string text)
        {
            lock (lockObject)
            {
                using (var fileStream = new FileStream("traffic.txt",
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.Read))
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.WriteLineAsync(text);
                }

            }
        }
    }
}