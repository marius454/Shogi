using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class GoldGeneral : ShogiPiece
{
    public GoldGeneral(int x, int y, PlayerNumber player, PieceType pieceType, BoardManager board) : base(x, y, player, pieceType, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.GoldGeneral - 0.01f, gameObject.transform.position.z);
    }
    
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        GoldMove();
        moves = RemoveIllegalMoves(moves, checkForSelfCheck);
        
        return moves;
    }
    public override void CheckForPromotion(){
        // Golden general cannot be promoted
    }
    public override void Promote(bool changePosition = true)
    {
        // Golden general cannot be promoted
    }
    public override void Unpromote(bool changePosition = true)
    {
        // Golden general will never be promoted to be unpromoted
    }
    public override bool CheckIfCouldBePromoted(int y){
        // Golden general cannot be promoted
        return false;
    }
}
