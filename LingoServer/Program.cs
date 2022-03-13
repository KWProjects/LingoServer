using System;
using System.Threading;

namespace LingoServer
{
    class Program
    {
        TCPListener _listener;
        static void Main(string[] args)
        {
            Server server = new Server();

            while (true)
            {

                string line = Console.ReadLine();
                if (line == "stop")
                {

                    Console.WriteLine("Stopping server...");
                    server.Stop();
                    Thread.Sleep(500000);
                    break;
                }
                else if (line != "")
                {

                    server.SendAll(line);
                }

            }

        }
    }
}
