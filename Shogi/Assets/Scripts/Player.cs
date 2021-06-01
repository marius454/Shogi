using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class ShogiPlayer
{
    public PlayerNumber playerNumber { set; get; }
    private BoardManager board;
    public bool[,] attackedTiles { set; get; }
    public List<ShogiPiece> piecesInPlay { set; get; }
    public List<ShogiPiece> capturedPieces { set; get; }
    public List<ShogiPiece> allPieces { set; get; }
    public King king { set; get; }
    public CaptureBoard captureBoard { set; get; }
    public bool isInCheck { set; get; }
    public bool isAttackingKing { set; get; }
    public int nrOfChecksInARow { set; get; }
    // to set this check both attacked tiles and whether are legal drops available
    public bool hasPossibleMoves { set; get; }
    public List<Move> possibleMoves { set; get; }
    public GameObject playerCamera { set; get; }

    public ShogiPlayer (PlayerNumber playerNumber, BoardManager board, CaptureBoard captureBoard){
        piecesInPlay = new List<ShogiPiece>();
        capturedPieces = new List<ShogiPiece>();
        allPieces = new List<ShogiPiece>();
        attackedTiles = new bool[C.numberRows, C.numberRows];
        hasPossibleMoves = true;
        isAttackingKing = false;
        isInCheck = false;
        nrOfChecksInARow = 0;
        this.captureBoard = captureBoard;
        this.board = board;
        this.playerNumber = playerNumber;
    }
    public void CalculatePossibleMoves(bool checkForSelfCheck = true, bool checkDrops = true){
        // attackedTiles = new bool[C.numberRows, C.numberRows];
        Array.Clear(attackedTiles, 0, C.numberRows*C.numberRows);
        possibleMoves = new List<Move>();
        int nrMoves = 0;
        foreach(ShogiPiece piece in piecesInPlay){
            bool[,] moves = piece.PossibleMoves(checkForSelfCheck); 
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        attackedTiles[x,y] = true;
                        possibleMoves.Add(new Move{pieceX = piece.CurrentX, pieceY = piece.CurrentY, 
                                targetX = x, targetY = y, promote = false});
                        if (piece.CheckIfCouldBePromoted(y))
                            possibleMoves.Add(new Move{pieceX = piece.CurrentX, pieceY = piece.CurrentY, 
                                targetX = x, targetY = y, promote = true});
                        nrMoves++;
                    }
                }
        }
        if (checkDrops){
            foreach(ShogiPiece piece in capturedPieces){
            bool[,] drops = piece.PossibleDrops(checkForSelfCheck); 
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (drops[x,y]){
                        possibleMoves.Add(new Move{pieceX = piece.CurrentX, pieceY = piece.CurrentY, 
                                            targetX = x, targetY = y, promote = false});
                        nrMoves++;
                    }
                }
            }
        }
        if (nrMoves == 0){
            hasPossibleMoves = false;
        }
        else hasPossibleMoves = true;
        CheckIfAttackingKing();
    }
    public void CheckIfAttackingKing(){
        // Check all attacked tiles for opponent King
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (attackedTiles[x, y]){
                    if (board.ShogiPieces[x, y]){
                        if (board.ShogiPieces[x, y] is King){
                            isAttackingKing = true;
                            return;
                        }
                    }
                }
            }
        isAttackingKing = false;
    }
    public void RebuildAfterPossibleMoveCalculation(bool hasPossibleMoves, bool[,] attackedTiles, bool isAttackingKing){
        this.hasPossibleMoves = hasPossibleMoves;
        this.attackedTiles = attackedTiles.Clone() as bool[,];
        this.isAttackingKing = isAttackingKing;
    }
    public void CheckIfKingIsBeingAttacked(){
        if (king){
            if (king.IsAttacked()) PlaceInCheck();
            else RemoveCheck();
        }
    }
    public void InitializePiecesInPlay(){
        allPieces.Clear(); // Needs to be done better so that InitializeCapturedPieces also gets this when needed
        piecesInPlay.Clear();
        foreach (ShogiPiece piece in board.ShogiPieces){
            if (piece && piece.player == playerNumber){
                piecesInPlay.Add(piece);
                allPieces.Add(piece);
                if (piece is King){
                    king = piece as King;
                }
            }
        }
    }
    public void InitializeCapturedPieces(){
        capturedPieces.Clear();
        foreach (ShogiPiece piece in captureBoard.capturedPieces){
            if (piece && piece.player == playerNumber){
                capturedPieces.Add(piece);
                allPieces.Add(piece);
            }
        }
    }
    public void AddPieceInPlay(ShogiPiece piece)
	{
		if (!piecesInPlay.Contains(piece)){
            piecesInPlay.Add(piece);
            allPieces.Add(piece);
        }
	}

	public void RemovePieceInPlay(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece)){
            piecesInPlay.Remove(piece);
            allPieces.Remove(piece);
        }
	}
    public void CapturePiece(ShogiPiece piece, bool isSimulated = false)
	{
		if (!capturedPieces.Contains(piece)){
            piece.player = this.playerNumber;
            piece.SetOpponents();
			capturedPieces.Add(piece);
            allPieces.Add(piece);
            captureBoard.AddPiece(piece, isSimulated);
        }
	}

	public void DropPiece(ShogiPiece piece)
	{
		if (capturedPieces.Contains(piece)){
            capturedPieces.Remove(piece);
            allPieces.Remove(piece);
            captureBoard.DropCapturedPiece(piece);
        }
	}
    public void DestroyAllPieces(){
        captureBoard.DestroyAllPieces();
        piecesInPlay.Clear();
        capturedPieces.Clear();
    }
    public void PlaceInCheck(){
        if (!isInCheck){
            isInCheck = true;
        }
        // BoardHighlights.Instance.HighlightCheck(king.CurrentX, king.CurrentY);
    }
    public void RemoveCheck(){
        if (isInCheck){
            isInCheck = false;
            // BoardHighlights.Instance.HideCheckHighlight();
        }
    }
    #region Multiplayer
    public void SetupCamera(GameObject camera){
        playerCamera = camera;
        playerCamera.SetActive(true);
    }
    #endregion
}
