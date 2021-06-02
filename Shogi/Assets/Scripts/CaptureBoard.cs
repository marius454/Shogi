using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class CaptureBoard : MonoBehaviour
{
    [SerializeField] protected PlayerNumber player;
    public ShogiPiece[,] capturedPieces { set; get; }
    protected int selectionX;
    protected int selectionY;
    protected int layerMask;
    // The boundarie coordinates of the capture board
    public int maxX { set; get; }
    public int maxY { set; get; }
    public int minX { set; get; }
    public int minY { set; get; }
    private void Awake(){
        Init();
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
    }
}
