using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Pawn : ShogiPiece
{
    public Pawn(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Pawn - 0.01f, gameObject.transform.position.z);
    }
    public override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedPawn - 0.01f, gameObject.transform.position.z);
    }
    public override void SetNormalRotation()
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
    public override void SetPromotedRotation()
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
        moves = new bool[C.numberRows,C.numberRows];

        if (!isPromoted){
            if (player == PlayerNumber.Player1)
                SingleMove(moves, CurrentX, CurrentY + 1);
            else if (player == PlayerNumber.Player2)
                SingleMove(moves, CurrentX, CurrentY - 1);
            else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        else {
            GoldMove();
        }
        
        RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
    public override void RemoveIllegalDrops(bool checkForPawnDropMate){
        // pawn cant be on the last row
        RemoveLastRow();

        // cant drop pawn on a row with an unpromoted pawn
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (board.ShogiPieces[x, y]) {
                    if (board.ShogiPieces[x, y].GetType() == typeof(Pawn) && !board.ShogiPieces[x, y].isPromoted){
                        for (int Y=0; Y < C.numberRows; Y++){
                            moves[x, Y] = false;
                        }
                    }
                }
                // cant drop a pawn in place that would cause checkmate
                if (checkForPawnDropMate){
                    int pawnMove;
                    if (player == PlayerNumber.Player1 && moves[x, y]) pawnMove = 1;
                    else pawnMove = -1;
                    if (moves[x, y])
                        if (board.ShogiPieces[x, y+pawnMove] && board.ShogiPieces[x, y+pawnMove].GetType() == typeof(King) && board.ShogiPieces[x, y+pawnMove].player == PlayerNumber.Player2)
                            if (CheckIfDropWillCauseCheckmate(x, y)){
                                moves[x, y] = false;
                            }
                }
            }
        base.RemoveIllegalDrops(checkForPawnDropMate);
    }
    private bool CheckIfDropWillCauseCheckmate(int x, int y){
        bool wouldCauseCheckMate;

        GameObject clone = Instantiate(board.piecePrefabs[(int)player], board.GetTileCenter(x, y), Quaternion.Euler(0,0,0)) as GameObject;
        board.ShogiPieces[x, y] = clone.GetComponent<ShogiPiece>();
        board.ShogiPieces[x, y].Init(x, y, player, board);

        board.currentPlayer.AddPieceInPlay(clone.GetComponent<ShogiPiece>());
        board.opponentPlayer.CalculateAttackedTiles(true, false);

        wouldCauseCheckMate = !board.opponentPlayer.hasPossibleMoves;
        board.currentPlayer.RemovePieceInPlay(clone.GetComponent<ShogiPiece>());
        board.ShogiPieces[x, y] = null;
        Destroy (clone);

        board.opponentPlayer.CalculateAttackedTiles(true, false);
        // RemoveLastRow();

        return wouldCauseCheckMate;
    }
}
