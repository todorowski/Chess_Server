using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoardServer
{
    class GameRoom
    {
        GameBoard gameBoard = new GameBoard();
        List<Player> players = new List<Player>();
        Dictionary<Player, List<Vector2Int>> colorListDictionary = new Dictionary<Player, List<Vector2Int>>();
        Dictionary<Player, List<string>> messageListDictionary = new Dictionary<Player, List<string>>();
        int gameRoomID;
        bool whiteTurn = true;

        public GameRoom(int ID)
        {
            gameRoomID = ID;
            gameBoard.remiCallback += AjDetArOavgjort;
        }
        public void AddPlayer(Player p)
        {
            Console.WriteLine("Player joined room: " + gameRoomID);
            players.Add(p);
            if(players.Count == 1)
            {
                headerMessage("You play as white", p);
            }else if(players.Count == 2)
            {
                headerMessage("You play as black", p);
            }
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
                ClearAllColor(p);
                ClearAllLogMessages(p);
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
                headerMessage("You play as white", player);
            }
            else if(players.Count >= 2 && players[1] == player)
            {
                playerColor = "black";
                headerMessage("You play as black", player);
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
                    if (gameBoard.PromotePieceTo(from, promoteTo))
                        whiteTurn = !whiteTurn;
                }

                //Move a piece 
                if (Interpreter.ReadMovePiece(message, out from, out Vector2Int to) && !gameBoard.PromotionPossibleForTeam(playerColor))
                {
                    ClearAllColor(player);
                    Console.WriteLine(playerColor + " player made move: " + message);
                    if (gameBoard.GetPiece(from) != null && playerColor == gameBoard.GetPiece(from).Color)
                    {
                        if(gameBoard.MovePiece(from, to))
                        {
                            logMessage($"{playerColor}: ({from.x}, {from.y}) , ({to.x}, {to.y})", player);
                            if (!gameBoard.PromotionPossibleForTeam(playerColor))
                            {
                                foreach(Player p in players)
                                {
                                    ColorSquareMessageGreen(to, p);
                                    ColorSquareMessageGreen(from, p);
                                }
                                whiteTurn = !whiteTurn;
                            }

                            //check and checkmate feedback
                            if (gameBoard.CheckForCheck(gameBoard.blackKing))
                            {
                                headerMessageForAll("black is in check!");
                                ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.blackKing));
                            }else if (gameBoard.CheckForCheck(gameBoard.whiteKing))
                            {
                                headerMessageForAll("White is in check!");
                                ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.whiteKing));
                            }

                            if (gameBoard.CheckForCheckmate(gameBoard.blackKing))
                            {
                                headerMessageForAll("Black is in checkmate!");
                                ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.blackKing));
                            }
                            else if (gameBoard.CheckForCheckmate(gameBoard.whiteKing))
                            {
                                headerMessageForAll("White is in checkmate!");
                                ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.whiteKing));
                            }

                        }
                        else
                        {
                            ColorSquareMessageRed(to, player);
                            ColorSquareMessageRed(from, player);

                            for(int i = 0; i < 8; i++)
                            {
                                for(int j = 0; j < 8; j++)
                                {
                                    Vector2Int square = new Vector2Int(i, j);

                                    if (gameBoard.GetPiece(from).CheckMove(square))
                                    {
                                        ColorSquareMessageYellow(from, player);
                                        ColorSquareMessageYellow(square, player);
                                    }
                                }
                            }
                            
                        }
                    }
                }

                Console.WriteLine("Sending all players in " + gameRoomID + " the boardState");
                foreach (Player p in players)
                    p.sendData(Interpreter.WriteGameBoard(gameBoard.GetBoardState()));
            }
        }
        private void headerMessage(string message, Player p)
        {
            p.sendData(Interpreter.WriteInfoMessage(2, message));
        }
        private void headerMessageForAll(string message)
        {
            foreach (Player p in players)
            {
                p.sendData(Interpreter.WriteInfoMessage(2, message));
            }
        }
        private void logMessage(string message, Player player)
        {
            foreach (Player p in players)
            {
                p.sendData(Interpreter.WriteInfoMessage(1, message));
            }

            if (!messageListDictionary.ContainsKey(player))
            {
                List<string> messageList = new List<string>();
                messageListDictionary.Add(player, messageList);
                messageList.Add(message);
            }
            else
            {
                messageListDictionary[player].Add(message);
            }
        }

        //Color methods!
        private void ColorSquareMessage(Vector2Int square, Player p, int r, int g, int b, int a)
        {
            p.sendData(Interpreter.WriteColorSquareMessage(square, r, g, b, a));
            if (!colorListDictionary.ContainsKey(p))
            {
                //add a new list
                List<Vector2Int> list = new List<Vector2Int>();
                colorListDictionary.Add(p, list);
                list.Add(square);
            }
            else
            {
                colorListDictionary[p].Add(square);
            }
        }
        private void ColorSquareMessageRedForAll(Vector2Int square)
        {
            foreach (Player p in players)
            {
                ColorSquareMessageRed(square, p);
            }
        }
        private void ColorSquareMessageRed(Vector2Int square, Player p)
        {
            ColorSquareMessage(square, p, 241, 24, 79, 100);
        }
        private void ColorSquareMessageGreenForAll(Vector2Int square)
        {
            foreach (Player p in players)
            {
                ColorSquareMessageGreen(square, p);
            }
        }
        private void ColorSquareMessageGreen(Vector2Int square, Player p)
        {
            ColorSquareMessage(square, p, 52, 214, 77, 100);
        }
        private void ColorSquareMessageYellow(Vector2Int square, Player p)
        {
            ColorSquareMessage(square, p, 245, 228, 20, 100);
        }
        private void ClearAllColor(Player p)
        {
            if (!colorListDictionary.ContainsKey(p))
                return;

            //remove the color from each square in the players list
            List<Vector2Int> allColoredSquiares = colorListDictionary[p];
            for (int i = 0; i < allColoredSquiares.Count; i++)
            {
                p.sendData(Interpreter.WriteColorSquareMessage(allColoredSquiares[i], 0, 0, 0, 0));
            }
            colorListDictionary[p].Clear();  
        }

        //clear the log from all messages belonging to specific player
        private void ClearAllLogMessages(Player p)
        {
            if (!messageListDictionary.ContainsKey(p))
                return;

            messageListDictionary[p].Clear();
             
        }
        private void AjDetArOavgjort()
        {
            headerMessageForAll("It's a draw!");
        }
    }
}
