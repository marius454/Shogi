using System;
using UnityEngine;
using C = Constants;

public class King : ShogiPiece
{
    public override bool[,] PossibleMoves(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];
        int a = 1;

        // Select all moves in a 3x3 square around the piece, except it's current position
        for (int t = -a; t <= a; t++)
            for (int s = -a; s <= a; s++){
                if (!(t == 0 && s == 0))
                    SingleMove(moves, CurrentX + t, CurrentY + s);
            }

        return moves;
    }
}
