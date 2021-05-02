using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Player
{
    public PlayerNumber playerNumber{set;get;}
    private BoardManager board{set;get;}
    public bool[,] possibleMoves{set;get;}
    private List<ShogiPiece> piecesInPlay{set;get;}
    public bool isInCheck{set; get;}
    public bool isAttackingKing{set; get;}
    // to set this check both attacked tiles and whether are legal drops available
    public bool hasPossibleMoves{set; get;}

    private List<ShogiPiece> capturedPieces{set;get;}
    public Player (PlayerNumber playerNumber, BoardManager board){
        piecesInPlay = new List<ShogiPiece>();
        capturedPieces = new List<ShogiPiece>();
        possibleMoves = new bool[C.numberRows, C.numberRows];
        hasPossibleMoves = true;
        isAttackingKing = false;
        isInCheck = false;

        this.board = board;
        this.playerNumber = playerNumber;
    }
    public void CalculatePossibleMoves(bool checkForSelfCheck = true){
        possibleMoves = new bool[C.numberRows, C.numberRows];
        int nrMoves = 0;
        //Debug.Log(checkForSelfCheck);
        foreach(ShogiPiece piece in piecesInPlay){
            bool[,] moves = piece.PossibleMoves(checkForSelfCheck); 
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        possibleMoves[x,y] = true;
                        nrMoves++;
                    }
                }
        }
        if (checkForSelfCheck) Debug.Log(playerNumber + " " + nrMoves);
        if (nrMoves == 0){
            hasPossibleMoves = false;
        }
        CheckIfAttackingKing();
    }
    public void CheckIfAttackingKing(){
        // foreach (ShogiPiece piece in piecesInPlay){
        //     foreach (ShogiPiece attackedPiece in piece.GetAttackedPieces()){
        //         // Debug.Log("Piece " + piece + " is attacking piece " + attackedPiece);
        //         // Debug.Log("Is the attacked piece a King? - " + (attackedPiece is King));
        //         if (attackedPiece is King){
        //             isAttackingKing = true;
        //             return;
        //         }
        //     }
        // }

        // Another way to do this is checking all attacked tile for a king
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (possibleMoves[x, y]){
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
		if (!piecesInPlay.Contains(piece))
			piecesInPlay.Add(piece);
        // perkelti prie pagrindeines lentos
	}

	public void RemovePieceInPlay(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece))
			piecesInPlay.Remove(piece);
	}
    public void AddCapturedPiece(ShogiPiece piece)
	{
		if (!piecesInPlay.Contains(piece))
			capturedPieces.Add(piece);
        // perkelti prie capture lentos.
	}

	public void RemoveCapturedPiece(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece))
			capturedPieces.Remove(piece);

	}

}