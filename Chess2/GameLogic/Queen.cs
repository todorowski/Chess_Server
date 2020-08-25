using System;
using System.Collections;
using System.Collections.Generic;

public class Queen : Piece
{
    public Queen(GameBoard gameBoard, string c)
    {
        MyBoard = gameBoard;
        Color = c;
    }

    private bool IsOrthogonal(Vector2Int MyPosition, Vector2Int move)
    {
        int xabs = Math.Abs(MyPosition.x - move.x);
        int yabs = Math.Abs(MyPosition.y - move.y);
        return xabs == 0 || yabs == 0;
    }

    private bool IsDiagonal(Vector2Int MyPosition, Vector2Int move)
    {
        if (Math.Abs(MyPosition.x - move.x) == Math.Abs(MyPosition.y - move.y))
        {
            return true;
        }
        return false;
    }

    public Vector2Int GetDirection(Vector2Int currentPos, Vector2Int move)
    {
        Vector2Int delta = move - currentPos;
        Vector2Int direction = new Vector2Int((int)Math.Sign(delta.x), (int)Math.Sign(delta.y));

        if (delta.x == 0)
        {
            direction.x = 0;
        }
        if(delta.y == 0)
        {
            direction.y = 0;
        }

        return direction;
    }

    public override bool CheckMove(Vector2Int move)
    {
        if (!IsOrthogonal(MyPosition, move) && !IsDiagonal(MyPosition, move))
        {
            return false;
        }

        Vector2Int delta = move - MyPosition;
        int steps = Math.Max(Math.Abs(delta.x), Math.Abs(delta.y));
        return !PathIsBlockedDirectional(MyPosition, GetDirection(MyPosition, move), steps) && CheckEndPos(move);
    }
}
