using System.Collections;
using System.Collections.Generic;

public class Piece
{
    protected GameBoard MyBoard;
    public string Color;
    protected Vector2Int MyPosition{ get { return MyBoard.GetPiecePosition(this); } }
    protected Piece[,] BoardState { get { return MyBoard.GetBoardState(); } }

    public bool HasMoved = false;

    virtual public bool CheckMove(Vector2Int move)
    {
        return false;
    }

    virtual public bool PathIsBlockedDirectional(Vector2Int start, Vector2Int direction, int steps)
    {
        for (int i = 0; i < steps - 1; i++)
        {
            start = start + direction;
            if (BoardState[start.x, start.y] != null)
            {
                return true;
            }
        }

        return false;
    }

    virtual public bool CheckEndPos(Vector2Int pos)
    {
        if (BoardState[pos.x, pos.y] == null || BoardState[pos.x, pos.y].Color != this.Color)
        {
            return true;
        }

        return false;
    }

}
