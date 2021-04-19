using System;
using UnityEngine;
using C = Constants;

public abstract class ShogiPiece : MonoBehaviour
{
    public int CurrentX {set;get;}
    public int CurrentY {set;get;}
    public short player;

    public void SetPosition (int x, int y){
        CurrentX = x;
        CurrentY = y;
    }

    public virtual bool[,] PossibleMove(){
        return new bool[C.numberRows, C.numberRows];
    }

    protected void SingleMove(bool[,] moves, int x, int y){
        ShogiPiece potentialTile;
        
        if (x >= 0 && y >= 0 && x < C.numberRows && y < C.numberRows){
            potentialTile = BoardManager.Instance.ShogiPieces[x, y];
            if (potentialTile == null || potentialTile.player != player){
                moves[x, y] = true;
            }
        }
    }
    protected void OrthagonalLine(bool[,] moves, int direction){
        int x = CurrentX;
        int y = CurrentY;
        ShogiPiece potentialTile;
        while (true){
            if (direction == C.right) x++;
            else if (direction == C.forward) y++;
            else if (direction == C.left) x--;
            else if (direction == C.back) y--;
            else throw new InvalidOperationException("Invalid direction given for orthagoanlLine() method");

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
    protected void DiagonalLine(bool[,] moves, int direction){
        int x = CurrentX;
        int y = CurrentY;
        ShogiPiece potentialTile;
        while (true){
            if (direction == C.forwardLeft) {x--; y++;}
            else if (direction == C.forwardRight) {x++; y++;}
            else if (direction == C.backLeft) {x--; y--;}
            else if (direction == C.backRight) {x++; y--;}
            else throw new InvalidOperationException("Invalid direction given for orthagoanlLine() method");

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
}
