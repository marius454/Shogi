using System;
using UnityEngine;
using C = Constants;

public class GoldGeneral : ShogiPiece
{
    public override bool[,] PossibleMoves(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];
        int a = 1;


        // Select all moves in a 3x3 square around the piece, except it's current position and the tile it cannot go
        if (player == PlayerNumber.player2){
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == -1) && !(t == -1 && s == -1))
                        SingleMove(moves, CurrentX + t, CurrentY + s);
                }
        }
        else if (player == PlayerNumber.player2) {
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == 1) && !(t == -1 && s == 1))
                        SingleMove(moves, CurrentX + t, CurrentY + s);
                }
        }
        else {
            throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        
        return moves;
    }
}
