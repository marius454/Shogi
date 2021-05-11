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
    // private void Update(){
    //     DrawBoard();
    // }
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
                    piece.Unpromote();
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
