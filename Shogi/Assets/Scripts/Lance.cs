using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Lance : ShogiPiece
{
    public override bool[,] PossibleMove(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];

        // Player 1
        if (player == C.player1)
            OrthagonalLine(moves, C.forward);
        // Player 2
        else OrthagonalLine(moves, C.back);

        return moves;
    }
}
