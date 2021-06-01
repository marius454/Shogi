using System;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public abstract class ShogiPiece : MonoBehaviour
{
    public int id { set; get; }
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }
    public PlayerNumber player { set; get; }
    public PieceType pieceType { set; get; }
    public BoardManager board { set; get; }
    protected ShogiPlayer currentPlayer;
    protected ShogiPlayer opponentPlayer;
    public bool[,] moves { set; get; }
    public bool[,] drops { set; get; }
    public bool isPromoted { set; get; }
    public bool isAttacked { set; get; }

    public ShogiPiece(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board){
        Init(x, y, player, pieceType, board);
    }
    public virtual void Init(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board, bool changePosition = true){
        this.player = player;
        this.pieceType = pieceType;
        this.board = board;
        this.isPromoted = false;
        this.moves = new bool[C.numberRows, C.numberRows];
        this.drops = new bool[C.numberRows, C.numberRows];
        this.isAttacked = false;

        SetXY(x, y, changePosition);
        // SetHeight();
        if (changePosition) SetNormalRotation();
        SetOpponents();
    }
    public void SetID(int id){
        this.id = id;
    }
    public void SetOpponents()
    {
        if (player == PlayerNumber.Player1){
            currentPlayer = board.player1;
            opponentPlayer = board.player2;
        }
        else{
            currentPlayer = board.player2;
            opponentPlayer = board.player1;
        }
    }
    public void SetXY (int x, int y, bool changePosition = true){
        CurrentX = x;
        CurrentY = y;
        if (changePosition){
            this.gameObject.transform.position = board.GetTileCenter(x, y);
            SetHeight();
        }
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
    public void ChangeSides(){
        if (player == PlayerNumber.Player1){
            player = PlayerNumber.Player2;
        }
        else player = PlayerNumber.Player1;
        SetOpponents();
    }
    public virtual bool[,] PossibleMoves(bool checkForSelfCheck = true){
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        return moves;
    }
    // check if any of the possible moves are illegal, and remove them
    public bool[,] RemoveIllegalMoves(bool[,] moves, bool checkForSelfCheck = true){
        if (checkForSelfCheck){
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        // check if after this move the players' King will be under attack
                        if(CheckIfMoveWillCauseSelfCheck(x, y)) moves[x, y] = false;
                        
                        // check if after this move a perpetual check would be called
                        if (board.checkForPerpetualCheck)
                            if(CheckIfMoveWillCausePerpetualCheck(x, y)) moves[x, y] = false;
                    }
                }
        }
        return moves;
    }
    public virtual bool[,] PossibleDrops(bool checkForSelfCheck = true){
        Array.Clear(drops, 0, C.numberRows*C.numberRows);
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (!board.ShogiPieces[x, y]){
                    drops[x, y] = true;
                }
            }
        RemoveIllegalDrops(checkForSelfCheck);
        return drops;
    }
    public virtual void RemoveIllegalDrops(bool checkForSelfCheck){
        // if player is in check only allow drops that would stop the check
        if(opponentPlayer.isAttackingKing){
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
    // public void SingleMove(bool[,] moves, int x, int y){
    //     ShogiPiece potentialTile;
        
    //     if (x >= 0 && y >= 0 && x < C.numberRows && y < C.numberRows){
    //         potentialTile = board.ShogiPieces[x, y];
    //         if ((object)potentialTile == null || potentialTile.player != player){
    //             moves[x, y] = true;
    //         }
    //     }
    // }
    public void SingleMove(bool[,] moves, int x, int y, PlayerNumber currentPlayer, BoardManager board){
        ShogiPiece potentialTile;
        
        if (x >= 0 && y >= 0 && x < C.numberRows && y < C.numberRows){
            potentialTile = board.ShogiPieces[x, y];
            if ((object)potentialTile == null || potentialTile.player != currentPlayer){
                moves[x, y] = true;
            }
        }
    }
    public void OrthagonalLine(bool[,] moves, DirectionOrthagonal direction, PlayerNumber currentPlayer, BoardManager board, int x, int y, bool checkForAttacker = false){
        // int x = CurrentX;
        // int y = CurrentY;
        ShogiPiece potentialTile;
        while (true){
            if (direction == DirectionOrthagonal.right) x++;
            else if (direction == DirectionOrthagonal.forward) y++;
            else if (direction == DirectionOrthagonal.left) x--;
            else if (direction == DirectionOrthagonal.back) y--;
            else throw new InvalidOperationException("Invalid direction given for OrthagoanlLine() method");

            if (x < 0 || y < 0 || x >= C.numberRows || y >= C.numberRows)
                break;

            potentialTile = board.ShogiPieces[x, y];
            if ((object)potentialTile == null){
                moves[x, y] = true;
            } else{
                if (potentialTile.player != currentPlayer){
                    moves[x, y] = true;
                    if (checkForAttacker){
                        if (potentialTile.GetType() == typeof(Rook) 
                         || (player == PlayerNumber.Player1 && direction == DirectionOrthagonal.forward && potentialTile.GetType() == typeof(Lance) && !potentialTile.isPromoted)
                         || (player == PlayerNumber.Player2 && direction == DirectionOrthagonal.back && potentialTile.GetType() == typeof(Lance) && !potentialTile.isPromoted)){
                            this.isAttacked = true;
                            return;
                        }
                    }
                }
                break;
            }
        }
    }
    public void DiagonalLine(bool[,] moves, DirectionDiagonal direction, PlayerNumber currentPlayer, BoardManager board, int x, int y, bool checkForAttacker = false){
        // int x = CurrentX;
        // int y = CurrentY;
        ShogiPiece potentialTile;
        while (true){
            if (direction == DirectionDiagonal.forwardLeft) {x--; y++;}
            else if (direction == DirectionDiagonal.forwardRight) {x++; y++;}
            else if (direction == DirectionDiagonal.backLeft) {x--; y--;}
            else if (direction == DirectionDiagonal.backRight) {x++; y--;}
            else throw new InvalidOperationException("Invalid direction given for DiagonalLine() method");

            if (x < 0 || y < 0 || x >= C.numberRows || y >= C.numberRows)
                break;

            potentialTile = board.ShogiPieces[x, y];
            if ((object)potentialTile == null){
                moves[x, y] = true;
            } else{
                if (potentialTile.player != currentPlayer){
                    moves[x, y] = true;
                    if (checkForAttacker){
                        if (potentialTile.GetType() == typeof(Bishop)){
                            this.isAttacked = true;
                            return;
                        } 
                    }
                }
                break;
            }
        }
    }
    public void GoldMove(){
        // Select all moves in a 3x3 square around the piece, except it's current position and the tile it cannot go
        Array.Clear(moves, 0, C.numberRows*C.numberRows);
        int x = CurrentX;
        int y = CurrentY;
        PlayerNumber currentPlayer = player;
        BoardManager localBoard = board;

        int a = 1;
        if (player == PlayerNumber.Player1){
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == -1) && !(t == -1 && s == -1))
                        SingleMove(moves, x + t, y + s, currentPlayer, localBoard);
                }
        }
        else if (player == PlayerNumber.Player2) {
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == 1) && !(t == -1 && s == 1))
                        SingleMove(moves, x + t, y + s, currentPlayer, localBoard);
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
        bool wouldCauseSelfCheck = false;
        ShogiPiece tempCapture = null;
        
        // ShogiPiece[,] tempShogiPieces = board.ShogiPieces.Clone() as ShogiPiece[,];
        board.ShogiPieces[CurrentX, CurrentY] = null;
        if (board.ShogiPieces[x, y]){
            tempCapture = board.ShogiPieces[x, y];
        }
        board.ShogiPieces[x, y] = this;
        int tempX = CurrentX;
        int tempY = CurrentY;
        this.SetXY(x, y, false);

        bool tempKingAttackedStatus = currentPlayer.isInCheck;
        currentPlayer.CheckIfKingIsBeingAttacked();
        wouldCauseSelfCheck = currentPlayer.isInCheck;
        // board.ShogiPieces = tempShogiPieces.Clone() as ShogiPiece[,];

        this.SetXY(tempX, tempY, false);
        board.ShogiPieces[tempX, tempY] = this;
        if (tempCapture){
            board.ShogiPieces[x, y] = tempCapture;
        }
        else board.ShogiPieces[x, y] = null;
        
        if (tempKingAttackedStatus) currentPlayer.PlaceInCheck();
        else currentPlayer.RemoveCheck();

        return wouldCauseSelfCheck;
    }
    private bool CheckIfMoveWillCausePerpetualCheck(int x, int y){
        bool wouldCauseSelfCheck = false;
        ShogiPiece tempCapture = null;
        
        // ShogiPiece[,] tempShogiPieces = board.ShogiPieces.Clone() as ShogiPiece[,];
        board.ShogiPieces[CurrentX, CurrentY] = null;
        if (board.ShogiPieces[x, y]){
            tempCapture = board.ShogiPieces[x, y];
        }
        board.ShogiPieces[x, y] = this;
        int tempX = CurrentX;
        int tempY = CurrentY;
        this.SetXY(x, y, false);

        bool tempKingAttackedStatus = opponentPlayer.isInCheck;
        opponentPlayer.CheckIfKingIsBeingAttacked();
        if (currentPlayer.nrOfChecksInARow >= 3){
            // Debug.Log(currentPlayer.nrOfChecksInARow + " " + currentPlayer.isAttackingKing + " " + currentPlayer.playerNumber + " " + x + " " + y);
            if(opponentPlayer.isInCheck)
                wouldCauseSelfCheck = true;
        }

        // board.ShogiPieces = tempShogiPieces.Clone() as ShogiPiece[,];
        this.SetXY(tempX, tempY, false);
        board.ShogiPieces[tempX, tempY] = this;
        if (tempCapture){
            board.ShogiPieces[x, y] = tempCapture;
        }
        else board.ShogiPieces[x, y] = null;
        if (tempKingAttackedStatus) opponentPlayer.PlaceInCheck();
        else opponentPlayer.RemoveCheck();

        return wouldCauseSelfCheck;
    }
    private bool CheckIfDropWillCauseSelfCheck(int x, int y){
        bool wouldCauseSelfCheck = false;
        board.ShogiPieces[x,y] = this;

        currentPlayer.CheckIfKingIsBeingAttacked();
        wouldCauseSelfCheck = currentPlayer.isInCheck;
        board.ShogiPieces[x, y] = null;
        currentPlayer.CheckIfKingIsBeingAttacked();
        
        return wouldCauseSelfCheck;
    }

    public virtual void CheckForPromotion(){
        if (!isPromoted){
            if (player == PlayerNumber.Player1){
                if (CurrentY >= C.numberRows - 3) {
                    if (board.player1.playerCamera)
                        board.player1.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowPromotionMenu(this);
                    else GameUI.Instance.ShowPromotionMenu(this);
                }
            }
            else{
                if (CurrentY <= 2) {
                    if (board.player2.playerCamera)
                        board.player2.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowPromotionMenu(this);
                    else GameUI.Instance.ShowPromotionMenu(this);
                }
            }
        }
    }
    public virtual void Promote(bool changePosition = true){
        isPromoted = true;
        if (changePosition){
            SetHeight();
            SetRotation();
        }
    }
    public virtual void Unpromote(bool changePosition = true){
        isPromoted = false;
        if (changePosition){
            SetHeight();
            SetRotation();
        }
    }
    public virtual bool IsAttacked(){
        // only applicable to king piece
        return false;
    }
    public virtual bool CheckIfCouldBePromoted(int y){
        if (player == PlayerNumber.Player1){
            if (y >= C.numberRows - 3) {
                return true;
            }
            return false;
        }
        else{
            if (y <= 2) {
                return true;
            }
            return false;
        }
    }
}
