using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Rook : ShogiPiece
{
    public override bool[,] PossibleMoves(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];

        // Right
        OrthagonalLine(moves, DirectionOrthagonal.right);

        // Left
        OrthagonalLine(moves, DirectionOrthagonal.left);

        // Forwards
        OrthagonalLine(moves, DirectionOrthagonal.forward);

        // Backwards
        OrthagonalLine(moves, DirectionOrthagonal.back);

        return moves;
    }
}
