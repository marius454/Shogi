// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class BoardLayout : MonoBehaviour
// {
//     private BoardManager board;
//     public List<GameObject> piecePrefabs;
//     public BoardLayout(BoardManager board){
//         this.board = board;
//     }
//     public void buildBoard(){
//         SpawnAllShogiPieces();
//     }
//         private void SpawnPiece(PieceType index, int x, int y, Quaternion rotation, PlayerNumber player){
//         //position.z = pieceZValues[index];
//         GameObject piece = Instantiate(piecePrefabs[(int)index], board.GetTileCenter(x, y), rotation) as GameObject;
//         piece.transform.SetParent(transform);
//         board.ShogiPieces[x, y] = piece.GetComponent<ShogiPiece>();
//         board.ShogiPieces[x, y].SetPosition(x,y);
//         board.ShogiPieces[x, y].player = player;
//         board.activePieces.Add(piece);
//     }

//     private void SpawnAllShogiPieces(){
//         Quaternion rotation1 = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
//         Quaternion rotation2 = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

//         // Kings
//         SpawnPiece(PieceType.king, 4, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.king, 4, 8, rotation2, PlayerNumber.player2);

//         // Rook
//         SpawnPiece(PieceType.rook, 7, 1, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.rook, 1, 7, rotation2, PlayerNumber.player2);

//         // Bishop
//         SpawnPiece(PieceType.bishop, 1, 1, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.bishop, 7, 7, rotation2, PlayerNumber.player2);

//         // Gold generals
//         SpawnPiece(PieceType.gold, 3, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.gold, 5, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.gold, 3, 8, rotation2, PlayerNumber.player2);
//         SpawnPiece(PieceType.gold, 5, 8, rotation2, PlayerNumber.player2);

//         // Silver generals
//         SpawnPiece(PieceType.silver, 2, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.silver, 6, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.silver, 2, 8, rotation2, PlayerNumber.player2);
//         SpawnPiece(PieceType.silver, 6, 8, rotation2, PlayerNumber.player2);

//         // Knights
//         SpawnPiece(PieceType.knight, 1, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.knight, 7, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.knight, 1, 8, rotation2, PlayerNumber.player2);
//         SpawnPiece(PieceType.knight, 7, 8, rotation2, PlayerNumber.player2);

//         // Lances
//         SpawnPiece(PieceType.lance, 0, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.lance, 8, 0, rotation1, PlayerNumber.player1);
//         SpawnPiece(PieceType.lance, 0, 8, rotation2, PlayerNumber.player2);
//         SpawnPiece(PieceType.lance, 8, 8, rotation2, PlayerNumber.player2);

//         // Pawns (rotations switched, becouse of the orientation of the pawn asset)
//         for (int i = 0; i < C.numberRows; i++){
//             SpawnPiece(PieceType.pawn, i, 2, rotation2, PlayerNumber.player1);
//             SpawnPiece(PieceType.pawn, i, 6, rotation1, PlayerNumber.player2);
//         }
//     }
// }
