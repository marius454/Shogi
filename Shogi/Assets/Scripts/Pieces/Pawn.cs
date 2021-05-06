using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class Pawn : ShogiPiece
{
    public Pawn(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.Pawn, gameObject.transform.position.z);
    }
    public override void SetNormalRotation()
    {
        if (player == PlayerNumber.Player1){
            Quaternion rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        } 
        else {
            Quaternion rotation = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
            this.gameObject.transform.rotation = rotation;
        }
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];

        if (player == PlayerNumber.Player1)
            SingleMove(moves, CurrentX, CurrentY + 1);
        else if (player == PlayerNumber.Player2)
            SingleMove(moves, CurrentX, CurrentY - 1);
        else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        
        removeIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
}
