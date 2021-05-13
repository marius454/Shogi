using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using C = Constants;


public class BoardManager : MonoBehaviour
{
    // Decide whether to allow free movement for dubugging
    public bool freeMove;

    public static BoardManager Instance{set;get;}
    // [SerializeField] private GameObject CaptureBoardPlayer1;
    // [SerializeField] private GameObject CaptureBoardPlayer2;

    public ShogiPlayer player1;
    public ShogiPlayer player2;
    public ShogiPlayer currentPlayer;
    public ShogiPlayer opponentPlayer;
    
    public bool[,] allowedMoves{set;get;}
    public ShogiPiece selectedShogiPiece{set;get;}

    protected int selectionX;
    protected int selectionY;

    public List<GameObject> piecePrefabs;
    //private List<GameObject> activePieceObjects;
    public ShogiPiece[,] ShogiPieces{set;get;}

    // Start is called before the first frame update
    private void Awake(){
        // Instance = this;
    }
    // Update is called once per frame
    private void Update(){
        if (GameController.Instance.gameStarted && this.gameObject.activeSelf){
            UpdateSelection();
            MarkSelectedPiece();
            MarkAllowedMoves();
            MarkCheckedKing();
            ComputeMouseClick();
        }
    }
    protected virtual void MarkSelectedPiece(){
        if (selectedShogiPiece){
            BoardHighlights.Instance.HighlightSelection(selectedShogiPiece.CurrentX, selectedShogiPiece.CurrentY);
        }
        else {
            BoardHighlights.Instance.HideSelectionHighlight();
        }
    }
    protected virtual void MarkAllowedMoves(){
        if (selectedShogiPiece){
            BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);
        }
        else {
            BoardHighlights.Instance.HideHighlights();
        }
    }
    protected void MarkCheckedKing(){
        if (player1.isAttackingKing){
            player2.PlaceInCheck();
        }
        else {
            player2.RemoveCheck();
        }

        if (player2.isAttackingKing){
            player1.PlaceInCheck();
        }
        else {
            player1.RemoveCheck();
        }
    }
    public void InitializeGame(){
        player1 = new ShogiPlayer(PlayerNumber.Player1, this, GameObject.Find("CaptureBoardPlayer1").GetComponent<CaptureBoard>());
        player2 = new ShogiPlayer(PlayerNumber.Player2, this, GameObject.Find("CaptureBoardPlayer2").GetComponent<CaptureBoard>());
        selectionX = -1;
        selectionY = -1;

        //activePieceObjects = new List<GameObject>();
        ShogiPieces = new ShogiPiece[C.numberRows, C.numberRows];
        currentPlayer = player1;
        opponentPlayer = player2;

        
        Debug.LogError("player1 " + player1);
        Debug.LogError("player2 " + player2);

        SpawnAllShogiPieces();
        // SpawnPiece(PieceType.king, 4, 4, PlayerNumber.Player1);
        player1.InitializePiecesInPlay();
        player2.InitializePiecesInPlay();
    }
    protected void UpdateSelection(){
        if (!Camera.main)
            return;

        // Make a ray collision on the ShogiPlane object
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ShogiPlane"))){
            // Check where the ray collided with the plane
            if ((int)hit.point.x >= 0){
                selectionX = (int)hit.point.x;
            }
            else {
                // Special case for player2 capture board
                selectionX = (int)hit.point.x - 1;
            }
            selectionY = (int)hit.point.z;
        }
        else {
            selectionX = -1;
            selectionY = -1;
        }
    }
    protected virtual void ComputeMouseClick(){
        if (Input.GetMouseButtonDown (0) && !EventSystem.current.IsPointerOverGameObject()){
            if (selectionX >= 0 && selectionY >= 0 && selectionX <= C.numberRows && selectionY <= C.numberRows){
                if (selectedShogiPiece == null || (ShogiPieces[selectionX, selectionY] != null && ShogiPieces[selectionX, selectionY].player == currentPlayer.playerNumber && !freeMove)){
                    SelectShogiPiece(selectionX, selectionY);
                }else{
                    if (currentPlayer.capturedPieces.Contains(selectedShogiPiece)){
                        DropShogiPiece(selectionX, selectionY);
                    }
                    else{
                        MoveShogiPiece(selectionX, selectionY);
                    }
                }
            }
            else if (selectionX >= currentPlayer.captureBoard.minX && selectionY >= currentPlayer.captureBoard.minY 
              && selectionX <= currentPlayer.captureBoard.maxX && selectionY <= currentPlayer.captureBoard.maxY)
            {
                if (currentPlayer.captureBoard.ExistsPiece(selectionX, selectionY)){
                    SelectCapturedPiece(selectionX, selectionY);
                }
            }
        }
    }
    public Vector3 GetTileCenter(int x, int y){
        Vector3 center = Vector3.zero;
        center.x += (C.tileSize * x) + C.tileOffset;
        center.z += (C.tileSize * y) + C.tileOffset;
        return center;
    }
    protected void SpawnPiece(PieceType index, int x, int y, PlayerNumber player){
        GameObject piece = Instantiate(piecePrefabs[(int)index], GetTileCenter(x, y), Quaternion.Euler(0,0,0)) as GameObject;
        piece.transform.SetParent(transform);
        ShogiPieces[x, y] = piece.GetComponent<ShogiPiece>();
        ShogiPieces[x, y].Init(x, y, player, this);
        //activePieceObjects.Add(piece);
    }
    protected void SpawnAllShogiPieces(){
        // Kings
        SpawnPiece(PieceType.king, 4, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.king, 4, 8, PlayerNumber.Player2);

        // Rook
        SpawnPiece(PieceType.rook, 7, 1, PlayerNumber.Player1);
        SpawnPiece(PieceType.rook, 1, 7, PlayerNumber.Player2);

        // Bishop
        SpawnPiece(PieceType.bishop, 1, 1, PlayerNumber.Player1);
        SpawnPiece(PieceType.bishop, 7, 7, PlayerNumber.Player2);

        // Gold generals
        SpawnPiece(PieceType.gold, 3, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.gold, 5, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.gold, 3, 8, PlayerNumber.Player2);
        SpawnPiece(PieceType.gold, 5, 8, PlayerNumber.Player2);

        // Silver generals
        SpawnPiece(PieceType.silver, 2, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.silver, 6, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.silver, 2, 8, PlayerNumber.Player2);
        SpawnPiece(PieceType.silver, 6, 8, PlayerNumber.Player2);

        // Knights
        SpawnPiece(PieceType.knight, 1, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.knight, 7, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.knight, 1, 8, PlayerNumber.Player2);
        SpawnPiece(PieceType.knight, 7, 8, PlayerNumber.Player2);

        // Lances
        SpawnPiece(PieceType.lance, 0, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.lance, 8, 0, PlayerNumber.Player1);
        SpawnPiece(PieceType.lance, 0, 8, PlayerNumber.Player2);
        SpawnPiece(PieceType.lance, 8, 8, PlayerNumber.Player2);

        for (int i = 0; i < C.numberRows; i++){
            SpawnPiece(PieceType.pawn, i, 2, PlayerNumber.Player1);
            SpawnPiece(PieceType.pawn, i, 6, PlayerNumber.Player2);
        }
    }    
    protected virtual void SelectShogiPiece(int x, int y){
        OnShogiPieceSelect(x, y);
    }
    protected void OnShogiPieceSelect(int x, int y){
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
    }
    protected virtual void MoveShogiPiece(int x, int y){
        OnShogiPieceMove(x, y);
    }
    protected virtual void OnShogiPieceMove(int x, int y){
        if (allowedMoves[x,y]){
            // Capture piece if possible
            CapturePieceIfPossible(x, y);
            ShogiPieces[selectedShogiPiece.CurrentX, selectedShogiPiece.CurrentY] = null;
            PlacePiece(selectedShogiPiece, x, y);
            CheckForPromotion();
            EndTurn();
        }
        // BoardHighlights.Instance.HideHighlights();
        selectedShogiPiece = null;
    }
    protected virtual void SelectCapturedPiece(int x, int y){
        OnCapturedPieceSelect(x, y);
    }
    protected void OnCapturedPieceSelect(int x, int y){
        (x, y) = currentPlayer.captureBoard.CoordinatesToIndeces(x, y);
        selectedShogiPiece = currentPlayer.captureBoard.capturedPieces[x, y];
        allowedMoves = selectedShogiPiece.PossibleDrops();

        currentPlayer.CalculateAttackedTiles();
        opponentPlayer.CalculateAttackedTiles();
    }

    protected virtual void DropShogiPiece(int x, int y){
        OnShogiPieceDrop(x, y);
    }
    protected virtual void OnShogiPieceDrop(int x, int y){
        if (allowedMoves[x,y]){
            currentPlayer.DropPiece(selectedShogiPiece);
            currentPlayer.AddPieceInPlay(selectedShogiPiece);
            PlacePiece(selectedShogiPiece, x, y);
            EndTurn();
        }
        selectedShogiPiece = null;
        // BoardHighlights.Instance.HideHighlights();
    }
    protected void CapturePieceIfPossible(int x, int y){
        ShogiPiece targetPiece = ShogiPieces[x,y];
        if (targetPiece != null && (targetPiece.player != currentPlayer.playerNumber || freeMove)){
            //activePieceObjects.Remove(targetPiece.gameObject);
            opponentPlayer.RemovePieceInPlay(targetPiece);
            currentPlayer.CapturePiece(targetPiece);
            ShogiPieces[x, y] = null;
        }
    }
    protected void PlacePiece(ShogiPiece piece, int x, int y){
        piece.transform.position = GetTileCenter (x, y);
        piece.SetXY(x, y);
        piece.SetHeight();
        ShogiPieces[x, y] = piece;

        currentPlayer.CalculateAttackedTiles();
        opponentPlayer.CalculateAttackedTiles();
    }
    protected void EndTurn(){
        selectedShogiPiece = null;
        // Check win condition
        if (CheckIfGameOver()){
            EndGame();
        }
        else if (!freeMove){
            ChangePlayer();
        }
    }
    protected void ChangePlayer(){
        if (currentPlayer == player1){
            currentPlayer = player2;
            opponentPlayer = player1;
        }
        else{
            currentPlayer = player1;
            opponentPlayer = player2;
        }
    }
    public bool CheckIfGameOver(){
        if (currentPlayer.isAttackingKing){
            if (!opponentPlayer.hasPossibleMoves) return true;
        }
        return false;
    }
    protected void EndGame(){
        Debug.Log("Victory for " + currentPlayer.playerNumber);
        // Lock The game from being played further
        GameUI.Instance.ShowEndScreen(currentPlayer.playerNumber);
    }
    public void ResetGame(){
        DestroyAllPieces();
        InitializeGame();
        BoardHighlights.Instance.HideAllHighlights();
    }
    protected void DestroyAllPieces(){
        player1.captureBoard.DestroyAllPieces();
        player2.captureBoard.DestroyAllPieces();
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (ShogiPieces[x, y]){
                    Destroy (ShogiPieces[x, y].gameObject);
                }
            }
        BoardHighlights.Instance.HideCheckHighlight();
    }
    protected virtual void CheckForPromotion(){
        // If the game is over before the promotion then just end the game
        if (CheckIfGameOver()){
            return;
        }

        // Give the option to promote a piece when it enters the promotion zone
        // If a piece would have no more legal moves without promoting promote automatically
        // The promotion zone is the last three rows for a player
        if (!selectedShogiPiece.isPromoted && !freeMove){
            if (currentPlayer == player1){
                if (selectedShogiPiece.CurrentY >= C.numberRows - 3)
                    selectedShogiPiece.CheckForPromotion();
            }
            else{
                if (selectedShogiPiece.CurrentY <= 2)
                    selectedShogiPiece.CheckForPromotion();
            }
        }
    }
    public virtual void PromotePiece(ShogiPiece piece){
        OnPiecePromote(piece.CurrentX, piece.CurrentY);
    }
    public void OnPiecePromote(int x, int y){
        ShogiPieces[x, y].Promote();

        currentPlayer.CalculateAttackedTiles();
        opponentPlayer.CalculateAttackedTiles();
        if (CheckIfGameOver()){
            EndGame();
        }
    }
}
