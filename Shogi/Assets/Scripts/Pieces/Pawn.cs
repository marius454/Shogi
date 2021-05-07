using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Pawn : ShogiPiece
{
    public Pawn(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Pawn, gameObject.transform.position.z);
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
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        if (player == PlayerNumber.Player1)
            SingleMove(moves, CurrentX, CurrentY + 1);
        else if (player == PlayerNumber.Player2)
            SingleMove(moves, CurrentX, CurrentY - 1);
        else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        
        RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
    public override void RemoveIllegalDrops(){
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
                int pawnMove;
                if (player == PlayerNumber.Player1 && moves[x, y]) pawnMove = 1;
                else pawnMove = -1;
                //Debug.Log(x + " " + y + " " + moves[x,y]);
                if (moves[x, y])
                    if (board.ShogiPieces[x, y+pawnMove] && board.ShogiPieces[x, y+pawnMove].GetType() == typeof(King) && board.ShogiPieces[x, y+pawnMove].player == PlayerNumber.Player2)
                        if (CheckIfDropWillCauseCheckmate(x, y)){
                            moves[x, y] = false;
                        }
            }
        base.RemoveIllegalDrops();
    }
    private bool CheckIfDropWillCauseCheckmate(int x, int y){
        bool wouldCauseCheckMate;

        // Player opponentPlayer;
        // Player currentPlayer;
        // if (player == board.player1.playerNumber){
        //     currentPlayer = board.player1;
        //     opponentPlayer = board.player2;
        // } 
        // else{
        //     currentPlayer = board.player2;
        //     opponentPlayer = board.player1;
        // } 
        GameObject clone = Instantiate(board.piecePrefabs[(int)player], board.GetTileCenter(x, y), Quaternion.Euler(0,0,0)) as GameObject;
        board.ShogiPieces[x, y] = clone.GetComponent<ShogiPiece>();
        board.ShogiPieces[x, y].Init(x, y, player, board);

        board.currentPlayer.AddPieceInPlay(clone.GetComponent<ShogiPiece>());
        board.opponentPlayer.CalculateAttackedTiles();

        for (int i=0; i < C.numberRows; i++)
            for (int j=0; j < C.numberRows; j++){
                if (board.opponentPlayer.attackedTiles[i,j]){
                    Debug.Log(i + " " + j);
                }
            }

        wouldCauseCheckMate = !board.opponentPlayer.hasPossibleMoves;
        //Debug.Log(wouldCauseCheckMate);
        board.currentPlayer.RemovePieceInPlay(clone.GetComponent<ShogiPiece>());
        board.ShogiPieces[x, y] = null;
        Destroy (clone);

        board.opponentPlayer.CalculateAttackedTiles();
        // RemoveLastRow();

        return wouldCauseCheckMate;
    }
}
