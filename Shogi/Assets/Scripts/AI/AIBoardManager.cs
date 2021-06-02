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
    public ShogiPlayer AIPlayer { set; get; }
    public ShogiPlayer localPlayer { set; get; }
    private bool shouldEndTurn;
    public bool gameOver { set; get; }
    private bool AIMoveStarted;
    public Thread mainThread { set; get; }
    public Thread AiThread { set; get; }
    public TaskCompletionSource<Move> bestMove { set; get; }
    public int difficulty { set; get; }

    private void Awake(){
        checkForPerpetualCheck = true;
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
        AIPlayer = player2;
        localPlayer = player1;
        shouldEndTurn = true;
        checkForPerpetualCheck = true;
        currentPlayer.CalculatePossibleMoves();
        InitializeAIThread();
    }
    public void SetDifficulty(int difficulty){
        this.difficulty = difficulty;
    }

    private void Update(){
        if (GameController.Instance.gameStarted && this.gameObject.activeSelf && currentPlayer != AIPlayer){
            UpdateSelection();
            MarkSelectedPiece();
            MarkAllowedMoves();
            MarkCheckedKing();
            ComputeMouseClick();
        }
        if (!gameOver){
            if (currentPlayer == AIPlayer && !AIMoveStarted){
                if (AiThread.ThreadState != ThreadState.Aborted && AiThread.ThreadState != ThreadState.Unstarted){
                    Move move = bestMove.Task.Result;
                    AiThread.Abort();
                    if (difficulty > 0)
                        if (BoardEvaluation.boardStates.Count > 0){
                            RestoreGameState(BoardEvaluation.boardStates[0][0], true);
                        }
                    
                    FinishAIMove(move);
                    InitializeAIThread();
                }
            }
        }
    }
    protected override void SelectShogiPiece(int x, int y, bool isSimulated = false){
        if (!ShogiPieces[x, y])
            return;
        if (ShogiPieces[x, y].player != currentPlayer.playerNumber && !isSimulated)
            return;
        base.SelectShogiPiece(x, y);
    }
    private void AIMove(){
        // Move bestMove;
        // ChangePlayer();
        // shouldEndTurn = false;
        // checkForPerpetualCheck = false;
        // if (difficulty == 0){
        //     bestMove = RandomMove(AIPlayer);
        // }
        // else {
        //     bestMove = BoardEvaluation.GetAIMove(difficulty, this);
        // }
        // checkForPerpetualCheck = true;
        // shouldEndTurn = true;

        // DoMove(bestMove);

        // EndTurn();
        // ChangePlayer();
        
        if (!AIMoveStarted){
            AIMoveStarted = true;
            AiThread.Start();
        }
    }
    private Move OnAIMove(){
        Move bestMove;
        ChangePlayer();
        shouldEndTurn = false;
        checkForPerpetualCheck = false;
        if (difficulty == 0){
            bestMove = RandomMove(AIPlayer);
        }
        else {
            bestMove = BoardEvaluation.GetAIMove(difficulty, this);
        }
        return bestMove;
    }
    private void FinishAIMove(Move bestMove){
        shouldEndTurn = true;
        DoMove(bestMove);
        EndTurn();
        ChangePlayer();
    }
    public void DoMove(Move move, bool isSimulated = false){
        ShogiPlayer player;
        if (move.pieceX > C.numberRows) player = player1;
        else if (move.pieceX < 0) player = player2;
        else if (ShogiPieces[move.pieceX, move.pieceY].player == PlayerNumber.Player1){
            player = player1;
        } 
        else player = player2;

        if (move.pieceX >= 0 && move.pieceY >= 0 && move.pieceX <= C.numberRows && move.pieceY <= C.numberRows){
            SelectShogiPiece(move.pieceX, move.pieceY, isSimulated);
            allowedMoves[move.targetX, move.targetY] = true;
            MoveShogiPiece(move.targetX, move.targetY, isSimulated);

            if (move.promote){
                PromotePiece(ShogiPieces[move.targetX, move.targetY], isSimulated);
            }
        }
        else if (move.pieceX >= player.captureBoard.minX && move.pieceY >= player.captureBoard.minY 
          && move.pieceX <= player.captureBoard.maxX && move.pieceY <= player.captureBoard.maxY){
              SelectCapturedPiece(move.pieceX, move.pieceY, player);
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
        if (changePositions)
            player.CalculatePossibleMoves();
    }

    public Move RandomMove(ShogiPlayer player){
        if (player.possibleMoves.Count <= 0){
            return new Move{pieceX = -1, pieceY = -1, targetX = -1, targetY = -1, promote = false};
        }
        int randomMove = random.Next(player.possibleMoves.Count - 1);
        int pieceX = player.possibleMoves[randomMove].pieceX;
        int pieceY = player.possibleMoves[randomMove].pieceY;
        int targetX = player.possibleMoves[randomMove].targetX;
        int targetY = player.possibleMoves[randomMove].targetY;
        bool promote = player.possibleMoves[randomMove].promote;

        return new Move{pieceX = pieceX, pieceY = pieceY, targetX = targetX, targetY = targetY, promote = promote};
    }
    private void SelectCapturedPiece(int x, int y, ShogiPlayer player){
        (x, y) = player.captureBoard.CoordinatesToIndeces(x, y);
        selectedShogiPiece = player.captureBoard.capturedPieces[x, y];
        allowedMoves = selectedShogiPiece.PossibleDrops();
    }
    protected override void OnTurnEnd()
    {
        selectedShogiPiece = null;
        if (shouldEndTurn){
            if (CheckIfGameOver()){
                EndGame();
                gameOver = true;
            }
            else if (CheckForRepetitionDraw()){
                EndGame("Repetition Draw");
            }
            if (currentPlayer != AIPlayer){
                MarkSelectedPiece();
                MarkAllowedMoves();
                MarkCheckedKing();
                AIMove();
            }
        }
    }
    protected override void CheckForPromotion(bool isSimulated = false)
    {
        if (currentPlayer != AIPlayer && !isSimulated)
            base.CheckForPromotion();
        else {
            if (selectedShogiPiece.player == PlayerNumber.Player1){
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
