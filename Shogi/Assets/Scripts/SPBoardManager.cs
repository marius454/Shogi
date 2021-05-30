using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPBoardManager : BoardManager
{
    private void Awake(){
        checkForPerpetualCheck = true;
        Instance = this;
    }
    protected override void SelectShogiPiece(int x, int y, bool isSimulated = false){
        if (!ShogiPieces[x, y])
            return;
        if (ShogiPieces[x, y].player != currentPlayer.playerNumber && !freeMove)
            return;
        base.SelectShogiPiece(x, y);
    }
}
