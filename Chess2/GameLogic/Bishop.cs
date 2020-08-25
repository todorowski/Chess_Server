using System.Collections;
using System.Collections.Generic;
using System;

public class Bishop : Piece
{
    public Bishop(GameBoard gameBoard, string c)
    {
        MyBoard = gameBoard;
        Color = c;
    }

    public bool IsDiagonal(Vector2Int MyPosition, Vector2Int move)
    {
        if(Math.Abs(MyPosition.x - move.x) == Math.Abs(MyPosition.y - move.y))
        {
            return true;
        }
        return false;
    }

    public Vector2Int GetDirection(Vector2Int currentPos, Vector2Int move)
    {
        Vector2Int direction = move - currentPos;

        if (direction.x > 0 && direction.y > 0)
        {
            direction = new Vector2Int(1, 1);
        }
        else if (direction.x > 0 && direction.y < 0)
        {
            direction = new Vector2Int(1, -1);
        }
        else if (direction.x < 0 && direction.y > 0)
        {
            direction = new Vector2Int(-1, 1);
        }
        else if (direction.x < 0 && direction.y < 0)
        {
            direction = new Vector2Int(-1, -1);
        }

        return direction;
    }

    public override bool CheckMove(Vector2Int move)
    {
        if (!IsDiagonal(MyPosition, move))
        {
            return false;
        }
        int steps = Math.Abs(move.x - MyPosition.x);
        return !PathIsBlockedDirectional(MyPosition, GetDirection(MyPosition, move), steps) && IsDiagonal(MyPosition, move) && CheckEndPos(move);    
    }

}
