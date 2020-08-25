using System.Collections;
using System.Collections.Generic;
using System;

public class GameBoard
{
    Piece[,] boardState = new Piece[8,8];
    public List<Piece> whiteTeam = new List<Piece>();
    public List<Piece> blackTeam = new List<Piece>();
    public Piece whiteKing;
    public Piece blackKing;
    //Constructor
    public GameBoard()
    {
        //White pieces
        whiteKing = new King(this, "white");
        //Pawns
        for (int i = 0; i < 8; i++)
        {
            AddPiece(new Pawn(this, "white"), new Vector2Int(i, 1));
        }
        //Suites
        AddPiece(new Tower(this, "white"), new Vector2Int(0,0));
        AddPiece(new Knight(this, "white"), new Vector2Int(1, 0));
        AddPiece(new Bishop(this, "white"), new Vector2Int(2, 0));
        AddPiece(new Queen(this, "white"), new Vector2Int(3, 0));
        AddPiece(whiteKing, new Vector2Int(4, 0));
        AddPiece(new Bishop(this, "white"), new Vector2Int(5, 0));
        AddPiece(new Knight(this, "white"), new Vector2Int(6, 0));
        AddPiece(new Tower(this, "white"), new Vector2Int(7, 0));

        //Black pieces
        blackKing = new King(this, "black");
        //Pawns
        for (int i = 0; i < 8; i++)
        {
            AddPiece(new Pawn(this, "black"), new Vector2Int(i, 6));
        }
        //Suites
        AddPiece(new Tower(this, "black"), new Vector2Int(0, 7));
        AddPiece(new Knight(this, "black"), new Vector2Int(1, 7));
        AddPiece(new Bishop(this, "black"), new Vector2Int(2, 7));
        AddPiece(new Queen(this, "black"), new Vector2Int(3, 7));
        AddPiece(blackKing, new Vector2Int(4, 7));
        AddPiece(new Bishop(this, "black"), new Vector2Int(5, 7));
        AddPiece(new Knight(this, "black"), new Vector2Int(6, 7));
        AddPiece(new Tower(this, "black"), new Vector2Int(7, 7));
    }
    //Public Methods
    public bool CheckForCheck(Vector2Int square, string attackingTeam)
    {
        List<Piece> team;
        team = attackingTeam == "white" ? whiteTeam : blackTeam;
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i].CheckMove(square))
            {
                return true;
            }
        }
        return false;
    }
    public bool CheckForCheck(Piece king)
    {
        return CheckForCheck(GetPiecePosition(king), king.Color == "white" ? "black" : "white");
    }
    public bool CheckForCheckmate(Piece king)
    {
        //Check for check
        if (!CheckForCheck(king))
            return false;

        //Check for mate
        List<Piece> team = king.Color == "black" ? blackTeam : whiteTeam;
        Vector2Int to = new Vector2Int(0, 0);

        for (int i = 0; i < team.Count; i++)
        {
            //Go through all board squares
            //check CheckMove for each square and piece
            for (int j = 0; j < boardState.GetLength(0); j++)
            {
                for (int k = 0; k < boardState.GetLength(1); k++)
                {
                    to.x = j;
                    to.y = k;
                    if (MovePieceIsLegal(GetPiecePosition(team[i]), to))
                    {
                        //not checkmate
                        return false;
                    }
                }
            }
        }
        //checkmate
        return true;
    }
    public void SetGameBoardState(Piece[,] gameBoardState)
    {
        boardState = gameBoardState;
        whiteTeam.Clear();
        blackTeam.Clear();
        foreach (Piece p in gameBoardState)
        {
            if (p.Color == "white")
            {
                if (p is King)
                    whiteKing = p;
                whiteTeam.Add(p);
            }
            else
            {
                if (p is King)
                    blackKing = p;
                blackTeam.Add(p);
            }
        }
    }
    //Getters
    public Piece[,] GetGameBoardState()
    {
        return boardState;
    }
    public Piece GetPiece(Vector2Int from)
    {
        return boardState[from.x, from.y];
    }
    public Vector2Int GetPiecePosition(Piece p)
    {
        for (int i = 0; i < boardState.GetLength(0); i++)
        {
            for (int j = 0; j < boardState.GetLength(1); j++)
            {
                if (boardState[i, j] == p)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        //No such piece
        return new Vector2Int(-1, -1);
    }
    public Piece[,] GetBoardState()
    {
        return boardState;
    }
    //Inteface
    public bool MovePiece(Vector2Int from, Vector2Int to)
    {
        if (!MovePieceIsLegal(from, to))
            return false;

        //Is this enpassant 
        if(DoEnpassant(from, to))
            return true;
        //Is this a castle
        if (DoCastling(from, to))
            return true;
         
        //Move piece
        Piece killedPiece = boardState[to.x, to.y];
        Piece movedPiece = boardState[from.x, from.y];
        RemovePiece(killedPiece);
        boardState[to.x, to.y] = movedPiece;
        boardState[from.x, from.y] = null;
        movedPiece.HasMoved = true;
        return true;
    }
    public bool PromotePieceTo(Vector2Int from, string promotionPiece)
    {
        if (boardState[from.x, from.y] != null && PromotionPossible(from))
        {
            string playerColor = boardState[from.x, from.y].Color;
            Console.Write(playerColor + " is promoting from " + from.x + "," + from.y + " to " + promotionPiece);

            Piece piece = null;
            RemovePiece(boardState[from.x, from.y]);
            if (promotionPiece == "QU")
            {
                piece = new Queen(this, playerColor);
            }
            if (promotionPiece == "TO")
            {
                piece = new Tower(this, playerColor);
            }
            if (promotionPiece == "BI")
            {
                piece = new Bishop(this, playerColor);
            }
            if (promotionPiece == "KN")
            {
                piece = new Knight(this, playerColor);
            }

            AddPiece(piece, from);
            return true;
        }
        return false;
    }
    public bool PromotionPossibleForTeam(string team)
    {
        if(team == "white")
        {
            foreach(Piece p in whiteTeam)
            {
                if (PromotionPossible(GetPiecePosition(p)))
                {
                    return true;
                }
            }
        }
        if(team == "black")
        {
            foreach (Piece p in blackTeam)
            {
                if (PromotionPossible(GetPiecePosition(p)))
                {
                    return true;
                }
            }
        }

        return false;
    }
    //Private methods
    private bool AddPiece(Piece piece, Vector2Int boardPosition)
    {
        if (boardState[boardPosition.x, boardPosition.y] != null)
            return false;

        //Cool turnery
        List <Piece> team = piece.Color == "white" ? whiteTeam : blackTeam;
        team.Add(piece);

        boardState[boardPosition.x, boardPosition.y] = piece;
        return true;
    }
    private bool RemovePiece(Piece piece)
    {
        if (piece == null)
            return false;
        List<Piece> team = piece.Color == "white" ? whiteTeam : blackTeam;
        Vector2Int pos = GetPiecePosition(piece);
        //Escapes
        if (pos.x < 0) return false;
        if (!team.Contains(piece)) return false;
            team.Remove(piece);

        boardState[pos.x, pos.y] = null;
        return true;
    }
    private bool MovePieceIsLegal(Vector2Int from, Vector2Int to)
    {
        //if null
        if (boardState[from.x, from.y] == null)
            return false;

        //a castling move or not an enpassant
        bool enPassantMove = EnPassantPossible(from, to);
        bool castlingMove = CastlingPossible(from, to);
        if (enPassantMove || castlingMove)
            return true;

        //If from-piece checkMove is false 
        if (!boardState[from.x, from.y].CheckMove(to))
            return false;

        //Move piece
        Piece killedPiece = boardState[to.x, to.y];
        Piece movePiece = boardState[from.x, from.y];
        RemovePiece(killedPiece);
        boardState[to.x, to.y] = movePiece;
        boardState[from.x, from.y] = null;

        //Check for check
        bool inCheck = CheckForCheck(boardState[to.x, to.y].Color == "white" ? whiteKing : blackKing);
        boardState[from.x, from.y] = movePiece;
        boardState[to.x, to.y] = null;
        if (killedPiece != null)
            AddPiece(killedPiece, to);
        return !inCheck;
    }
    private bool EnPassantPossible(Vector2Int from, Vector2Int to)
    {
        Piece pawn = boardState[from.x, from.y];
        //get the pawn or player is trying to make an illegal move
        if (boardState[from.x, from.y] == null || !(boardState[from.x, from.y] is Pawn))
            return false;

        //Black pawn is attacker
        if (pawn.Color == "black")
        {
            //en passant in right diagonal
            if (to.y == 2 && to.x == (from.x + 1))
            {
                if (boardState[from.x + 1, from.y] != null && boardState[from.x + 1, from.y].Color == "white" && boardState[from.x + 1, from.y] is Pawn)
                {
                    return true;
                }
            //en passant in left diagonal
            }else if(to.y == 2 && to.x == (from.x - 1))
            {
                if(boardState[from.x - 1, from.y] != null && boardState[from.x - 1, from.y].Color == "white" && boardState[from.x - 1, from.y] is Pawn)
                {
                    return true;
                }
            }
        //white pawn is the attacker
        }else if(pawn.Color == "white")
        {
            if(to.y == 5 && to.x == (from.x + 1))
            {
                if(boardState[from.x + 1, from.y] != null && boardState[from.x + 1, from.y].Color == "black" && boardState[from.x + 1, from.y] is Pawn)
                {
                    return true;
                }
            }else if(to.y == 5 && to.x == (from.x - 1))
            {
                if (boardState[from.x - 1, from.y] != null && boardState[from.x - 1, from.y].Color == "black" && boardState[from.x - 1, from.y] is Pawn)
                {
                    return true;
                }
            }
        }
   
        return false;
    }
    private bool CastlingPossible(Vector2Int from, Vector2Int to)
    {
        //White queenside castling
        Piece king = boardState[from.x, from.y];

        //Get the color of the king
        string color = king.Color;

        //white side castling
        if (color == "white")
        {
            //King is aigth
            if (king == null || !(king is King) || king.HasMoved)
                return false;

            //white queenside castling
            if(to.Equals(new Vector2Int(2, 0)))
            {
                Piece queenTower = boardState[0, 0];
                //Tower is alright
                if (queenTower == null || !(queenTower is Tower) || queenTower.HasMoved)
                    return false;
                //Path is blocked but hardcoded
                if (boardState[1, 0] != null || boardState[2, 0] != null || boardState[3, 0] != null)
                    return false;
                //Path Is checked?
                if (CheckForCheck(new Vector2Int(2, 0), "black") || CheckForCheck(new Vector2Int(3, 0), "black"))
                    return false;

                return true;
            //White kingside castling
            }else if(to.Equals(new Vector2Int(6, 0)))
            {
                Piece kingTower = boardState[7, 0];
                //Tower is alright
                if (kingTower == null || !(kingTower is Tower) || kingTower.HasMoved)
                    return false;
                //Path is blocked but hardcoded
                if (boardState[5, 0] != null || boardState[6, 0] != null)
                    return false;
                //Path Is checked?
                if (CheckForCheck(new Vector2Int(5, 0), "black") || CheckForCheck(new Vector2Int(6, 0), "black"))
                    return false;

                return true;
            }
        }

        //Black side castling
        if(color == "black")
        {
            //King is aigth
            if (king == null || !(king is King) || king.HasMoved)
                return false;

            //Black queenside castling
            if(to.Equals(new Vector2Int(2, 7)))
            {
                Piece queenTower = boardState[0, 7];
                //Tower is alright
                if (queenTower == null || !(queenTower is Tower) || queenTower.Color != "black" || queenTower.HasMoved)
                    return false;
                //Path is blocked but hardcoded
                if (boardState[1, 7] != null || boardState[2, 7] != null && boardState[3, 7] != null)
                    return false;
                //Path Is checked?
                if (CheckForCheck(new Vector2Int(2, 7), "white") || CheckForCheck(new Vector2Int(3, 7), "white"))
                    return false;

                return true;
            //Black kingside castling
            }else if(to.Equals(new Vector2Int(6, 7)))
            {
                Piece kingTower = boardState[7, 7];
                //Tower is alright
                if (kingTower == null || !(kingTower is Tower) || kingTower.HasMoved)
                    return false;
                //Path is blocked but hardcoded
                if (boardState[5, 7] != null && boardState[6, 7] != null)
                    return false;
                //Path Is checked?
                if (CheckForCheck(new Vector2Int(5, 7), "white") || CheckForCheck(new Vector2Int(6, 7), "white"))
                    return false;

                return true;
            }
        }
        return false;
    }
    private bool PromotionPossible(Vector2Int from)
    {
        if (boardState[from.x, from.y] is Pawn)
        {
            if ((boardState[from.x, from.y].Color == "black" && from.y == 0) || (boardState[from.x, from.y].Color == "white" && from.y == 7))
            {
                return true;
            }
        }

        return false;
    }
    private bool DoEnpassant(Vector2Int from, Vector2Int to)
    {
        //Is this enpassant Will this update the team list? is that needed?
        Piece pieceToRemove = null;
        if (EnPassantPossible(from, to))
        {
            if (boardState[from.x, from.y].Color == "black")
            {
                if (to.x > from.x && to.y == 2)
                {
                    //right side en passant
                    boardState[to.x, to.y] = boardState[from.x, from.y];
                    pieceToRemove = boardState[from.x + 1, from.y];
                    RemovePiece(pieceToRemove);
                    boardState[from.x + 1, from.y] = null;
                    boardState[from.x, from.y] = null;
                    return true;
                }
                else if (to.x < from.x && to.y == 2)
                {
                    //left side en passant
                    boardState[to.x, to.y] = boardState[from.x, from.y];
                    pieceToRemove = boardState[from.x - 1, from.y];
                    RemovePiece(pieceToRemove);
                    boardState[from.x - 1, from.y] = null;
                    boardState[from.x, from.y] = null;
                    return true;
                }  
            }
            else if (boardState[from.x, from.y].Color == "white")
            {
                if (to.x > from.x && to.y == 5)
                {
                    //right side en passant
                    boardState[to.x, to.y] = boardState[from.x, from.y];
                    pieceToRemove = boardState[from.x + 1, from.y];
                    RemovePiece(pieceToRemove);
                    boardState[from.x + 1, from.y] = null;
                    boardState[from.x, from.y] = null;
                    return true;
                }
                else if (to.x < from.x && to.y == 5)
                {
                    //left side en passant
                    boardState[to.x, to.y] = boardState[from.x, from.y];
                    pieceToRemove = boardState[from.x - 1, from.y];
                    RemovePiece(pieceToRemove);
                    boardState[from.x - 1, from.y] = null;
                    boardState[from.x, from.y] = null;
                    return true;
                }
            }
        }
        return false;
    }
    private bool DoCastling(Vector2Int from, Vector2Int to)
    {
        void doLogic(int y)
        {
            if (to.x == 2)
            {
                boardState[2, y] = boardState[4, y];
                boardState[4, y] = null;
                boardState[3, y] = boardState[0, y];
                boardState[0, y] = null;
            }

            if (to.x == 6)
            {
                boardState[6, y] = boardState[4, y];
                boardState[4, y] = null;
                boardState[5, y] = boardState[7, y];
                boardState[7, y] = null;
            }
        }
        //Is this a castle
        if (CastlingPossible(from, to))
        {
            if (boardState[from.x, from.y].Color == "white")
            {
                doLogic(0);
                return true;
            }
            else if (boardState[from.x, from.y].Color == "black")
            {
                doLogic(7);
                return true;
            }
        }
        return false;
    }
}
