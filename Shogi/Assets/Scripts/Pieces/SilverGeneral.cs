using System;
using UnityEngine;
using C = Constants;
using Y = PieceYValues;

public class SilverGeneral : ShogiPiece
{
    public SilverGeneral(int x, int y, PlayerNumber player, BoardManager board) : base(x, y, player, board){}
    protected override void SetNormalHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.SilverGeneral - 0.01f, gameObject.transform.position.z);
    }
    protected override void SetPromotedHeight(){
        this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, Y.promotedSilverGeneral - 0.01f, gameObject.transform.position.z);
    }
    public override bool[,] PossibleMoves(bool checkForSelfCheck = true){
        moves = new bool[C.numberRows,C.numberRows];
        int a = 1;

        if (!isPromoted){
            // Select all moves in a 3x3 square around the piece, except it's current position and the tile it cannot go
            if (player == PlayerNumber.Player1){
                for (int t = -a; t <= a; t++)
                    for (int s = -a; s <= a; s++){
                        if (!(t == 0 && s == 0) && !(t == 1 && s == 0) && !(t == -1 && s == 0) && !(t == 0 && s == -1))
                            SingleMove(moves, CurrentX + t, CurrentY + s);
                    }
            }
            else if (player == PlayerNumber.Player2) {
                for (int t = -a; t <= a; t++)
                    for (int s = -a; s <= a; s++){
                        if (!(t == 0 && s == 0) && !(t == 1 && s == 0) && !(t == -1 && s == 0) && !(t == 0 && s == 1))
                            SingleMove(moves, CurrentX + t, CurrentY + s);
                    }
            }
            else {
                throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
            }
        }
        else {
            GoldMove();
        }
        

        RemoveIllegalMoves(moves, checkForSelfCheck);
        return moves;
    }
}
