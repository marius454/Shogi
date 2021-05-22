using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using System.Threading;
using System.Threading.Tasks;

public struct Move{
    public int pieceX;
    public int pieceY;
    public int targetX;
    public int targetY;
    public bool promote;
    public float score;
}
public class AIBoardManager : BoardManager
{
    private System.Random random;
    public ShogiPlayer AIPlayer;
    public ShogiPlayer localPlayer;
    private bool shouldEndTurn;
    public bool gameOver;
    private bool AIMoveStarted;
    public Thread mainThread;
    public Thread AiThread;
    public TaskCompletionSource<Move> bestMove;
    private void Awake(){
        mainThread = Thread.CurrentThread;
        random = new System.Random();
        Instance = this;
    }
    private void InitializeAIThread(){
        bestMove = new TaskCompletionSource<Move>();
        AiThread = new Thread(() => {
            Move move = OnAIMove();
            currentPlayer.CalculatePossibleMoves();
            AIMoveStarted = false;
            while (true)
                bestMove.TrySetResult(move);
        });
    }
    public override void InitializeGame()
    {
        base.InitializeGame();
        gameOver = false;
        AIMoveStarted = false;
        AIPlayer = player2; // later will need to be changed so that the user can chose
        localPlayer = player1;
        shouldEndTurn = true;
        currentPlayer.CalculatePossibleMoves();
        InitializeAIThread();
    }

    private void Update(){
        if (!gameOver){
            if (currentPlayer == AIPlayer && !AIMoveStarted){
                if (AiThread.ThreadState != ThreadState.Aborted && AiThread.ThreadState != ThreadState.Unstarted){
                    Move move = bestMove.Task.Result;
                    Debug.Log(move);
                    Debug.Log(move.pieceX + " " + move.pieceY + " " + move.targetX + " " + move.targetY + " " + move.promote + " " + move.score);
                    AiThread.Abort();
                    if (BoardEvaluation.boardStates.Count > 0){
                        RestoreGameState(BoardEvaluation.boardStates[0][0], true);
                    }
                    
                    FinishAIMove(move);
                    InitializeAIThread();
                }
            }
            if (currentPlayer != AIPlayer && shouldEndTurn && !AIMoveStarted){
                DoMove(RandomMove(currentPlayer));
                ChangePlayer();
            }
            else AIMove();
        }
    }
    protected override void SelectShogiPiece(int x, int y, bool isSimulated = false){
        if (ShogiPieces[x, y] == null)
            return;
        if (ShogiPieces[x, y].player != currentPlayer.playerNumber && !isSimulated)
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
        bestMove = BoardEvaluation.GetAIMove(1, true, this, AIPlayer);
        shouldEndTurn = true;

        DoMove(bestMove);
        Debug.Break();

        EndTurn();
        ChangePlayer();
        
        // if (!AIMoveStarted){
        //     AIMoveStarted = true;
        //     AiThread.Start();
        // }
    }
    private Move OnAIMove(){
        Move bestMove;
        //ChangePlayer();
        shouldEndTurn = false;
        bestMove = BoardEvaluation.GetAIMove(1, true, this, AIPlayer);
        return bestMove;
    }
    private void FinishAIMove(Move bestMove){
        shouldEndTurn = true;
        DoMove(bestMove);
        EndTurn();
        ChangePlayer();
    }
    public void DoMove(Move move, bool isSimulated = false){
        if (move.pieceX >= 0 && move.pieceY >= 0 && move.pieceX <= C.numberRows && move.pieceY <= C.numberRows){
            SelectShogiPiece(move.pieceX, move.pieceY);
            allowedMoves[move.targetX, move.targetY] = true;
            MoveShogiPiece(move.targetX, move.targetY, isSimulated);

            if (move.promote){
                if (ShogiPieces[move.targetX, move.targetY] == null){
                    bool[,] moves = ShogiPieces[move.pieceX, move.pieceY].PossibleMoves();
                    Debug.Log(moves);
                    Debug.Log("Null after move");
                    Debug.Break();
                }
                PromotePiece(ShogiPieces[move.targetX, move.targetY], isSimulated);
            }
        }
        else if (move.pieceX >= currentPlayer.captureBoard.minX && move.pieceY >= currentPlayer.captureBoard.minY 
          && move.pieceX <= currentPlayer.captureBoard.maxX && move.pieceY <= currentPlayer.captureBoard.maxY){
              SelectCapturedPiece(move.pieceX, move.pieceY);
              DropShogiPiece(move.targetX, move.targetY, isSimulated);
        }
    }

    public void RestoreGameState(BoardState boardState, bool changePositions = true){
        selectedShogiPiece = null;
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                var piece = boardState.shogiPieceState[x, y];
                if (piece.pieceIndex >= 0){
                    ShogiPieces[x, y] = allPieces[piece.pieceIndex];
                    ShogiPieces[x, y].Init(x, y, piece.playerNumber, piece.pieceType, this, changePositions);
                    if (piece.promoted) ShogiPieces[x, y].Promote(changePositions);
                }
                else if (ShogiPieces[x, y]) ShogiPieces[x, y] = null;
            }

        RestorePlayer (player1, boardState.captureBoardPlayer1State, changePositions);
        RestorePlayer (player2, boardState.captureBoardPlayer2State, changePositions);
    }
    private void RestorePlayer(ShogiPlayer player, (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)[,] captureBoard, bool changePositions = true){
        player.InitializePiecesInPlay();
        
        for (int x = 0; x < C.captureNumberColumns; x++)
            for (int y = 0; y < C.captureNumberRows; y++){
                var piece = captureBoard[x, y];
                if (piece.pieceIndex >= 0){
                    player.captureBoard.capturedPieces[x, y] = allPieces[piece.pieceIndex];
                    player.captureBoard.capturedPieces[x, y].Init(piece.x, piece.y, piece.playerNumber, piece.pieceType, this, changePositions);
                    player.captureBoard.capturedPieces[x, y].Unpromote(changePositions);
                }
                else player.captureBoard.capturedPieces[piece.x, piece.y] = null;
            }
        
        player.InitializeCapturedPieces();
        player.CalculatePossibleMoves();
    }
    // protected override void CapturePieceIfPossible(int x, int y, bool isSimulated = false)
    // {
    //     base.CapturePieceIfPossible(x, y, isSimulated);
    // }

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
    protected override void CheckForPromotion(bool isSimulated = false)
    {
        if (currentPlayer != AIPlayer)
            base.CheckForPromotion();
        else {
            if (AIPlayer == player1){
                if ((selectedShogiPiece is Pawn) || (selectedShogiPiece is Lance)){
                    if (selectedShogiPiece.CurrentY >= C.numberRows - 1){
                        PromotePiece(selectedShogiPiece, isSimulated);
                    }
                }
                else if (selectedShogiPiece is Knight){
                    if (selectedShogiPiece.CurrentY >= C.numberRows - 2){
                        PromotePiece(selectedShogiPiece, isSimulated);
                    }
                }
            }
            else {
               if ((selectedShogiPiece is Pawn) || (selectedShogiPiece is Lance)){
                    if (selectedShogiPiece.CurrentY <= 0){
                        PromotePiece(selectedShogiPiece, isSimulated);
                    }
                }
                else if (selectedShogiPiece is Knight){
                    if (selectedShogiPiece.CurrentY <= 1){
                        PromotePiece(selectedShogiPiece, isSimulated);
                    }
                } 
            }
        }
    }
    protected override void OnShogiPieceDrop(int x, int y, bool isSimulated = false){
        if (allowedMoves[x,y]){
            currentPlayer.DropPiece(selectedShogiPiece);
            currentPlayer.AddPieceInPlay(selectedShogiPiece);
            PlacePiece(selectedShogiPiece, x, y, isSimulated);
        }
        selectedShogiPiece = null;
    }
    public override void OnPiecePromote(int x, int y, bool isSimulated = false){
        ShogiPieces[x, y].Promote(!isSimulated);

        if (shouldEndTurn){
            currentPlayer.CalculatePossibleMoves();
            opponentPlayer.CalculatePossibleMoves();
            if (CheckIfGameOver()){
                EndGame();
                gameOver = true;
            }
        } 
    }
    public override void ResetGame(){
        gameOver = true;
        Thread.Sleep(500);
        DestroyAllPieces();
        InitializeGame();
    }
}
