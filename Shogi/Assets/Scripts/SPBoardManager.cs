using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPBoardManager : BoardManager
{
    private void Awake(){
        Instance = this;
        // this.gameObject.transform.parent.transform.Find("CaptureBoardPlayer1").GetComponent<CaptureBoard>().InitializeCaptureBoard();
        // this.gameObject.transform.parent.transform.Find("CaptureBoardPlayer2").GetComponent<CaptureBoard>().InitializeCaptureBoard();
    }
    protected override void SelectShogiPiece(int x, int y){
        if (ShogiPieces[x, y] == null)
            return;
        if (ShogiPieces[x, y].player != currentPlayer.playerNumber && !freeMove)
            return;
        base.SelectShogiPiece(x, y);
    }
}
