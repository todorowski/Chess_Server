using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoardServer
{
    class GameRoom
    {
        GameBoard gameBoard = new GameBoard();
        public List<Player> players = new List<Player>();
        Dictionary<Player, List<Vector2Int>> colorListDictionary = new Dictionary<Player, List<Vector2Int>>();
        Dictionary<Player, List<string>> messageListDictionary = new Dictionary<Player, List<string>>();
        int gameRoomID;
        bool whiteTurn = true;
        bool gameEnded = false;
        bool whiteWantsRematch = false;
        bool blackWantsRematch = false;
        public bool rematch = false;
        bool whiteAcceptsDraw = false;
        bool blackAcceptsDraw = false;

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
               
            }
            else if(players.Count == 2)
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
            //player should still be able to press rematch
            bool actingOutOfTurn = false;
            if ((playerColor == "white" && !whiteTurn) || (playerColor == "black" && whiteTurn))
            {
                if (!gameEnded)
                {
                    Console.WriteLine("Player is acting out of turn");
                    actingOutOfTurn = true;
                }
            }  

            lock(gameBoard)
            {
                //Is promotion message?
                if (Interpreter.ReadPromotePiece(message, out Vector2Int from, out string promoteTo) && gameBoard.PromotionPossibleForTeam(playerColor))
                {
                    if (actingOutOfTurn)
                        return;

                    PromotePiece(playerColor, from, promoteTo, message);
                }

                //Move a piece 
                if (Interpreter.ReadMovePiece(message, out from, out Vector2Int to) && !gameBoard.PromotionPossibleForTeam(playerColor))
                {
                    if (actingOutOfTurn)
                        return;
                    MakeMove(player, playerColor, from, to, message);
                }

                //When player resigns, other player wins and win screen is shown on client side
                if (Interpreter.ReadResign(message, out gameRoomID))
                {
                    PlayerResignFromGame(playerColor); 
                }

                //Check if players want a Rematch
                //check first if one player has pressed rematch and then if the remaining player also does so
                
                if (Interpreter.ReadRematch(message, out gameRoomID, out int wantsRematch))
                {
                    CheckIfPlayersWantRematch(playerColor);   
                }

                if(Interpreter.ReadOfferDraw(message, out gameRoomID))
                {
                    foreach(Player p in players)
                    {
                        p.sendData(Interpreter.WriteActivateDrawWindow(gameRoomID));
                    }
                }

                if(Interpreter.ReadDrawDecision(message, out int decision))
                {
                    CheckPlayersDrawDecisions(player, playerColor, decision);
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
            foreach(Player player in players)
            {
                Interpreter.WritePlayerWonMessage("");
            }
        }

        private void Rematch()
        {
            gameBoard = new GameBoard();
            blackWantsRematch = false;
            whiteWantsRematch = false;
            rematch = false;
            whiteTurn = true;

            foreach (Player p in players)
            {
                ClearAllColor(p);
                ClearAllLogMessages(p);
                p.sendData(Interpreter.WriteSetUpGameMessage());
                p.sendData(Interpreter.WriteGameBoard(gameBoard.GetBoardState()));
            }
        }

        private void PromotePiece(string playerColor, Vector2Int from, string promoteTo, string message)
        {
            Console.WriteLine(playerColor + " promoted a piece to " + promoteTo + " : " + message);
            if (gameBoard.PromotePieceTo(from, promoteTo))
                whiteTurn = !whiteTurn;
        }

        private void MakeMove(Player player, string playerColor, Vector2Int from, Vector2Int to, string message)
        {
            ClearAllColor(player);
            Console.WriteLine(playerColor + " player made move: " + message);
            if (gameBoard.GetPiece(from) != null && playerColor == gameBoard.GetPiece(from).Color)
            {
                if (gameBoard.MovePiece(from, to))
                {
                    logMessage($"{playerColor}: ({from.x}, {from.y}) , ({to.x}, {to.y})", player);
                    if (!gameBoard.PromotionPossibleForTeam(playerColor))
                    {
                        foreach (Player p in players)
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
                    }
                    else if (gameBoard.CheckForCheck(gameBoard.whiteKing))
                    {
                        headerMessageForAll("White is in check!");
                        ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.whiteKing));
                    }

                    if (gameBoard.CheckForCheckmate(gameBoard.blackKing))
                    {
                        headerMessageForAll("Black is in checkmate!");
                        ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.blackKing));
                        foreach (Player p in players)
                        {
                            Interpreter.WritePlayerWonMessage("white");
                        }
                    }
                    else if (gameBoard.CheckForCheckmate(gameBoard.whiteKing))
                    {
                        headerMessageForAll("White is in checkmate!");
                        ColorSquareMessageRedForAll(gameBoard.GetPiecePosition(gameBoard.whiteKing));
                        foreach (Player p in players)
                        {
                            Interpreter.WritePlayerWonMessage("black");
                        }
                    }
                   
                    if (gameBoard.killed != null)
                    {
                        //A PIECE WAS KILLED!
                        Console.Write("A PIECE WAS KILLED!");
                        string pieceType = gameBoard.killed.GetType().ToString();
                        Console.Write("THE TYPE WAS" + pieceType);

                        //check color of piece
                        //send message to that player
                        if(gameBoard.killed.Color == "black")
                        {
                            headerMessageForAll("Black " + pieceType + " was taken!");
                        }
                        else
                        {
                            headerMessageForAll("White " + pieceType + " was taken!");
                        }
                    }
                }
                else
                {
                    ColorSquareMessageRed(to, player);
                    ColorSquareMessageRed(from, player);

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
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

        private void PlayerResignFromGame(string playerColor)
        {
            gameEnded = true;
            string color = "";

            color = playerColor == "white" ? "black" : "white";
            headerMessageForAll(color + "Player" + " " + "won!");
            foreach (Player p in players)
            {
                p.sendData(Interpreter.WritePlayerWonMessage(color));

            }
        }

        private void CheckIfPlayersWantRematch(string playerColor)
        {
            if (playerColor == "white")
            {
                whiteWantsRematch = true;
                if(whiteWantsRematch && !blackWantsRematch)
                {
                    headerMessageForAll("White wanted rematch, waiting for other player...");
                }
               
            }

            if (playerColor == "black")
            {
                blackWantsRematch = true;
                if(blackWantsRematch && !whiteWantsRematch)
                {
                    headerMessageForAll("Black wanted rematch, waiting for other player...");
                }
            }

            if(whiteWantsRematch && blackWantsRematch)
            {
                headerMessageForAll("It's a rematch!");
                gameEnded = false;
                rematch = true;
                Rematch();
            }
        }

        private void CheckPlayersDrawDecisions(Player player, string playerColor, int decision)
        {
            //players want a draw
            if (decision == 1 && player == players[0])
            {
                whiteAcceptsDraw = true;
                headerMessageForAll("White accepts draw, waiting for other player...");
            }

            if (decision == 1 && player == players[1])
            {
                blackAcceptsDraw = true;
                headerMessageForAll("Black accepts draw, waiting for other player...");
            }
         
            if (whiteAcceptsDraw && blackAcceptsDraw)
            {
                gameEnded = true;
                foreach (Player p in players)
                    p.sendData(Interpreter.WritePlayerWonMessage("Draw!"));
                whiteAcceptsDraw = false;
                blackAcceptsDraw = false;
            }

            //players do not want a draw
            if (decision == 0 && player == players[0])
            {
                headerMessageForAll("White did not accept draw");
                whiteAcceptsDraw = false;
            }

            if (decision == 0 && player == players[1])
            {
                headerMessageForAll("Black did not accept draw");
                blackAcceptsDraw = false;
            }
        }
    }
}
