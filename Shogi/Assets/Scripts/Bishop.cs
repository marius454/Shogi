using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Bishop : ShogiPiece
{
    public override bool[,] PossibleMove(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];

        // Forward left
        DiagonalLine(moves, C.forwardLeft);

        // Forward right
        DiagonalLine(moves, C.forwardRight);

        // Backward left
        DiagonalLine(moves, C.backLeft);

        // Backward right
        DiagonalLine(moves, C.backRight);

        return moves;
    }
}
