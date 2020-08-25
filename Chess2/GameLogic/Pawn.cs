using System.Collections;
using System.Collections.Generic;

public class Pawn : Piece
{
    public Pawn(GameBoard gameBoard, string color)
    {
        MyBoard = gameBoard;
        Color = color;
    }

    //Check number of steps

    //Check direction

    //Check endPos

    //Check if blocked

    public override bool CheckMove(Vector2Int move)
    {
        Vector2Int numberOfSteps = move - this.MyPosition;

        //White pieces
        if (this.Color == "white")
        {
            //Movement
            //One step
            if (numberOfSteps.y == 1 && numberOfSteps.x == 0 && BoardState[move.x, move.y] == null)
            {
                return true;
            }

            //two step
            if(MyPosition.y == 1 && numberOfSteps.y == 2 && numberOfSteps.x == 0 && BoardState[move.x, move.y] == null && BoardState[move.x, move.y - 1] == null)
            {
                return true;
            }
            //Attacking TopRight!
            if (numberOfSteps.y == 1 && numberOfSteps.x == 1 && BoardState[move.x, move.y] != null && BoardState[move.x, move.y].Color == "black")
            {
                return true;
            }
            //Attacking TopLeft!
            if (numberOfSteps.y == 1 && numberOfSteps.x == -1 && BoardState[move.x, move.y] != null && BoardState[move.x, move.y].Color == "black")
            {
                return true;
            }
        }

        //Black pieces
        if (this.Color == "black")
        {
            //One step
            if (numberOfSteps.y == -1 && numberOfSteps.x == 0 && BoardState[move.x, move.y] == null)
            {
                return true;
            }

            //two step
            if (MyPosition.y == 6 && numberOfSteps.y == -2 && BoardState[move.x, move.y] == null && BoardState[move.x, move.y + 1] == null)
            {
                return true;
            }
            //Attacking DownRight!
            if (numberOfSteps.y == -1 && numberOfSteps.x == 1 && BoardState[move.x, move.y] != null && BoardState[move.x, move.y].Color == "white")
            {
                return true;
            }
            //Attacking DownLeft!
            if (numberOfSteps.y == -1 && numberOfSteps.x == -1 && BoardState[move.x, move.y] != null && BoardState[move.x, move.y].Color == "white")
            {
                return true;
            }
        }

        return false;
    }

}
