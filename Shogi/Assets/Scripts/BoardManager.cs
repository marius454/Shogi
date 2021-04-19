using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance{set;get;}
    private bool[,] allowedMoves{set;get;}

    public ShogiPiece[,] ShogiPieces{set;get;}
    private ShogiPiece selectedShogiPiece;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> piecePrefabs;
    public List<float> pieceZValues;
    private List<GameObject> activePiece;

    public short currentPlayer;


    // Start is called before the first frame update
    private void Start(){
        // Make game cap out at 60 fps for less stress on the computer
        Application.targetFrameRate = 60;
        Instance = this;
        SpawnAllShogiPieces();
    }
    // Update is called once per frame
    private void Update(){
        UpdateSelection();
        DrawBoard();

        if (Input.GetMouseButtonDown (0)){
            if (selectionX >= 0 && selectionY >= 0){
                if (selectedShogiPiece == null || (ShogiPieces[selectionX, selectionY] != null && ShogiPieces[selectionX, selectionY].player == currentPlayer)){
                    // select the shogi piece
                    SelectShogiPiece(selectionX, selectionY);
                }else{
                    // move the shogi piece
                    MoveShogiPiece(selectionX, selectionY);
                }
            }
        }
    }

    private void UpdateSelection(){
        if (!Camera.main)
            return;

        // Make a ray collision on the ShogiPlane object
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ShogiPlane"))){
            // Check where the ray collided with the plane
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private Vector3 GetTileCenter(int x, int y){
        Vector3 center = Vector3.zero;
        center.x += (C.tileSize * x) + C.tileOffset;
        center.z += (C.tileSize * y) + C.tileOffset;
        return center;
    }

    private void SpawnPiece(int index, int x, int y, Quaternion rotation, short player){
        //position.z = pieceZValues[index];
        GameObject piece = Instantiate(piecePrefabs[index], GetTileCenter(x, y), rotation) as GameObject;
        piece.transform.SetParent(transform);
        ShogiPieces[x, y] = piece.GetComponent<ShogiPiece>();
        ShogiPieces[x, y].SetPosition(x,y);
        ShogiPieces[x, y].player = player;
        activePiece.Add(piece);
    }

    private void SpawnAllShogiPieces(){
        Quaternion rotation1 = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
        Quaternion rotation2 = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

        activePiece = new List<GameObject>();
        ShogiPieces = new ShogiPiece[C.numberRows, C.numberRows];
        currentPlayer = 1;

        // Kings
        SpawnPiece(C.king, 4, 0, rotation1, 1);
        SpawnPiece(C.king, 4, 8, rotation2, 2);

        // Rook
        SpawnPiece(C.rook, 7, 1, rotation1, 1);
        SpawnPiece(C.rook, 1, 7, rotation2, 2);

        // Bishop
        SpawnPiece(C.bishop, 1, 1, rotation1, 1);
        SpawnPiece(C.bishop, 7, 7, rotation2, 2);

        // Gold generals
        SpawnPiece(C.gold, 3, 0, rotation1, 1);
        SpawnPiece(C.gold, 5, 0, rotation1, 1);
        SpawnPiece(C.gold, 3, 8, rotation2, 2);
        SpawnPiece(C.gold, 5, 8, rotation2, 2);

        // Silver generals
        SpawnPiece(C.silver, 2, 0, rotation1, 1);
        SpawnPiece(C.silver, 6, 0, rotation1, 1);
        SpawnPiece(C.silver, 2, 8, rotation2, 2);
        SpawnPiece(C.silver, 6, 8, rotation2, 2);

        // Knights
        SpawnPiece(C.knight, 1, 0, rotation1, 1);
        SpawnPiece(C.knight, 7, 0, rotation1, 1);
        SpawnPiece(C.knight, 1, 8, rotation2, 2);
        SpawnPiece(C.knight, 7, 8, rotation2, 2);

        // Lances
        SpawnPiece(C.lance, 0, 0, rotation1, 1);
        SpawnPiece(C.lance, 8, 0, rotation1, 1);
        SpawnPiece(C.lance, 0, 8, rotation2, 2);
        SpawnPiece(C.lance, 8, 8, rotation2, 2);

        // Pawns (rotations switched, becouse of the orientation of the pawn asset)
        for (int i = 0; i < C.numberRows; i++){
            SpawnPiece(C.pawn, i, 2, rotation2, 1);
            SpawnPiece(C.pawn, i, 6, rotation1, 2);
        }
    }

    private void DrawBoard(){
        // unitary vector going to the right and forward for one unit of space 8 times
        Vector3 widthLine = Vector3.right * C.numberRows;
        Vector3 heightLine = Vector3.forward * C.numberRows;

        for (int i = 0; i <= C.numberRows; i++){
            Vector3 startRow = Vector3.forward * i;
            Debug.DrawLine(startRow, startRow + widthLine);
            for (int j = 0; j <= C.numberRows; j++){
                Vector3 startCol = Vector3.right * j;
                Debug.DrawLine(startCol, startCol + heightLine);
            }
        }

        // Draw the selection
        if (selectionX >= 0 && selectionY >= 0){
            //Debug.Log(selectionX + " " + selectionY);
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1)
            );
            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1)
            );
        }
    }
    private void SelectShogiPiece(int x, int y){
        if (ShogiPieces[x, y] == null)
            return;
        if (ShogiPieces[x, y].player != currentPlayer)
            return;

        
        allowedMoves = ShogiPieces[x, y].PossibleMove();
        selectedShogiPiece = ShogiPieces[x, y];

        BoardHighlights.Instance.HideHighlights();
        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
    }
    private void MoveShogiPiece(int x, int y){
        if (allowedMoves[x,y]){
            ShogiPiece targetPiece = ShogiPieces[x,y];
            if (targetPiece != null && targetPiece.player != currentPlayer){
                // Capture piece
                activePiece.Remove(targetPiece.gameObject);
                Destroy (targetPiece.gameObject);
            }

            ShogiPieces[selectedShogiPiece.CurrentX, selectedShogiPiece.CurrentY] = null;
            selectedShogiPiece.transform.position = GetTileCenter (x, y);
            selectedShogiPiece.SetPosition(x, y);
            ShogiPieces[x, y] = selectedShogiPiece;

            // Check win condition
            // ..
            
            if (currentPlayer == 1) currentPlayer = 2;
            else currentPlayer = 1;
        }
        
        BoardHighlights.Instance.HideHighlights();
        selectedShogiPiece = null;
    }
}
