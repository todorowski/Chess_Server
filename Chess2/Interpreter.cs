using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GameBoardServer
{
    class Interpreter
    {
        public static bool ReadJoinGame(string message, out int gameID)
        {
            string[] messages = message.Split(' ');
          
            if(messages.Length == 2 && messages[0] == "J")
            {
                gameID = int.Parse(messages[1]);
                return true;
            }

            gameID = -1;
            return false;
        }
        public static bool ReadLeaveGame(string message, out int gameID)
        {
            string[] messages = message.Split(' ');

            if (messages.Length == 2 && messages[0] == "L")
            {
                gameID  = int.Parse(messages[1]);
                return true;
            }

            gameID = -1;
            return false;
        }
        public static bool ReadMovePiece(string message, out Vector2Int from, out Vector2Int to)
        {
            string[] messages = message.Split(' ');
           

            if (messages.Length == 3 && messages[0] == "M")
            {
                string[] message1 = messages[1].Split(',');
                string[] message2 = messages[2].Split(',');

                from = new Vector2Int(int.Parse(message1[0]), int.Parse(message1[1]));
                to = new Vector2Int(int.Parse(message2[0]), int.Parse(message2[1]));

                return true;
            }

            from = new Vector2Int(0,0);
            to = new Vector2Int(0, 0);
            return false;
        }
        private static bool ReadPiece(string message, ref GameBoard gameBoard, out Piece piece)
        {
            string[] messages = message.Split(':');
            piece = null;
            if (messages.Length >= 1 && messages[0] == "X")
                return true;
            if (messages.Length != 3)
                return false;

            //color
            string color = messages[1] == "B" ? "black" : "white";

            //types
            string type = messages[0];
            if (type == "KI")
                piece = new King(gameBoard, color);
            if (type == "QU")
                piece = new Queen(gameBoard, color);
            if (type == "TO")
                piece = new Tower(gameBoard, color);
            if (type == "BI")
                piece = new Bishop(gameBoard, color);
            if (type == "KN")
                piece = new Knight(gameBoard, color);
            if (type == "PA")
                piece = new Pawn(gameBoard, color);

            //hasMoved
            if (piece != null)
                piece.HasMoved = messages[2] == "T";

            return true;
        }
        public static bool ReadGameBoard(string message, ref GameBoard gameBoard)
        {
            Piece[,] boardState = new Piece[8, 8];
            string[] messages = message.Split(' ');
            if (messages.Length != 64)
                return false;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    ReadPiece(messages[y * 8 + x], ref gameBoard, out Piece p);
                    boardState[x, y] = p;
                }
            }

            gameBoard.SetGameBoardState(boardState);
            return true;
        }

        public static bool ReadPromotePiece(string message, out Vector2Int from, out string promoteType)
        {
            string[] messages = message.Split(' ');

            if(messages.Length == 3 && messages[0] == "P")
            {
                string[] message1 = messages[1].Split(',');
                string message2 = messages[2];

                from = new Vector2Int(int.Parse(message1[0]), int.Parse(message1[1]));
                promoteType = message2;
                return true;
            }

            from = new Vector2Int(0, 0);
            promoteType = " ";
            return false;
        }

        public static bool ReadInfoMessage(string message, out int messageType, out string content)
        {
            string[] messages = message.Split('&');

            if(messages.Length == 3 && messages[0] == "I")
            {
                messageType = int.Parse(messages[1]);
                content = messages[2];
                return true;
            }

            messageType = -1;
            content = "";
            return false;
        }

        public static bool ReadColorSquareMessage(string message, out Vector2Int square, out int r, out int g, out int b, out int a)
        {
            string[] messages = message.Split(' ');

            if(messages.Length == 3 && messages[0] == "C")
            {
                string[] messages1 = messages[1].Split(',');
                string[] messages2 = messages[2].Split(',');

                square = new Vector2Int(int.Parse(messages1[0]), int.Parse(messages1[1]));
                r = int.Parse(messages2[0]);
                g = int.Parse(messages2[1]); ;
                b = int.Parse(messages2[2]); ;
                a = int.Parse(messages2[3]); ;
                return true;
            }

            square = new Vector2Int(0, 0);
            r = 0;
            g = 0;
            b = 0;
            a = 0;
            return false;
        }

        public static string WriteColorSquareMessage(string message, Vector2Int square, int r, int g, int b, int a)
        {
            return "C " + square.x + "," + square.y + " " + r + "," + g + "," + b + "," + a;
        }

        public static string WriteInfoMessage(int messageType, string message)
        {
            return "I" + "&" + messageType + "&" + message;
        }

        public static string WriteJoinGame(int gameID)
        {
            return "J " + gameID;
        }
        public static string WriteLeaveGame(int gameID)
        {
            return "L " + gameID;
        }
        public static string WriteMovePiece(Vector2Int from, Vector2Int to)
        {
            return "M " + from.x + "," + from.y + " " + to.x + "," + to.y;
        }
        private static string WritePiece(Piece piece)
        {
            if (piece == null) return "X";

            string type = "X";
            string color = piece.Color.Substring(0, 1).ToUpper();
            string hasMoved = piece.HasMoved ? "T" : "F";
            //which type is it?
            if (piece is King)
                type = "KI";
            if (piece is Queen)
                type = "QU";
            if (piece is Tower)
                type = "TO";
            if (piece is Bishop)
                type = "BI";
            if (piece is Knight)
                type = "KN";
            if (piece is Pawn)
                type = "PA";

            return type + ":" + color + ":" + hasMoved;
        }
        public static string WriteGameBoard(Piece[,] pieces)
        {
            string result = "";
            for (int y = 0; y < pieces.GetLength(1); y++)
            {
                for(int x = 0; x < pieces.GetLength(0); x++)
                {
                    result += WritePiece(pieces[x, y]) + " ";
                }
            }
            return result.Substring(0, result.Length-1);
        }

        public static string WritePromotePiece(Vector2Int from, string promoteType)
        {
            return "P" + from.x + "," + from.y + promoteType;
        }
    }
}
