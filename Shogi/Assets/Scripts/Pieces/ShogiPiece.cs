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
    public bool[,] drops{set;get;}
    public bool isPromoted{set;get;}

    public ShogiPiece(int x, int y, PlayerNumber player, BoardManager board){
        Init(x, y, player, board);
    }
    public void Init(int x, int y, PlayerNumber player, BoardManager board){
        this.player = player;
        this.board = board;
        this.isPromoted = false;
        SetXY(x, y);
        SetHeight();
        SetNormalRotation();
        moves = new bool[C.numberRows, C.numberRows];
    }

    public void SetXY (int x, int y){
        CurrentX = x;
        CurrentY = y;
    }
    public void SetHeight(){
        if (isPromoted){
            SetPromotedHeight();
        }
        else {
            SetNormalHeight();
        }
    }
    protected abstract void SetNormalHeight();
    protected virtual void SetPromotedHeight(){}
    public void SetRotation(){
        if (isPromoted){
            SetPromotedRotation();
        }
        else {
            SetNormalRotation();
        }
    }
    protected virtual void SetNormalRotation(){
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        }
    }
    protected virtual void SetPromotedRotation(){
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(95.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(95.0f, 180.0f, 0.0f);
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
        drops = new bool[C.numberRows, C.numberRows];
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (board.ShogiPieces[x, y] == null){
                    drops[x, y] = true;
                }
            }
        RemoveIllegalDrops(checkForSelfCheck);
        return drops;
    }
    public virtual void RemoveIllegalDrops(bool checkForSelfCheck){
        // if player is in check only allow drops that would stop the check
        if(board.currentPlayer.isInCheck){
            if (checkForSelfCheck){
                for (int x=0; x < C.numberRows; x++)
                    for (int y=0; y < C.numberRows; y++){
                        if (drops[x,y]){
                            if (CheckIfDropWillCauseSelfCheck(x,y)){
                                drops[x,y] = false;
                            }
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
    public void RemoveDropsLastRows(bool removeNextToLast = false){
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
            drops[x, yLast] = false;
            if (removeNextToLast) drops[x, yNextToLast] = false;
        }
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
    public void GoldMove(){
        // Select all moves in a 3x3 square around the piece, except it's current position and the tile it cannot go
        moves = new bool[C.numberRows,C.numberRows];
        int a = 1;
        if (player == PlayerNumber.Player1){
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == -1) && !(t == -1 && s == -1))
                        SingleMove(moves, CurrentX + t, CurrentY + s);
                }
        }
        else if (player == PlayerNumber.Player2) {
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == 1) && !(t == -1 && s == 1))
                        SingleMove(moves, CurrentX + t, CurrentY + s);
                }
        }
        else {
            throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
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
        ShogiPiece tempCapture = null;

        Player opponentPlayer;
        if (player == PlayerNumber.Player1) opponentPlayer = board.player2;
        else opponentPlayer = board.player1;
        
        ShogiPiece[,] tempShogiPieces = board.ShogiPieces.Clone() as ShogiPiece[,];
        if (this.CurrentX >= 0 && this.CurrentY >= 0 && this.CurrentX <= C.numberRows && this.CurrentY <= C.numberRows)
            board.ShogiPieces[this.CurrentX, this.CurrentY] = null;
        if (board.ShogiPieces[x, y]){
            tempCapture = board.ShogiPieces[x, y];
            opponentPlayer.RemovePieceInPlay(board.ShogiPieces[x, y]);
        }
        board.ShogiPieces[x, y] = this;

        opponentPlayer.CalculateAttackedTiles(false, false);
        wouldCauseCheck = opponentPlayer.isAttackingKing;
        board.ShogiPieces = tempShogiPieces.Clone() as ShogiPiece[,];

        if (tempCapture){
            board.ShogiPieces[x, y] = tempCapture;
            opponentPlayer.AddPieceInPlay(tempCapture);
        }
        opponentPlayer.CalculateAttackedTiles(false, false);


        return wouldCauseCheck;
    }
    private bool CheckIfDropWillCauseSelfCheck(int x, int y){
        bool wouldCauseCheckMate;
        board.ShogiPieces[x,y] = this;

        board.currentPlayer.AddPieceInPlay(this);
        board.opponentPlayer.CalculateAttackedTiles(false, false);

        wouldCauseCheckMate = board.opponentPlayer.isAttackingKing;
        board.currentPlayer.RemovePieceInPlay(this);
        board.ShogiPieces[x, y] = null;

        board.opponentPlayer.CalculateAttackedTiles(false, false);
        return wouldCauseCheckMate;
    }
    public virtual void CheckForPromotion(){
        if (player == PlayerNumber.Player1){
            if (board.selectedShogiPiece.CurrentY >= C.numberRows - 1)
                board.PromotePiece(this);
            else GameUI.Instance.ShowPromotionMenu(this);
        }
        else{
            if (board.selectedShogiPiece.CurrentY == 0)
                board.PromotePiece(this);
            else GameUI.Instance.ShowPromotionMenu(this);
        }
    }
    public virtual void Promote(){
        isPromoted = true;
        SetHeight();
        SetRotation();
    }
    public virtual void Unpromote(){
        isPromoted = false;
        SetHeight();
        SetRotation();
    }
}
