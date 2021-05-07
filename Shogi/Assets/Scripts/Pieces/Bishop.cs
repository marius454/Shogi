using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Bishop : ShogiPiece
{
    public Bishop(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Bishop, gameObject.transform.position.z);
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

        RemoveIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
}
