using System;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public abstract class ShogiPiece : MonoBehaviour
{
    public int CurrentX {set;get;}
    public int CurrentY {set;get;}
    public PlayerNumber player;
    public BoardManager board;

    public void SetPosition (int x, int y){
        CurrentX = x;
        CurrentY = y;
    }

    public virtual bool[,] PossibleMoves(){
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
    // public List<ShogiPiece> GetAttackingPieces(){
    //     List<ShogiPiece> attackedPieces = new List<ShogiPiece>();
    //     bool[,] moves = PossibleMoves();
    //     for (int x=0; x < C.numberRows; x++)
    //         for (int y=0; y < C.numberRows; y++){
    //             if (moves[x,y] == true && board.ShogiPieces[x,y]){
    //                 attackedPieces.Add(board.ShogiPieces[x,y]);
    //             }
    //         }
        
    //     return attackedPieces;
    // }
}
