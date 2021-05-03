using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class CaptureBoard : MonoBehaviour
{
    [SerializeField] private PlayerNumber player;
    private int selectionX;
    private int selectionY;
    // public CaptureBoard (Player player){
    //     this.player = player;
    //     selectionX = -10;
    //     selectionY = -10;
    // }
    private void Start(){
        // DrawBoard();
    }
    private void Update(){
        UpdateSelection();
        DrawBoard();
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
            selectionX = -10;
            selectionY = -10;
        }
    }
    private void DrawBoard(){
        // unitary vector going to the right and forward for one unit of space 8 times
        Vector3 widthLine = Vector3.right * C.captureNumberRows;
        Vector3 heightLine = Vector3.forward * C.captureNumberColumns;
        Vector3 startPoint;

        if (player == PlayerNumber.player1) startPoint = new Vector3(C.numberRows + 1, 0, 0);
        else startPoint = new Vector3(-2 - C.captureNumberColumns, 0, C.numberRows - C.captureNumberColumns);

        for (int i = 0; i <= C.captureNumberColumns; i++){
            Vector3 startRow = startPoint + (Vector3.forward * i);
            Debug.DrawLine(startRow, startRow + widthLine);
            for (int j = 0; j <= C.captureNumberRows; j++){
                Vector3 startCol = startPoint + (Vector3.right * j);
                Debug.DrawLine(startCol, startCol + heightLine);
            }
        }

        // Draw the selection
        // if (selectionX >= 0 && selectionY >= 0){
        //     //Debug.Log(selectionX + " " + selectionY);
        //     Debug.DrawLine(
        //         Vector3.forward * selectionY + Vector3.right * selectionX,
        //         Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1)
        //     );
        //     Debug.DrawLine(
        //         Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
        //         Vector3.forward * selectionY + Vector3.right * (selectionX + 1)
        //     );
        // }
    }
}
