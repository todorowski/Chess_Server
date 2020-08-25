using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameBoardServer
{
    public class Program
    {

        static Dictionary<int, GameRoom> gameRoomMap = new Dictionary<int, GameRoom>();
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a port to listen to: ");
            int port = int.Parse(Console.ReadLine());
            Listener l = new Listener(port, PlayerMessageCallback);
        }

        private static void PlayerMessageCallback(Player player, string message)
        {
            Console.WriteLine("Player send message: " + message);
            int id = -1;

            lock (gameRoomMap)
            {
                if (Interpreter.ReadJoinGame(message, out id))
                {
                    if (!gameRoomMap.ContainsKey(id))
                    {
                        Console.WriteLine("Room was created from message: " + message);
                        GameRoom newRoom = new GameRoom(id);
                        gameRoomMap.Add(id, newRoom);
                        gameRoomMap[id].AddPlayer(player);
                    }
                    else
                    {
                        gameRoomMap[id].AddPlayer(player);
                    }
                }

                if (Interpreter.ReadLeaveGame(message, out id))
                {
                    gameRoomMap[id].RemovePlayer(player);
                }
            }
        }

        public static void RemoveRoom(int id)
        {
            if (gameRoomMap.ContainsKey(id))
            {
                gameRoomMap.Remove(id);

            }
        }
    }
}
