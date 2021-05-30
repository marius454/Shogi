using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Rook : ShogiPiece
{
    public Rook(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board) : base(x, y, player, pieceType, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Rook - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedRook - 0.01f, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        int x = CurrentX;
        int y = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        // Right
        OrthagonalLine(moves, DirectionOrthagonal.right, currentPlayer, localBoard, x, y);
        // Left
        OrthagonalLine(moves, DirectionOrthagonal.left, currentPlayer, localBoard, x, y);
        // Forwards
        OrthagonalLine(moves, DirectionOrthagonal.forward, currentPlayer, localBoard, x, y);
        // Backwards
        OrthagonalLine(moves, DirectionOrthagonal.back, currentPlayer, localBoard, x, y);
        
        if (isPromoted){
            SingleMove(moves, x + 1, y + 1, currentPlayer, localBoard);
            SingleMove(moves, x - 1, y + 1, currentPlayer, localBoard);
            SingleMove(moves, x + 1, y - 1, currentPlayer, localBoard);
            SingleMove(moves, x - 1, y - 1, currentPlayer, localBoard);
        }

        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
}
