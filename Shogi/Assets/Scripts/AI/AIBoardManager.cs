using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public struct Move{
    public int pieceX;
    public int pieceY;
    public int targetX;
    public int targetY;
    public bool promote;
}
public class AIBoardManager : BoardManager
{
    private System.Random random;
    public ShogiPlayer AIPlayer;
    private bool shouldEndTurn;
    private bool gameOver;
    private void Awake(){

        random = new System.Random();
        Instance = this;
    }
    public override void InitializeGame()
    {
        base.InitializeGame();
        gameOver = false;
        AIPlayer = player2; // later will need to be changed so that the user can chose
        shouldEndTurn = true;
    }
    private void Update(){
        currentPlayer.CalculatePossibleMoves();
        if (!gameOver){
            if (currentPlayer != AIPlayer && shouldEndTurn){
                DoMove(RandomMove(currentPlayer));
                ChangePlayer();
            }
            else AIMove();
        }
    }
    protected override void SelectShogiPiece(int x, int y){
        if (ShogiPieces[x, y] == null)
            return;
        if (ShogiPieces[x, y].player != currentPlayer.playerNumber && !freeMove)
            return;
        base.SelectShogiPiece(x, y);
    }
    // protected override void OnShogiPieceMove(int x, int y)
    // {
    //     base.OnShogiPieceMove(x, y);
    //     if (currentPlayer != AIPlayer)
    //         AIMove();
    // }
    private void AIMove(){
        Move bestMove;
        //ChangePlayer();
        shouldEndTurn = false;
        // bestMove = RandomMove();
        bestMove = BoardEvaluation.MiniMax(this, AIPlayer);
        shouldEndTurn = true;
        if (bestMove.pieceX >= 0 && bestMove.pieceY >= 0 && bestMove.pieceX <= C.numberRows && bestMove.pieceY <= C.numberRows)
            Debug.Log(bestMove + " " + ShogiPieces[bestMove.pieceX, bestMove.pieceY]);
        else Debug.Log(bestMove);
        DoMove(bestMove);
        // GC.Collect();
        // Debug.Break();

        EndTurn();
        ChangePlayer();
    }
    public void DoMove(Move move){
        if (move.pieceX >= 0 && move.pieceY >= 0 && move.pieceX <= C.numberRows && move.pieceY <= C.numberRows){
            SelectShogiPiece(move.pieceX, move.pieceY);
            allowedMoves[move.targetX, move.targetY] = true;
            MoveShogiPiece(move.targetX, move.targetY);

            if (move.promote){
                if (ShogiPieces[move.targetX, move.targetY] == null){
                    bool[,] moves = ShogiPieces[move.pieceX, move.pieceY].PossibleMoves();
                    Debug.Log(moves);
                    Debug.Log("Null after move");
                    Debug.Break();
                }
                PromotePiece(ShogiPieces[move.targetX, move.targetY]);
            }
        }
        else if (move.pieceX >= currentPlayer.captureBoard.minX && move.pieceY >= currentPlayer.captureBoard.minY 
          && move.pieceX <= currentPlayer.captureBoard.maxX && move.pieceY <= currentPlayer.captureBoard.maxY){
              SelectCapturedPiece(move.pieceX, move.pieceY);
              DropShogiPiece(move.targetX, move.targetY);
        }
    }

    public void RestoreGameState(BoardState boardState){
        // for (int x = 0; x < C.numberRows; x++)
        //     for (int y = 0; y < C.numberRows; y++){
        //         ShogiPieces[x, y] = null;
        //     }
        
        // // ShogiPieces = new ShogiPiece[C.numberRows, C.numberRows];
        // foreach(var piece in boardState.shogiPieceState){
        //     ShogiPieces[piece.x, piece.y] = allPieces[piece.pieceIndex];
        //     ShogiPieces[piece.x, piece.y].Init(piece.x, piece.y, piece.playerNumber, piece.pieceType, this);
        //     if (piece.promoted) ShogiPieces[piece.x, piece.y].Promote();
        // }
        selectedShogiPiece = null;
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                var piece = boardState.shogiPieceState[x, y];
                if (piece.pieceIndex >= 0){
                    ShogiPieces[x, y] = allPieces[piece.pieceIndex];
                    ShogiPieces[x, y].Init(x, y, piece.playerNumber, piece.pieceType, this);
                    if (piece.promoted) ShogiPieces[x, y].Promote();
                }
                else if (ShogiPieces[x, y]) ShogiPieces[x, y] = null;
            }

        RestorePlayer (player1, boardState.captureBoardPlayer1State);
        RestorePlayer (player2, boardState.captureBoardPlayer2State);
    }
    private void RestorePlayer(ShogiPlayer player, (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)[,] captureBoard){
        player.InitializePiecesInPlay();
        // bool thereAreRestorableCaptures = false;
        // for (int x = 0; x < C.captureNumberColumns; x++)
        //     for (int y = 0; y < C.captureNumberRows; y++){
        //         if (captureBoard[x, y].pieceIndex >= 0){
        //             thereAreRestorableCaptures = true;
        //             break;
        //         }
        //     }

        // if (thereAreRestorableCaptures){
        for (int x = 0; x < C.captureNumberColumns; x++)
            for (int y = 0; y < C.captureNumberRows; y++){
                var piece = captureBoard[x, y];
                if (piece.pieceIndex >= 0){
                    player.captureBoard.capturedPieces[x, y] = allPieces[piece.pieceIndex];
                    player.captureBoard.capturedPieces[x, y].Init(piece.x, piece.y, piece.playerNumber, piece.pieceType, this);
                    player.captureBoard.capturedPieces[x, y].Unpromote();
                }
                else player.captureBoard.capturedPieces[piece.x, piece.y] = null;
            }
        // foreach(var piece in captureBoard){
        //     player.captureBoard.capturedPieces[piece.x, piece.y] = allPieces[piece.pieceIndex];
        // }
        player.InitializeCapturedPieces();
        player.CalculatePossibleMoves();
    }
    protected override void CapturePieceIfPossible(int x, int y)
    {
        base.CapturePieceIfPossible(x, y);
    }

    public Move RandomMove(ShogiPlayer player){

        int randomMove = random.Next(player.possibleMoves.Count);
        int pieceX = player.possibleMoves[randomMove].pieceX;
        int pieceY = player.possibleMoves[randomMove].pieceY;
        int targetX = player.possibleMoves[randomMove].targetX;
        int targetY = player.possibleMoves[randomMove].targetY;
        bool promote = player.possibleMoves[randomMove].promote;

        return new Move{pieceX = pieceX, pieceY = pieceY, targetX = targetX, targetY = targetY, promote = promote};
    }
    protected override void OnTurnEnd()
    {
        selectedShogiPiece = null;
        // Check win condition
        if (shouldEndTurn){
            if (CheckIfGameOver()){
                EndGame();
                gameOver = true;
            }
            else if (CheckForRepetitionDraw()){
                EndGame("Repetition Draw");
            }
            // if (currentPlayer != AIPlayer)
            //     AIMove();
        }
    }
    protected override void CheckForPromotion()
    {
        if (currentPlayer != AIPlayer)
            base.CheckForPromotion();
        else {
            if (AIPlayer == player1){
                if ((selectedShogiPiece is Pawn) || (selectedShogiPiece is Lance)){
                    if (selectedShogiPiece.CurrentY >= C.numberRows - 1){
                        PromotePiece(selectedShogiPiece);
                    }
                }
                else if (selectedShogiPiece is Knight){
                    if (selectedShogiPiece.CurrentY >= C.numberRows - 2){
                        PromotePiece(selectedShogiPiece);
                    }
                }
            }
            else {
               if ((selectedShogiPiece is Pawn) || (selectedShogiPiece is Lance)){
                    if (selectedShogiPiece.CurrentY <= 0){
                        PromotePiece(selectedShogiPiece);
                    }
                }
                else if (selectedShogiPiece is Knight){
                    if (selectedShogiPiece.CurrentY <= 1){
                        PromotePiece(selectedShogiPiece);
                    }
                } 
            }
        }
    }
    protected override void OnShogiPieceDrop(int x, int y){
        if (allowedMoves[x,y]){
            currentPlayer.DropPiece(selectedShogiPiece);
            currentPlayer.AddPieceInPlay(selectedShogiPiece);
            PlacePiece(selectedShogiPiece, x, y);
        }
        selectedShogiPiece = null;
    }
}
