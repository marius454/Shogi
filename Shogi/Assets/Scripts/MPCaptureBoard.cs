// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Photon.Pun;
// using System;

// public class MPCaptureBoard : CaptureBoard
// {    
//     private MPBoardManager board;
//     private PhotonView photonView{set; get;}
//     protected override void Awake(){
//         mpBoard = (MPBoardManager)MPBoardManager.Instance;
//         board = mpBoard;
//         Init();
//     }

//     public override void SelectCapturedPiece(int x, int y){
//         if (capturedPieces[x, y] == null)
//             return;
//         if (capturedPieces[x, y].player != mpBoard.localPlayer.playerNumber)
//             return;

//         mpBoard.pieceSelectedByPlayer = capturedPieces[x, y];
//         mpBoard.possibleMovesForSelectedPiece = mpBoard.pieceSelectedByPlayer.PossibleMoves();
//         // Debug.LogError("Piece Selected " + pieceSelectedByPlayer);

//         if (mpBoard.currentPlayer != mpBoard.localPlayer) return;
//         photonView.RPC(nameof(RPC_SelectCapturedPiece), RpcTarget.AllBuffered, new object[] { x, y });
//     }
//     [PunRPC]
//     private void RPC_SelectCapturedPiece(int x, int y)
//     {
//         OnCapturedPieceSelect(x, y);
//     }
// }
