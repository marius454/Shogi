using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Bishop : ShogiPiece
{
    public Bishop(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board) : base(x, y, player, pieceType, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Bishop - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedBishop - 0.01f, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        int x = CurrentX;
        int y = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        // Forward left
        DiagonalLine(moves, DirectionDiagonal.forwardLeft, currentPlayer, localBoard, x, y);
        // Forward right
        DiagonalLine(moves, DirectionDiagonal.forwardRight, currentPlayer, localBoard, x, y);
        // Backward left
        DiagonalLine(moves, DirectionDiagonal.backLeft, currentPlayer, localBoard, x, y);
        // Backward right
        DiagonalLine(moves, DirectionDiagonal.backRight, currentPlayer, localBoard, x, y);

        if (isPromoted){
            SingleMove(moves, x, y + 1, currentPlayer, localBoard);
            SingleMove(moves, x, y - 1, currentPlayer, localBoard);
            SingleMove(moves, x + 1, y, currentPlayer, localBoard);
            SingleMove(moves, x - 1, y, currentPlayer, localBoard);
        }

        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
}
