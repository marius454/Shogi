using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using C = Constants;


public class BoardManager : MonoBehaviour
{
    // Decide whether to allow free movement for dubugging
    public bool freeMove;

    public static BoardManager Instance{set;get;}

    public Player player1;
    public Player player2;
    public Player currentPlayer;
    public Player opponentPlayer;
    
    
    private bool[,] allowedMoves{set;get;}    
    private ShogiPiece selectedShogiPiece;

    private int selectionX;
    private int selectionY;

    
    public List<GameObject> piecePrefabs;
    public List<float> pieceZValues;
    private List<GameObject> activePieces;
    public ShogiPiece[,] ShogiPieces{set;get;}

    

    // Start is called before the first frame update
    private void Start(){
        // Make game cap out at 60 fps for less stress on the computer
        Application.targetFrameRate = 60;
        Instance = this;
        InitializeGame();
    }
    // Update is called once per frame
    private void Update(){
        UpdateSelection();
        DrawBoard();

        if (Input.GetMouseButtonDown (0)){
            if (selectionX >= 0 && selectionY >= 0){
                if (selectedShogiPiece == null || (ShogiPieces[selectionX, selectionY] != null && ShogiPieces[selectionX, selectionY].player == currentPlayer.playerNumber)){
                    // select the shogi piece
                    SelectShogiPiece(selectionX, selectionY);
                }else{
                    // move the shogi piece
                    MoveShogiPiece(selectionX, selectionY);
                }
            }
        }
    }
    private void InitializeGame(){
        player1 = new Player(PlayerNumber.player1, this);
        player2 = new Player(PlayerNumber.player2, this);
        selectionX = -1;
        selectionY = -1;

        activePieces = new List<GameObject>();
        ShogiPieces = new ShogiPiece[C.numberRows, C.numberRows];
        currentPlayer = player1;
        opponentPlayer = player2;

        SpawnAllShogiPieces();
        player1.InitializePiecesInPlay();
        player2.InitializePiecesInPlay();
        
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

    private void SpawnPiece(PieceType index, int x, int y, Quaternion rotation, PlayerNumber player){
        //position.z = pieceZValues[index];
        GameObject piece = Instantiate(piecePrefabs[(int)index], GetTileCenter(x, y), rotation) as GameObject;
        piece.transform.SetParent(transform);
        ShogiPieces[x, y] = piece.GetComponent<ShogiPiece>();
        ShogiPieces[x, y].Init(x, y, player, this);
        activePieces.Add(piece);
    }

    private void SpawnAllShogiPieces(){
        Quaternion rotation1 = Quaternion.Euler(-90.0f, 180.0f, 0.0f);
        Quaternion rotation2 = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

        // Kings
        SpawnPiece(PieceType.king, 4, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.king, 4, 8, rotation2, PlayerNumber.player2);

        // Rook
        SpawnPiece(PieceType.rook, 7, 1, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.rook, 1, 7, rotation2, PlayerNumber.player2);

        // Bishop
        SpawnPiece(PieceType.bishop, 1, 1, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.bishop, 7, 7, rotation2, PlayerNumber.player2);

        // Gold generals
        SpawnPiece(PieceType.gold, 3, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.gold, 5, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.gold, 3, 8, rotation2, PlayerNumber.player2);
        SpawnPiece(PieceType.gold, 5, 8, rotation2, PlayerNumber.player2);

        // Silver generals
        SpawnPiece(PieceType.silver, 2, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.silver, 6, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.silver, 2, 8, rotation2, PlayerNumber.player2);
        SpawnPiece(PieceType.silver, 6, 8, rotation2, PlayerNumber.player2);

        // Knights
        SpawnPiece(PieceType.knight, 1, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.knight, 7, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.knight, 1, 8, rotation2, PlayerNumber.player2);
        SpawnPiece(PieceType.knight, 7, 8, rotation2, PlayerNumber.player2);

        // Lances
        SpawnPiece(PieceType.lance, 0, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.lance, 8, 0, rotation1, PlayerNumber.player1);
        SpawnPiece(PieceType.lance, 0, 8, rotation2, PlayerNumber.player2);
        SpawnPiece(PieceType.lance, 8, 8, rotation2, PlayerNumber.player2);

        // Pawns (rotations switched, becouse of the orientation of the pawn asset)
        for (int i = 0; i < C.numberRows; i++){
            SpawnPiece(PieceType.pawn, i, 2, rotation2, PlayerNumber.player1);
            SpawnPiece(PieceType.pawn, i, 6, rotation1, PlayerNumber.player2);
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
        if (ShogiPieces[x, y].player != currentPlayer.playerNumber && !freeMove)
            return;
        if (ShogiPieces[x, y] == selectedShogiPiece){
            selectedShogiPiece = null;
            return;
        }

        selectedShogiPiece = ShogiPieces[x, y];
        if (!freeMove){
            allowedMoves = selectedShogiPiece.PossibleMoves();
            BoardHighlights.Instance.HideHighlights();
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
        else{
            allowedMoves = new bool [C.numberRows,C.numberRows];
            for (int i=0; i < C.numberRows*C.numberRows; i++) allowedMoves[i%C.numberRows,i/C.numberRows] = true;
            BoardHighlights.Instance.HideHighlights();
            BoardHighlights.Instance.HighlightAllowedMoves(selectedShogiPiece.PossibleMoves());
        }

        int nrMoves = 0;
        for (int a=0; a < C.numberRows; a++)
            for (int b=0; b < C.numberRows; b++){
                if (allowedMoves[a,b]){
                    Debug.Log("allowed tile at " + a + " " + b);
                    nrMoves++;
                }
            }
        //Debug.Log(opponentPlayer.playerNumber + " " + nrMoves);
    }
    private void MoveShogiPiece(int x, int y){
        if (allowedMoves[x,y]){
            ShogiPiece targetPiece = ShogiPieces[x,y];
            if (targetPiece != null && targetPiece.player != currentPlayer.playerNumber){
                // Capture piece
                activePieces.Remove(targetPiece.gameObject);
                opponentPlayer.RemovePieceInPlay(targetPiece);
                opponentPlayer.AddCapturedPiece(targetPiece);

                // will not be destroyed when capture and drop is implemented
                Destroy (targetPiece.gameObject);
            }

            ShogiPieces[selectedShogiPiece.CurrentX, selectedShogiPiece.CurrentY] = null;
            selectedShogiPiece.transform.position = GetTileCenter (x, y);
            selectedShogiPiece.SetPosition(x, y);
            ShogiPieces[x, y] = selectedShogiPiece;

            currentPlayer.CalculatePossibleMoves();
            opponentPlayer.CalculatePossibleMoves();


            // Check win condition
            if (CheckGameOver()){
                EndGame();
            }
            else if (!freeMove){
                ChangePlayer();
            }
        }
        BoardHighlights.Instance.HideHighlights();
        selectedShogiPiece = null;
    }
    private void ChangePlayer(){
        if (currentPlayer == player1){
            currentPlayer = player2;
            opponentPlayer = player1;
        }
        else{
            currentPlayer = player1;
            opponentPlayer = player2;
        }
    }

    private void EndGame(){
        Debug.Log("Victory for " + currentPlayer.playerNumber);
        // Lock The game from being played further
    }
    private bool CheckGameOver(){
        int nrMoves = 0;
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (opponentPlayer.possibleMoves[x,y]){
                    Debug.Log("attacking tile at " + x + " " + y);
                    nrMoves++;
                }
            }
        Debug.Log(opponentPlayer.playerNumber + " " + nrMoves);

        if (currentPlayer.isAttackingKing){
            opponentPlayer.isInCheck = true;
            if (!opponentPlayer.hasPossibleMoves) return true;
        }
        return false;
    }
}
