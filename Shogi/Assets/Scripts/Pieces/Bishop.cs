using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Bishop : ShogiPiece
{
    public Bishop(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Bishop - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedBishop - 0.01f, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        // Forward left
        DiagonalLine(moves, DirectionDiagonal.forwardLeft);
        // Forward right
        DiagonalLine(moves, DirectionDiagonal.forwardRight);
        // Backward left
        DiagonalLine(moves, DirectionDiagonal.backLeft);
        // Backward right
        DiagonalLine(moves, DirectionDiagonal.backRight);

        if (isPromoted){
            SingleMove(moves, CurrentX, CurrentY + 1);
            SingleMove(moves, CurrentX, CurrentY - 1);
            SingleMove(moves, CurrentX + 1, CurrentY);
            SingleMove(moves, CurrentX - 1, CurrentY);
        }

        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
}
