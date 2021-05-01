using System;
using UnityEngine;
using C = Constants;

public class Pawn : ShogiPiece
{
    public override bool[,] PossibleMoves(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];

        if (player == PlayerNumber.player1)
            SingleMove(moves, CurrentX, CurrentY + 1);
        else if (player == PlayerNumber.player2)
            SingleMove(moves, CurrentX, CurrentY - 1);
        else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        
        return moves;
    }
}
