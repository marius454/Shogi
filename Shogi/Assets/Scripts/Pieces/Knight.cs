using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Knight : ShogiPiece
{
    public Knight(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Knight, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        if (player == PlayerNumber.Player1){
            SingleMove(moves, CurrentX + 1, CurrentY + 2);
            SingleMove(moves, CurrentX - 1, CurrentY + 2);
        }
        else if (player == PlayerNumber.Player2) {
            SingleMove(moves, CurrentX + 1, CurrentY - 2);
            SingleMove(moves, CurrentX - 1, CurrentY - 2);
        }
        else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        
        RemoveIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
    public override void RemoveIllegalDrops()
    {
        // do not allow to drop in the last two rows
        RemoveLastRow(true);
        base.RemoveIllegalDrops();
    }
}
