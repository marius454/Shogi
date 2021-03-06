using System;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class King : ShogiPiece
{
    public King(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board) : base(x, y, player, pieceType, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.King - 0.01f, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        int x = CurrentX;
        int y = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        // Select all moves in a 3x3 square around the piece, except it's current position
        int a = 1;
        for (int t = -a; t <= a; t++)
            for (int s = -a; s <= a; s++){
                if (!(t == 0 && s == 0))
                    SingleMove(moves, x + t, y + s, currentPlayer, localBoard);
            }

        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
    public override void CheckForPromotion(){
        // King cannot be promoted
    }
    public override void Promote(bool changePosition = true)
    {
        // King cannot be promoted
    }
    public override void Unpromote(bool changePosition = true)
    {
        // King will never be promoted to be unpromoted
    }
    public override bool CheckIfCouldBePromoted(int y){
        // King cannot be promoted
        return false;
    }

    public override bool IsAttacked(){
        bool[,] possibleLocations = new bool[C.numberRows, C.numberRows];
        int a = 1;
        int localX = CurrentX;
        int localY = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        // Mark all possible locations from where an enemy piece might attack and check if the the piece in that location has a move that can attack
        if (player == PlayerNumber.Player1){
            DiagonalLine(possibleLocations, DirectionDiagonal.forwardLeft, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            DiagonalLine(possibleLocations, DirectionDiagonal.forwardRight, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            DiagonalLine(possibleLocations, DirectionDiagonal.backLeft, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            DiagonalLine(possibleLocations, DirectionDiagonal.backRight, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            
            OrthagonalLine(possibleLocations, DirectionOrthagonal.forward, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            OrthagonalLine(possibleLocations, DirectionOrthagonal.left, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            OrthagonalLine(possibleLocations, DirectionOrthagonal.right, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            OrthagonalLine(possibleLocations, DirectionOrthagonal.back, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}

            for (int s = a; s >= -a; s--)
                for (int t = a; t >= -a; t--){
                    if (!(t == 0 && s == 0)){
                        SingleMove(possibleLocations, localX + t, localY + s, currentPlayer, localBoard);
                        if (localX + t >= 0 && localY + s >= 0 && localX + t < C.numberRows && localY + s < C.numberRows){
                            if (CheckForAttacker(possibleLocations[localX + t, localY + s], board.ShogiPieces[localX + t, localY + s], t, s)){
                                return true;
                            }
                        }
                    }
                }
        }
        else{
            DiagonalLine(possibleLocations, DirectionDiagonal.backLeft, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            DiagonalLine(possibleLocations, DirectionDiagonal.backRight, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            DiagonalLine(possibleLocations, DirectionDiagonal.forwardLeft, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            DiagonalLine(possibleLocations, DirectionDiagonal.forwardRight, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            
            OrthagonalLine(possibleLocations, DirectionOrthagonal.back, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            OrthagonalLine(possibleLocations, DirectionOrthagonal.right, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            OrthagonalLine(possibleLocations, DirectionOrthagonal.left, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}
            OrthagonalLine(possibleLocations, DirectionOrthagonal.forward, currentPlayer, localBoard, localX, localY, true);
            if (isAttacked) {isAttacked = false; return true;}

            for (int s = -a; s <= a; s++)
                for (int t = -a; t <= a; t++){
                    if (!(t == 0 && s == 0)){
                        SingleMove(possibleLocations, localX + t, localY + s, currentPlayer, localBoard);
                        if (localX + t >= 0 && localY + s >= 0 && localX + t < C.numberRows && localY + s < C.numberRows){
                            if (CheckForAttacker(possibleLocations[localX + t, localY + s], board.ShogiPieces[localX + t, localY + s], t, s)){
                                return true;
                            }
                        }
                    }
                }
        }
        int y = 2;
        if (player == PlayerNumber.Player2) y = -2;
        if (localX - 1 >= 0 && localY + y >= 0 && localX - 1 < C.numberRows && localY + y < C.numberRows){
            SingleMove(possibleLocations, localX - 1, localY + y, currentPlayer, localBoard);
            if (CheckForAttacker(possibleLocations[localX - 1, localY + y], board.ShogiPieces[localX - 1, localY + y], -1, y)){
                return true;
            }
        }
        if (localX + 1 >= 0 && localY + y >= 0 && localX + 1 < C.numberRows && localY + y < C.numberRows){
            SingleMove(possibleLocations, localX + 1, localY + y, currentPlayer, localBoard);
            if (CheckForAttacker(possibleLocations[localX + 1, localY + y], board.ShogiPieces[localX + 1, localY + y], 1, y)){
                return true;
            }
        }
        return false;
    }
    private bool CheckForAttacker(bool isPossibleLocation, ShogiPiece piece, int t, int s){
        if (!isPossibleLocation) return false;
        if (!piece) return false;

        if (s != 2 && s != -2){
            if ((piece.GetType() == typeof(Rook) && piece.isPromoted) 
             || (piece.GetType() == typeof(Bishop) && piece.isPromoted)
             || (piece.GetType() == typeof(King)))
                return true;
        }
        if (player == PlayerNumber.Player2){
            t = -t;
            s = -s;
        }
        if ((t==-1 && s==1) || (t==1 && s==1)){
            if ((piece.GetType() == typeof(Pawn) && piece.isPromoted) 
                || (piece.GetType() == typeof(Knight) && piece.isPromoted)
                || (piece.GetType() == typeof(Lance) && piece.isPromoted)
                || (piece.GetType() == typeof(SilverGeneral))
                || (piece.GetType() == typeof(GoldGeneral)))
                    return true;
        }
        else if ((t==0 && s==1)){
            if ((piece.GetType() == typeof(Pawn)) 
                || (piece.GetType() == typeof(Knight) && piece.isPromoted)
                || (piece.GetType() == typeof(Lance))
                || (piece.GetType() == typeof(SilverGeneral))
                || (piece.GetType() == typeof(GoldGeneral)))
                    return true;
        }
        else if ((t==-1 && s==0) || (t==0 && s==-1) || (t==1 && s==0)){
            if ((piece.GetType() == typeof(Pawn) && piece.isPromoted) 
                || (piece.GetType() == typeof(Knight) && piece.isPromoted)
                || (piece.GetType() == typeof(Lance) && piece.isPromoted)
                || (piece.GetType() == typeof(SilverGeneral) && piece.isPromoted)
                || (piece.GetType() == typeof(GoldGeneral)))
                    return true;
        }
        else if ((t==-1 && s==-1) || (t==1 && s==-1)){
            if (piece.GetType() == typeof(SilverGeneral) && !piece.isPromoted)
                    return true;
        }
        else if ((t==-1 && s==2) || (t==1 && s==2)){
            if (piece.GetType() == typeof(Knight) && !piece.isPromoted)
                    return true;
        }
        return false;
    }
}
