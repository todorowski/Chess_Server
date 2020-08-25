using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoardServer
{
    class GameRoom
    {
        GameBoard gameBoard = new GameBoard();
        List<Player> players = new List<Player>();
        int gameRoomID;
        bool whiteTurn = true;

        public GameRoom(int ID)
        {
            gameRoomID = ID;
        }

        public void AddPlayer(Player p)
        {
            Console.WriteLine("Player joined room: " + gameRoomID);
            players.Add(p);
            p.messageAction += playerMessageCallback;

            p.playerDisconnected += PlayerDisconnectedCallback;
            Console.WriteLine("Sending joined player the boardState");
            p.sendData(Interpreter.WriteGameBoard(gameBoard.GetBoardState()));
        }

        public void PlayerDisconnectedCallback(Player p)
        {
            RemovePlayer(p);
        }

        public void RemovePlayer(Player p)
        {
            lock (players)
            {
                Console.WriteLine("Player left room: " + gameRoomID);
                players.Remove(p);
                p.messageAction -= playerMessageCallback;
                p.playerDisconnected -= PlayerDisconnectedCallback;

                //Dissolve room if no player are in it.
                if (players.Count == 0)
                {
                    Console.WriteLine("No players left room dissolved");
                    Program.RemoveRoom(gameRoomID);
                }
            }
        }

        private void playerMessageCallback(Player player, string message)
        {
            //which team is the player on?
            string playerColor = "";
            if (players.Count >= 1 && players[0] == player)
            {
                playerColor = "white";
            }
            else if(players.Count >= 2 && players[1] == player)
            {
                playerColor = "black";
            }
            else
            {
                Console.WriteLine("Observer Tried to move a piece");
                return;
            }

            //Player is attempting to act out of turn
            if ((playerColor == "white" && !whiteTurn) || (playerColor == "black" && whiteTurn))
            {
                Console.WriteLine("Player is acting out of turn");
                return;
            }  

            lock(gameBoard)
            {
                //Is promotion message?
                if (Interpreter.ReadPromotePiece(message, out Vector2Int from, out string promoteTo) && gameBoard.PromotionPossibleForTeam(playerColor))
                {
                    Console.WriteLine(playerColor + " promoted a piece to " + promoteTo + " : " + message);
                    if(gameBoard.PromotePieceTo(from, promoteTo))
                        whiteTurn = !whiteTurn;
                }

                //Move a piece
                if (Interpreter.ReadMovePiece(message, out from, out Vector2Int to) && !gameBoard.PromotionPossibleForTeam(playerColor))
                {
                    Console.WriteLine(playerColor + " player made move: " + message);
                    if (gameBoard.GetPiece(from) != null && playerColor == gameBoard.GetPiece(from).Color)
                    {
                        if(gameBoard.MovePiece(from, to))
                        {
                            if (!gameBoard.PromotionPossibleForTeam(playerColor))
                                whiteTurn = !whiteTurn;
                        }
                    }
                }

                Console.WriteLine("Sending all players in " + gameRoomID + " the boardState");
                foreach (Player p in players)
                    p.sendData(Interpreter.WriteGameBoard(gameBoard.GetBoardState()));
            }
        }
    }
}
