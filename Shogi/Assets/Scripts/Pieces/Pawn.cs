using System;
using UnityEngine;
using C = Constants;

public class Pawn : ShogiPiece
{
    public Pawn(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        if (player == PlayerNumber.player1)
            SingleMove(moves, CurrentX, CurrentY + 1);
        else if (player == PlayerNumber.player2)
            SingleMove(moves, CurrentX, CurrentY - 1);
        else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        
        removeIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
}