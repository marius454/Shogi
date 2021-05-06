using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class CaptureBoard : MonoBehaviour
{
    [SerializeField] private PlayerNumber player;
    public BoardManager board{set;get;}
    public ShogiPiece[,] capturedPieces{set;get;}
    private int selectionX;
    private int selectionY;
    private int layerMask;
    // Ribos bus naudingos transformuojant absoliucias koordinates i masyvo koordinates
    public int maxX;
    public int maxY;
    public int minX;
    public int minY;
    private void Start(){
        Init();
    }
    private void Init(){
        board = BoardManager.Instance;
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
    private void SetBoardLimits(int maxX, int maxY, int minX, int minY){
        this.maxX = maxX;
        this.maxY = maxY;
        this.minX = minX;
        this.minY = minY;
    }
    private void Update(){
        DrawBoard();
    }
    private (int x, int y) CoordinatesToIndeces(int x, int y){
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
    public void AddPiece(ShogiPiece piece){
        // If needed, switching x and y places is exceptable depending on the order that is wanted for incoming captures
        for (int y=0; y < C.captureNumberRows; y++)
            for (int x=0; x < C.captureNumberColumns; x++){
                if (capturedPieces[x, y] == null){
                    if (player == PlayerNumber.Player1){
                        piece.transform.position = GetTileCenter (minX + x, maxY - y);
                        piece.SetXY(minX + x, maxY - y);
                    }
                    else {
                        piece.transform.position = GetTileCenter (maxX - x, minY + y);
                        piece.SetXY(maxX - x, minY + y);
                    }
                    piece.SetHeight();
                    piece.SetNormalRotation();
                    capturedPieces[x, y] = piece;
                    return;
                }
            }
    }
    private Vector3 GetTileCenter(int x, int y){
        Vector3 center = Vector3.zero;
        center.x += (C.tileSize * x) + C.tileOffset;
        center.z += (C.tileSize * y) + C.tileOffset;
        return center;
    }
    public void SelectCapturedPiece(int x, int y){
        (x, y) = CoordinatesToIndeces(x, y);

        board.selectedShogiPiece = capturedPieces[x, y];
        board.allowedMoves = board.selectedShogiPiece.PossibleDrops();
        BoardHighlights.Instance.HideHighlights();
        BoardHighlights.Instance.HighlightAllowedMoves(board.allowedMoves);
    }
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
                }
            }
    }
}
