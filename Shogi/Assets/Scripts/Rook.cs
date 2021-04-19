using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Rook : ShogiPiece
{
    public override bool[,] PossibleMove(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];

        // Right
        OrthagonalLine(moves, C.right);

        // Left
        OrthagonalLine(moves, C.left);

        // Forwards
        OrthagonalLine(moves, C.forward);

        // Backwards
        OrthagonalLine(moves, C.back);

        return moves;
    }
}
