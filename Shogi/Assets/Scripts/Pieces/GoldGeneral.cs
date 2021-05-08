using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class GoldGeneral : ShogiPiece
{
    public GoldGeneral(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    public override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.GoldGeneral - 0.01f, gameObject.transform.position.z);
    }
    
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        GoldMove();
        RemoveIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
    public override void Promote()
    {
        // Golden general cannot be promoted
    }
}
