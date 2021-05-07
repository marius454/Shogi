using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Lance : ShogiPiece
{
    public Lance(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Lance, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        if (player == PlayerNumber.Player1)
            OrthagonalLine(moves, DirectionOrthagonal.forward);
        else if (player == PlayerNumber.Player2) 
            OrthagonalLine(moves, DirectionOrthagonal.back);
        else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");

        RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
    public override void RemoveIllegalDrops()
    {
        RemoveLastRow();
        base.RemoveIllegalDrops();
    }
}
