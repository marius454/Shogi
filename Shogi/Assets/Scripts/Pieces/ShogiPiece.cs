using System;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public abstract class ShogiPiece : MonoBehaviour
{
    public int CurrentX {set;get;}
    public int CurrentY {set;get;}
    public PlayerNumber player{set;get;}
    public BoardManager board{set;get;}
    public bool[,] moves{set;get;}
    public bool isPromoted = false;

    public ShogiPiece(int x, int y, PlayerNumber player, BoardManager board){
        Init(x, y, player, board);
    }
    public void Init(int x, int y, PlayerNumber player, BoardManager board){
        this.player = player;
        this.board = board;
        SetXY(x, y);
        SetHeight();
        SetNormalRotation();
        moves = new bool[C.numberRows, C.numberRows];
    }

    public void SetXY (int x, int y){
        CurrentX = x;
        CurrentY = y;
    }
    public abstract void SetHeight();
    public virtual void SetNormalRotation(){
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        }
    }
    public virtual void SetPromotedRotation(){
        // TO DO
        // need to change this in a way that will flip the piece correctly to it's oppositesdie
        // might need a different one for every piece
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        }
    }

    public virtual bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows, C.numberRows];
        return moves;
    }
    // check if any of the possible moves are illegal, and remove them
    public void RemoveIllegalMoves(bool[,] moves, bool checkForSelfCheck){
        
        // check if after this move the players' King will be under attack
        if (checkForSelfCheck){
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        if(CheckIfMoveWillCauseSelfCheck(x, y)) moves[x, y] = false;
                    }
                }
        }
        // check if after this move a perpetual check would be called
        // TO DO

    }
    public virtual bool[,] PossibleDrops(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows, C.numberRows];
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (board.ShogiPieces[x, y] == null){
                    moves[x, y] = true;
                }
            }

        RemoveIllegalDrops();
        return moves;
    }
    public virtual void RemoveIllegalDrops(){
        // TO DO
        // if player is in check only allow drops that would stop the check
        if(board.currentPlayer.isInCheck){
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        if (CheckIfMoveWillCauseSelfCheck(x,y)){
                            moves[x,y] = false;
                        }
                    }
                }
        }
        // In subclasses:
        // pawns and lances can't be on the last row
        // knights cant be on the last and next to last row
        // cant drop pawn on a row with an unpromoted pawn
        // cant drop a pawn in place that would cause checkmate
    }
    public void RemoveLastRow(bool removeNextToLast = false){
        int yLast;
        int yNextToLast;
        if (player == PlayerNumber.Player1){
            yLast = C.numberRows - 1;
            yNextToLast = C.numberRows - 2;
        }
        else {
            yLast = 0;
            yNextToLast = 1;
        }
        for (int x=0; x < C.numberRows; x++){
            moves[x, yLast] = false;
            if (removeNextToLast) moves[x, yNextToLast] = false;
        }
        //Debug.Log(moves[4,8] + " " + yLast);
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

    private bool CheckIfMoveWillCauseSelfCheck(int x, int y){
        bool wouldCauseCheck;
        
        ShogiPiece[,] tempShogiPieces = board.ShogiPieces.Clone() as ShogiPiece[,];
        if (this.CurrentX >= 0 && this.CurrentY >= 0 && this.CurrentX <= C.numberRows && this.CurrentY <= C.numberRows)
            board.ShogiPieces[this.CurrentX, this.CurrentY] = null;
        board.ShogiPieces[x, y] = this;

        Player opponentPlayer;
        if (player == board.player1.playerNumber) opponentPlayer = board.player2;
        else opponentPlayer = board.player1;

        opponentPlayer.CalculateAttackedTiles(false);
        wouldCauseCheck = opponentPlayer.isAttackingKing;
        board.ShogiPieces = tempShogiPieces.Clone() as ShogiPiece[,];
        opponentPlayer.CalculateAttackedTiles(false);

        return wouldCauseCheck;
    }
}
