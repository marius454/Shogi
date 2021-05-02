using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Rook : ShogiPiece
{
    public Rook(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        // Right
        OrthagonalLine(moves, DirectionOrthagonal.right);

        // Left
        OrthagonalLine(moves, DirectionOrthagonal.left);

        // Forwards
        OrthagonalLine(moves, DirectionOrthagonal.forward);

        // Backwards
        OrthagonalLine(moves, DirectionOrthagonal.back);

        removeIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
}
