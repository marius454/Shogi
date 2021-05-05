using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Player
{
    public PlayerNumber playerNumber{set;get;}
    private BoardManager board{set;get;}
    public bool[,] attackedTiles{set;get;}
    public List<ShogiPiece> piecesInPlay{set;get;}
    public List<ShogiPiece> capturedPieces{set;get;}
    public CaptureBoard captureBoard{set;get;}
    public bool isInCheck{set; get;}
    public bool isAttackingKing{set; get;}
    // to set this check both attacked tiles and whether are legal drops available
    public bool hasPossibleMoves{set; get;}

    public Player (PlayerNumber playerNumber, BoardManager board, CaptureBoard captureBoard){
        piecesInPlay = new List<ShogiPiece>();
        capturedPieces = new List<ShogiPiece>();
        attackedTiles = new bool[C.numberRows, C.numberRows];
        hasPossibleMoves = true;
        isAttackingKing = false;
        isInCheck = false;
        this.captureBoard = captureBoard;
        this.board = board;
        this.playerNumber = playerNumber;
    }
    public void CalculateAttackedTiles(bool checkForSelfCheck = true){
        attackedTiles = new bool[C.numberRows, C.numberRows];
        int nrMoves = 0;
        //Debug.Log(checkForSelfCheck);
        foreach(ShogiPiece piece in piecesInPlay){
            bool[,] moves = piece.PossibleMoves(checkForSelfCheck); 
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        attackedTiles[x,y] = true;
                        nrMoves++;
                    }
                }
        }
        if (nrMoves == 0){
            hasPossibleMoves = false;
        }
        CheckIfAttackingKing();
    }
    public void CheckIfAttackingKing(){
        // Another way to do this is checking all attacked tile for a king
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
    public void InitializePiecesInPlay(){
        piecesInPlay = new List<ShogiPiece>();
        foreach (ShogiPiece piece in board.ShogiPieces){
            if (piece && piece.player == playerNumber){
                piecesInPlay.Add(piece);
            }
        }
    }
    public void AddPieceInPlay(ShogiPiece piece)
	{
		if (!piecesInPlay.Contains(piece)){}
			piecesInPlay.Add(piece);
	}

	public void RemovePieceInPlay(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece))
			piecesInPlay.Remove(piece);
	}
    public void CapturePiece(ShogiPiece piece)
	{
		if (!piecesInPlay.Contains(piece)){
            piece.player = this.playerNumber;
			capturedPieces.Add(piece);
            captureBoard.AddPiece(piece);
        }
	}

	public void DropPiece(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece)){
            capturedPieces.Remove(piece);
            captureBoard.DropCapturedPiece(piece);
        }
	}

}
