using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Lance : ShogiPiece
{
    public Lance(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board) : base(x, y, player, pieceType, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Lance - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedLance - 0.01f, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        int x = CurrentX;
        int y = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        if (!isPromoted){
            if (player == PlayerNumber.Player1)
                OrthagonalLine(moves, DirectionOrthagonal.forward, currentPlayer, localBoard, x, y);
            else if (player == PlayerNumber.Player2) 
                OrthagonalLine(moves, DirectionOrthagonal.back, currentPlayer, localBoard, x, y);
        }
        else{
            GoldMove();
        }

        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
    public override void RemoveIllegalDrops(bool checkForSelfCheck = true)
    {
        RemoveDropsLastRows();
        base.RemoveIllegalDrops(checkForSelfCheck);
    }
    public override void CheckForPromotion(){
        if (!isPromoted){
            if (player == PlayerNumber.Player1){
                if (CurrentY >= C.numberRows - 1){
                    board.PromotePiece(this);
                    board.EndTurn();
                } 
                else GameUI.Instance.ShowPromotionMenu(this);
            }
            else{
                if (CurrentY <= 0){
                    board.PromotePiece(this);
                    board.EndTurn();
                } 
                else GameUI.Instance.ShowPromotionMenu(this);
            }
        }
        
    }
    public override bool CheckIfCouldBePromoted(int y){
        if (player == PlayerNumber.Player1){
            if (y >= C.numberRows - 3) {
                if (y >= C.numberRows - 1) return false;
                return true;
            }
            return false;
        }
        else{
            if (y <= 2) {
                if (y <= 0) return false;
                return true;
            }
            return false;
        }
    }
}
