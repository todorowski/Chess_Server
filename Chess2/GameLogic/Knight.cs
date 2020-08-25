using System.Collections;
using System.Collections.Generic;
using System;

public class Knight : Piece
{
    
    public Knight(GameBoard gameBoard, string c)
    {
        MyBoard = gameBoard;
        Color = c;

    }

    public override bool CheckMove(Vector2Int move)
    {
        Vector2Int numberOfSteps = move - this.MyPosition;
        bool horizontal = Math.Abs(numberOfSteps.x) == 2 && Math.Abs(numberOfSteps.y) == 1;
        bool vertical = Math.Abs(numberOfSteps.y) == 2 && Math.Abs(numberOfSteps.x) == 1;

        return (horizontal || vertical) && !(BoardState[move.x, move.y] != null && BoardState[move.x, move.y].Color == this.Color);
    }

}
