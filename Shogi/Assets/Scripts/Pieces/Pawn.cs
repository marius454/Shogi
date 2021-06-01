using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Pawn : ShogiPiece
{
    public Pawn(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board) : base(x, y, player, pieceType, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Pawn - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedPawn - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetNormalRotation()
    {
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        }
    }
    protected override void SetPromotedRotation()
    {
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(85.0f, 180.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(85.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        }
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        int x = CurrentX;
        int y = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        if (!isPromoted){
            if (player == PlayerNumber.Player1)
                SingleMove(moves, x, y + 1, currentPlayer, localBoard);
            else if (player == PlayerNumber.Player2)
                SingleMove(moves, x, y - 1, currentPlayer, localBoard);
            else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        else {
            GoldMove();
        }
        
        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
    public override void RemoveIllegalDrops(bool checkForSelfCheck = true){
        // pawn cant be on the last row
        RemoveDropsLastRows();

        // cant drop pawn on a row with an unpromoted pawn
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (board.ShogiPieces[x, y]) {
                    if (board.ShogiPieces[x, y].GetType() == typeof(Pawn) && !board.ShogiPieces[x, y].isPromoted && board.ShogiPieces[x, y].player == player){
                        for (int Y=0; Y < C.numberRows; Y++){
                            drops[x, Y] = false;
                        }
                    }
                }
            }
        base.RemoveIllegalDrops(checkForSelfCheck);
        // Can't drop a pawn in place that would cause checkmate
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                int pawnMove;
                if (player == PlayerNumber.Player1 && drops[x, y]) pawnMove = 1;
                else pawnMove = -1;
                if (drops[x, y])
                    if (board.ShogiPieces[x, y+pawnMove] && board.ShogiPieces[x, y+pawnMove].GetType() == typeof(King) && board.ShogiPieces[x, y+pawnMove].player == board.opponentPlayer.playerNumber)
                        if (CheckIfDropWillCauseCheckmate(x, y)){
                            drops[x, y] = false;
                        }
            }
    }
    private bool CheckIfDropWillCauseCheckmate(int x, int y){
        bool wouldCauseCheckMate = false;

        board.ShogiPieces[x, y] = this;
        int tempX = this.CurrentX;
        int tempY = this.CurrentY;
        this.CurrentX = x;
        this.CurrentY = y;

        currentPlayer.AddPieceInPlay(this);
        currentPlayer.isAttackingKing = true;
        opponentPlayer.PlaceInCheck();

        bool tempHasPossibleMoves = opponentPlayer.hasPossibleMoves;
        bool[,] tempAttackedTile = opponentPlayer.attackedTiles.Clone() as bool[,];
        bool tempIsAttackingKing = opponentPlayer.isAttackingKing;
        opponentPlayer.CalculatePossibleMoves(true, false);

        wouldCauseCheckMate = !opponentPlayer.hasPossibleMoves;
        currentPlayer.RemovePieceInPlay(this);
        currentPlayer.isAttackingKing = false;
        opponentPlayer.RemoveCheck();
        board.ShogiPieces[x, y] = null;
        this.CurrentX = tempX;
        this.CurrentY = tempY;

        opponentPlayer.RebuildAfterPossibleMoveCalculation(tempHasPossibleMoves, tempAttackedTile, tempIsAttackingKing);
        return wouldCauseCheckMate;
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
