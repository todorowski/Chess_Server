using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameBoardServer
{
    class Listener
    {
        public Listener(int port, Action<Player, string> playerMessageCallback)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }

            //Remember to close the socket

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(100);

            //Create sockets when players connect
            new Thread(() => {
                while(true)
                {
                    Console.WriteLine("waiting for connection...");
                    Socket s = listener.Accept();
                    Player p = new Player(s);
                    p.messageAction += playerMessageCallback;
                    StartReceiveThread(s, p);
                    Console.WriteLine("Client connected...");
                }
            }).Start();
        }

        private static void StartReceiveThread(Socket s, Player p)
        {
            //Receive thread for player messages
            new Thread(() => {

                byte[] data = new byte[65565];
                int size = 0;
                do
                {
                    //Check dissconnect
                    if(s.Poll(1, SelectMode.SelectRead) && s.Available == 0)
                    {
                        //has disconnected
                        p.playerDisconnected?.Invoke(p);
                        p.messageAction = null;
                        return;
                    }
                    //Receive data
                    size = s.Receive(data);
                    string result = Encoding.ASCII.GetString(data, 0, size);
                    p.ReceiveFromSocket(result);
                } while (size != -1);

            }).Start();
        }
    }
}
