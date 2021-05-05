using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class CaptureBoard : MonoBehaviour
{
    [SerializeField] private PlayerNumber player;
    public BoardManager board{set;get;}
    public ShogiPiece[,] capturedPieces{set;get;}
    private ShogiPiece selectedCapturedPiece{set;get;}
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
        if (player == PlayerNumber.player1){
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
        UpdateSelection();
        DrawBoard();
    }
    private (int x, int y) CoordinatesToIndeces(int x, int y){
        if (player == PlayerNumber.player1){
            x = x - minX;
            y = maxY - y;
        } 
        else{
            x = maxX - x;
            y = y - minY;
        } 
        return (x, y);
    }
    private void UpdateSelection(){
        if (!Camera.main)
            return;

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, layerMask)){
            int x, z;
            if (hit.point.x < 0) x = (int)hit.point.x - 1;
            else x = (int)hit.point.x;

            if (hit.point.z < 0) z = (int)hit.point.z - 1;
            else z = (int)hit.point.z;

            selectionX = x;
            selectionY = z;
        }
        // else {
        //     selectionX = minX - 1;
        //     selectionY = minY - 1;
        // }
    }
    // private void ComputeMouseClick(){
    //     // if (Input.GetMouseButtonDown (0) && player == board.currentPlayer.playerNumber){
    //     //     Debug.Log(selectedCapturedPiece + " " + selectionX + " " + selectionY);
    //     //     if (selectionX >= minX && selectionY >= minY && selectionX <= maxX && selectionY <= maxY){
    //     //         int x, y;
    //     //         (x, y) = CoordinatesToIndeces(selectionX, selectionY);
    //     //         if (selectedCapturedPiece == null){
    //     //             Debug.Log("good select");
    //     //             Debug.Log(capturedPieces[x, y]);
    //     //             SelectCapturedPiece(x, y);
    //     //         }
    //     //         if (selectedCapturedPiece != null){
    //     //             Debug.Log("good unselect");
    //     //             UnselectCapturedPiece();
    //     //         }
    //     //     }
    //     //     else if (selectionX >= 0 && selectionY >= 0 && selectionX <= C.numberRows && selectionY <= C.numberRows){
    //     //         Debug.Log("good board");
    //     //         Debug.Log(selectedCapturedPiece);
    //     //         if (selectedCapturedPiece != null){
    //     //             Debug.Log("good drop");
    //     //             DropCapturedPiece(selectionX, selectionY);
    //     //         }
    //     //     }
    //     //     else{
    //     //         if (selectedCapturedPiece != null){
    //     //             UnselectCapturedPiece();
    //     //         }
    //     //     }
    //     // }
    //     if (Input.GetMouseButtonDown (0) && player == board.currentPlayer.playerNumber){
    //         if (selectionX >= 0 && selectionY >= 0 && selectionX <= C.numberRows && selectionY <= C.numberRows){
    //             if (board.selectedShogiPiece == null || capturedPieces[selectionX, selectionY]){
    //                 SelectCapturedPiece(selectionX, selectionY);
    //             }else{
    //                 // DropCapturedPiece(selectionX, selectionY);
    //             }
    //         }
    //     }
    // }
    private void DrawBoard(){
        // unitary vector going to the right and forward for one unit of space 8 times
        Vector3 widthLine = Vector3.right * C.captureNumberColumns;
        Vector3 heightLine = Vector3.forward * C.captureNumberRows;
        Vector3 startPoint;

        if (player == PlayerNumber.player1) startPoint = new Vector3(C.numberRows + 1, 0, 0);
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
    public void AddPiece(ShogiPiece piece){
        // if needed, switching x and y places is exceptable depending on the order that is wanted for incoming captures
        for (int y=0; y < C.captureNumberRows; y++)
            for (int x=0; x < C.captureNumberColumns; x++){
                if (capturedPieces[x, y] == null){
                    if (player == PlayerNumber.player1){
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
                    Debug.Log(x + " " + y);
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
        
        if (capturedPieces[x, y] == board.selectedShogiPiece){
            selectedCapturedPiece = null;
            return;
        }

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
    private void UnselectCapturedPiece(){
        if (selectedCapturedPiece.CurrentX != selectionX || selectedCapturedPiece.CurrentY != selectionY){
            board.allowedMoves = new bool[C.numberRows, C.numberRows];
            BoardHighlights.Instance.HideHighlights();
            selectedCapturedPiece = null;
        }
    }

    public void DropCapturedPiece(ShogiPiece piece){
        // Debug.Log("good");
        // if (board.allowedMoves[x, y]){
        //     board.DropShogiPiece(selectedCapturedPiece, x, y);
        //     capturedPieces[selectedCapturedPiece.CurrentX, selectedCapturedPiece.CurrentY] = null;
        //     UnselectCapturedPiece();
        // }
        int x, y;
        (x, y) = CoordinatesToIndeces(piece.CurrentX, piece.CurrentY);
        capturedPieces[x, y] = null;
    }
}
