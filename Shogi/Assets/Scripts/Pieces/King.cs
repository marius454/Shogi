using System;
using UnityEngine;
using C = Constants;

public class King : ShogiPiece
{
    public King(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];
        int a = 1;

        // Select all moves in a 3x3 square around the piece, except it's current position
        for (int t = -a; t <= a; t++)
            for (int s = -a; s <= a; s++){
                if (!(t == 0 && s == 0))
                    SingleMove(moves, CurrentX + t, CurrentY + s);
            }

        removeIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
}
