using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace GameBoardServer
{
    class Player
    {
        Socket sendSocket;
        public Action<Player, string> messageAction;
        public Action<Player> playerDisconnected;
        public Player(Socket s)
        {
            sendSocket = s;
        }

        public void sendData(string s)
        {
            byte[] message = Encoding.ASCII.GetBytes(s);
            sendSocket.Send(message);
        }

        public void ReceiveFromSocket(string data)
        {
            messageAction?.Invoke(this, data);
        }
    }
}
