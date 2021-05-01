using System;
using UnityEngine;
using C = Constants;

public class Lance : ShogiPiece
{
    public override bool[,] PossibleMoves(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];

        // Player 1
        if (player == PlayerNumber.player1)
            OrthagonalLine(moves, DirectionOrthagonal.forward);
        // Player 2
        else if (player == PlayerNumber.player2) 
            OrthagonalLine(moves, DirectionOrthagonal.back);

        return moves;
    }
}
