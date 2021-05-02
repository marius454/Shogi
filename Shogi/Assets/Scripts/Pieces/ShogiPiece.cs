using System;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class ShogiPiece : MonoBehaviour
{
    public int CurrentX {set;get;}
    public int CurrentY {set;get;}
    public PlayerNumber player{set;get;}
    public BoardManager board{set;get;}
    public bool[,] moves{set;get;}

    public ShogiPiece(int x, int y, PlayerNumber player, BoardManager board){
        Init(x, y, player, board);
    }
    public void Init(int x, int y, PlayerNumber player, BoardManager board){
        SetPosition(x, y);
        this.player = player;
        this.board = board;
        moves = new bool[C.numberRows, C.numberRows];
    }

    public void SetPosition (int x, int y){
        CurrentX = x;
        CurrentY = y;
    }

    public virtual bool[,] PossibleMoves(bool checkForSelfCheck = true){
        return new bool[C.numberRows, C.numberRows];
    }

    public void SingleMove(bool[,] moves, int x, int y){
        ShogiPiece potentialTile;
        
        if (x >= 0 && y >= 0 && x < C.numberRows && y < C.numberRows){
            potentialTile = BoardManager.Instance.ShogiPieces[x, y];
            if (potentialTile == null || potentialTile.player != player){
                moves[x, y] = true;
            }
        }
    }
    public void OrthagonalLine(bool[,] moves, DirectionOrthagonal direction){
        int x = CurrentX;
        int y = CurrentY;
        ShogiPiece potentialTile;
        while (true){
            if (direction == DirectionOrthagonal.right) x++;
            else if (direction == DirectionOrthagonal.forward) y++;
            else if (direction == DirectionOrthagonal.left) x--;
            else if (direction == DirectionOrthagonal.back) y--;
            else throw new InvalidOperationException("Invalid direction given for OrthagoanlLine() method");

            if (x < 0 || y < 0 || x >= C.numberRows || y >= C.numberRows)
                break;

            potentialTile = BoardManager.Instance.ShogiPieces[x, y];
            if (potentialTile == null){
                moves[x, y] = true;
            } else{
                if (potentialTile.player != player)
                    moves[x, y] = true;
                break;
            }
        }
    }
    public void DiagonalLine(bool[,] moves, DirectionDiagonal direction){
        int x = CurrentX;
        int y = CurrentY;
        ShogiPiece potentialTile;
        while (true){
            if (direction == DirectionDiagonal.forwardLeft) {x--; y++;}
            else if (direction == DirectionDiagonal.forwardRight) {x++; y++;}
            else if (direction == DirectionDiagonal.backLeft) {x--; y--;}
            else if (direction == DirectionDiagonal.backRight) {x++; y--;}
            else throw new InvalidOperationException("Invalid direction given for DiagonalLine() method");

            if (x < 0 || y < 0 || x >= C.numberRows || y >= C.numberRows)
                break;

            potentialTile = BoardManager.Instance.ShogiPieces[x, y];
            if (potentialTile == null){
                moves[x, y] = true;
            } else{
                if (potentialTile.player != player)
                    moves[x, y] = true;
                break;
            }
        }
    }
    public List<ShogiPiece> GetAttackedPieces(){
        List<ShogiPiece> attackedPieces = new List<ShogiPiece>();
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (moves[x,y] && board.ShogiPieces[x,y]){
                    attackedPieces.Add(board.ShogiPieces[x,y]);
                }
            }
        return attackedPieces;
    }

    // check if any of the possible moves are illegal, and remove them
    public void removeIllegalMoves(bool[,] moves, bool checkForSelfCheck){
        
        // check if after this move the players' King will be under attack
        if (checkForSelfCheck){
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        if(CheckIfMoveWillCauseSelfCheck(x, y)) moves[x, y] = false;
                    }
                }
        }
        // check if after this move a perpetual check will be called

    }
    private bool CheckIfMoveWillCauseSelfCheck(int x, int y){
        bool wouldCauseCheck;
        
        ShogiPiece[,] tempShogiPieces = board.ShogiPieces.Clone() as ShogiPiece[,];
        board.ShogiPieces[this.CurrentX, this.CurrentY] = null;
        board.ShogiPieces[x, y] = this;

        Player opponentPlayer;
        if (player == board.player1.playerNumber) opponentPlayer = board.player2;
        else opponentPlayer = board.player1;

        opponentPlayer.CalculatePossibleMoves(false);
        wouldCauseCheck = opponentPlayer.isAttackingKing ? true : false;
        board.ShogiPieces = tempShogiPieces.Clone() as ShogiPiece[,];
        opponentPlayer.CalculatePossibleMoves(false);

        return wouldCauseCheck;
    }
}
