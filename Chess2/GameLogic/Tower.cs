using System;
using System.Collections;
using System.Collections.Generic;

public class Tower : Piece
{
    public Tower(GameBoard gameBoard, string c)
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

    public Vector2Int GetDirection(Vector2Int currentPos, Vector2Int move)
    {
        Vector2Int direction = move - currentPos;

        if (direction.x > 0 && direction.y == 0)
        {
            direction = new Vector2Int(1, 0);
        }
        else if (direction.x == 0 && direction.y > 0)
        {
            direction = new Vector2Int(0, 1);
        }
        else if (direction.x < 0 && direction.y == 0)
        {
            direction = new Vector2Int(-1, 0);
        }
        else if (direction.x == 0 && direction.y < 0)
        {
            direction = new Vector2Int(0, -1);
        }

        return direction;
    }

    public bool Check(Vector2Int move)
    {
        Vector2Int testPos = new Vector2Int(move.x, move.y);
        //try adding stuff to this and see if its a legal move 
        return false;
    }

    public override bool CheckMove(Vector2Int move)
    {
        Vector2Int delta = move - MyPosition;
        if (!IsOrthogonal(MyPosition, move)) 
            return false;
        int steps = Math.Max(Math.Abs(delta.x), Math.Abs(delta.y));
        return !PathIsBlockedDirectional(MyPosition, GetDirection(MyPosition, move), steps) && CheckEndPos(move);
    }
}
