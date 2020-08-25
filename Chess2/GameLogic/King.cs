using System.Collections;
using System.Collections.Generic;
using System;

public class King : Piece
{
    public King(GameBoard gameBoard, string c)
    {
        MyBoard = gameBoard;
        Color = c;
    }

    public Vector2Int GetDirection(Vector2Int currentPos, Vector2Int move)
    {
        Vector2Int delta = move - currentPos;
        Vector2Int direction = new Vector2Int((int)Math.Sign(delta.x), (int)Math.Sign(delta.y));

        if (delta.x == 0)
        {
            direction.x = 0;
        }
        if (delta.y == 0)
        {
            direction.y = 0;
        }

        return direction;
    }

    public override bool CheckEndPos(Vector2Int move)
    {
        if (BoardState[move.x, move.y] == null || BoardState[move.x, move.y].Color != this.Color)
        {
            return true;
        }
        return false;
    }

    public override bool CheckMove(Vector2Int move)
    {
        int steps = Math.Max(Math.Abs(move.x - MyPosition.x), Math.Abs(move.y - MyPosition.y));
        if (steps != 1)
        {
            return false;
        }
        return CheckEndPos(move);
    }
}
