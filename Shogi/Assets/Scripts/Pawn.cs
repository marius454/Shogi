using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Pawn : ShogiPiece
{
    public override bool[,] PossibleMove(){
        bool[,] moves = new bool[C.numberRows,C.numberRows];
        
        // ShogiPiece potentialTile;

        if (player == C.player1){
            SingleMove(moves, CurrentX, CurrentY + 1);
            // if (CurrentY != C.numberRows-1){
            //     potentialTile = BoardManager.Instance.ShogiPieces[CurrentX, CurrentY + 1];
            //     if (potentialTile == null || potentialTile.player == C.player2){
            //         moves[CurrentX, CurrentY + 1] = true;
            //     }
            // }
        }
        else {
            SingleMove(moves, CurrentX, CurrentY - 1);
            // if (CurrentY != 0){
            //     potentialTile = BoardManager.Instance.ShogiPieces[CurrentX, CurrentY - 1];
            //     if (potentialTile == null || potentialTile.player == C.player1){
            //         moves[CurrentX, CurrentY - 1] = true;
            //     }
            // }
        }
        return moves;
    }
}
