using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class Player
{
    public PlayerNumber playerNumber{set;get;}
    private BoardManager board{set;get;}
    private bool[,] attackedTiles{set;get;}
    private List<ShogiPiece> piecesInPlay{set;get;}
    public bool isInCheck{set; get;}
    public bool isAttackingKing{set; get;}

    private List<ShogiPiece> capturedPieces{set;get;}
    public Player (PlayerNumber playerNumber, BoardManager board){
        piecesInPlay = new List<ShogiPiece>();
        this.board = board;
        this.playerNumber = playerNumber;
    }
    public void CalculateAttackedTiles(){
        foreach(ShogiPiece piece in piecesInPlay){
            bool[,] moves = piece.PossibleMoves(); 
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y] == true){
                        attackedTiles[x,y] = true;
                    }
                }
        }
        CheckIfAttackingKing();
    }
    private void CheckIfAttackingKing(){
        foreach (ShogiPiece piece in board.opponentPlayer.piecesInPlay){
            if(piece.GetType() == typeof(King)){
                Debug.Log(piece.CurrentX + " " + piece.CurrentY);
            }
        }
    }
    public void InitializePiecesInPlay(){
        foreach (ShogiPiece piece in board.ShogiPieces){
            if (piece.player == playerNumber){
                piecesInPlay.Add(piece);
            }
        }
    }
    public void AddPieceInPlay(ShogiPiece piece)
	{
		if (!piecesInPlay.Contains(piece))
			piecesInPlay.Add(piece);
	}

	public void RemovePieceInPlay(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece))
			piecesInPlay.Remove(piece);
	}
    public void AddCapturedPiece(ShogiPiece piece)
	{
		if (!piecesInPlay.Contains(piece))
			piecesInPlay.Add(piece);
	}

	public void RemoveCapturedPiece(ShogiPiece piece)
	{
		if (piecesInPlay.Contains(piece))
			piecesInPlay.Remove(piece);
	}

}
