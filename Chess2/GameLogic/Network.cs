using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace Assets.Scripts
{
    class Network
    {

        static void Connect()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("port:");
            string port = Console.ReadLine();

            IPEndPoint RemoteEndPoint = new IPEndPoint(ipAddress, int.Parse(port));
            listener.Connect(RemoteEndPoint);
            StartReciveThread(listener);
            Console.WriteLine("send some shit:");
            while (true)
            {
                string message = Console.ReadLine();
                byte[] msg = Encoding.ASCII.GetBytes(message);
                listener.Send(msg);
            }
        }

        private static void StartReciveThread(Socket s)
        {
            //Listing thread
            new Thread(() => {

                byte[] data = new byte[65565];
                int size = 0;
                do
                {
                    size = s.Receive(data);
                    string result = Encoding.ASCII.GetString(data, 0, size);
                    Console.WriteLine(result);
                } while (size != -1);

            }).Start();
        }
    }
}
