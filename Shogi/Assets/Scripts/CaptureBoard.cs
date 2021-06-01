using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class CaptureBoard : MonoBehaviour
{
    [SerializeField] protected PlayerNumber player;
    // protected BoardManager board;
    public ShogiPiece[,] capturedPieces { set; get; }
    protected int selectionX;
    protected int selectionY;
    protected int layerMask;
    // Ribos bus naudingos transformuojant absoliucias koordinates i masyvo koordinates
    public int maxX { set; get; }
    public int maxY { set; get; }
    public int minX { set; get; }
    public int minY { set; get; }
    private void Awake(){
        // board = BoardManager.Instance;
        Init();
    }
    private void Update(){
        DrawBoard();
    }
    // Drawing a representation of the board to for easier debugging
    private void DrawBoard(){
        Vector3 widthLine = Vector3.right * C.captureNumberColumns;
        Vector3 heightLine = Vector3.forward * C.captureNumberRows;
        Vector3 startPoint;

        if (player == PlayerNumber.Player1) startPoint = new Vector3(C.numberRows + 1, 0, 0);
        else startPoint = new Vector3(-1 - C.captureNumberColumns, 0, C.numberRows - C.captureNumberRows);

        for (int i = 0; i <= C.captureNumberRows; i++){
            Vector3 startRow = startPoint + (Vector3.forward * i);
            Debug.DrawLine(startRow, startRow + widthLine);
            for (int j = 0; j <= C.captureNumberColumns; j++){
                Vector3 startCol = startPoint + (Vector3.right * j);
                Debug.DrawLine(startCol, startCol + heightLine);
            }
        }

        // Draw the selection
        if (selectionX >= minX && selectionY >= minY && selectionX <= maxX && selectionY <= maxY){
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
    protected void Init(){
        capturedPieces = new ShogiPiece[C.captureNumberColumns, C.captureNumberRows];
        if (player == PlayerNumber.Player1){
            layerMask = LayerMask.GetMask("CapturePlanePlayer1");
            SetBoardLimits(C.numberRows + C.captureNumberColumns, C.captureNumberColumns, C.numberRows + 1, 0);
        } 
        else{
            layerMask = LayerMask.GetMask("CapturePlanePlayer2");
            SetBoardLimits(-2, C.numberRows - 1, -1 - C.captureNumberColumns, C.numberRows - C.captureNumberRows);
        } 
    }
    protected void SetBoardLimits(int maxX, int maxY, int minX, int minY){
        this.maxX = maxX;
        this.maxY = maxY;
        this.minX = minX;
        this.minY = minY;
    }
    public (int x, int y) CoordinatesToIndeces(int x, int y){
        if (player == PlayerNumber.Player1){
            x = x - minX;
            y = maxY - y;
        } 
        else{
            x = maxX - x;
            y = y - minY;
        } 
        return (x, y);
    }
    
    public void AddPiece(ShogiPiece piece, bool isSimulated = false){
        // If needed, switching x and y places is exceptable depending on the order that is wanted for incoming captures
        for (int y=0; y < C.captureNumberRows; y++)
            for (int x=0; x < C.captureNumberColumns; x++){
                if (!capturedPieces[x, y]){
                    if (!isSimulated){
                        if (player == PlayerNumber.Player1){
                        piece.transform.position = GetTileCenter (minX + x, maxY - y);
                        piece.SetXY(minX + x, maxY - y, false);
                        }
                        else {
                            piece.transform.position = GetTileCenter (maxX - x, minY + y);
                            piece.SetXY(maxX - x, minY + y, false);
                        }
                        piece.Unpromote();
                    }
                    else piece.Unpromote(false);
                    
                    capturedPieces[x, y] = piece;
                    return;
                }
            }
    }
    public Vector3 GetTileCenter(int x, int y){
        Vector3 center = Vector3.zero;
        center.x += (C.tileSize * x) + C.tileOffset;
        center.z += (C.tileSize * y) + C.tileOffset;
        return center;
    }
    // public virtual void SelectCapturedPiece(int x, int y){
    //     OnCapturedPieceSelect(x, y);
    // }
    // protected void OnCapturedPieceSelect(int x, int y){
    //     (x, y) = CoordinatesToIndeces(x, y);
        
    //     board.selectedShogiPiece = capturedPieces[x, y];
    //     board.allowedMoves = board.selectedShogiPiece.PossibleDrops();
    // }
    public bool ExistsPiece(int x, int y){
        (x, y) = CoordinatesToIndeces(x, y);
        if (capturedPieces[x, y]) return true;
        else return false;
    }
    public void DropCapturedPiece(ShogiPiece piece){
        int x, y;
        (x, y) = CoordinatesToIndeces(piece.CurrentX, piece.CurrentY);
        capturedPieces[x, y] = null;
    }
    public void DestroyAllPieces(){
        for (int x=0; x < C.captureNumberColumns; x++)
            for (int y=0; y < C.captureNumberRows; y++){
                if (capturedPieces[x, y]){
                    Destroy (capturedPieces[x, y].gameObject);
                    capturedPieces[x, y] = null;
                }
            }
        //capturedPieces = new ShogiPiece[C.captureNumberColumns, C.captureNumberRows];
    }
    public void SpawnPiece(BoardManager board, PieceType pieceType, GameObject piecePrefab, int x, int y, PlayerNumber player){
        GameObject piece = Instantiate(piecePrefab) as GameObject;
        piece.transform.SetParent(transform);
        capturedPieces[x, y] = piece.GetComponent<ShogiPiece>();
        capturedPieces[x, y].Init(x, y, player, pieceType, board);
        //activePieceObjects.Add(piece);
    }
}
