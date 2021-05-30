using System;
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
    protected List<ShogiPiece> allPieces{set;get;}
    public List<(ShogiPiece[,] state, int repetitions)> gameStates{set; get;}
    public bool checkForPerpetualCheck{set;get;}

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
            BoardHighlights.Instance.HideMoveHighlights();
        }
    }
    protected void MarkCheckedKing(){
        if (player1.isInCheck){
            if (player1.king)
                BoardHighlights.Instance.HighlightCheck(player1.king.CurrentX, player1.king.CurrentY);
        }
        else if (player2.isInCheck){
            if (player2.king)
                BoardHighlights.Instance.HighlightCheck(player2.king.CurrentX, player2.king.CurrentY);
        }
        else {
            BoardHighlights.Instance.HideCheckHighlight();
        }
    }
    public virtual void InitializeGame(){
        allowedMoves = new bool[C.numberRows, C.numberRows];
        allPieces = new List<ShogiPiece>();
        player1 = new ShogiPlayer(PlayerNumber.Player1, this, GameObject.Find("CaptureBoardPlayer1").GetComponent<CaptureBoard>());
        player2 = new ShogiPlayer(PlayerNumber.Player2, this, GameObject.Find("CaptureBoardPlayer2").GetComponent<CaptureBoard>());
        selectionX = -1;
        selectionY = -1;

        //activePieceObjects = new List<GameObject>();
        ShogiPieces = new ShogiPiece[C.numberRows, C.numberRows];
        gameStates = new List<(ShogiPiece[,] state, int repetitions)>();
        currentPlayer = player1;
        opponentPlayer = player2;

        SpawnAllShogiPieces();
        // SpawnPiece(PieceType.pawn, 4, 4, PlayerNumber.Player1);
        // ShogiPieces[4, 4].Promote();

        // SpawnPiece(PieceType.king, 4, 4, PlayerNumber.Player2);
        // SpawnPiece(PieceType.knight, 3, 2, PlayerNumber.Player1);
        // SpawnPiece(PieceType.knight, 5, 2, PlayerNumber.Player1);
        // for (int x=0; x < C.numberRows; x++)
        //     for (int y=0; y < C.numberRows; y++){
        //         if (!(x == 4 && y == 4))
        //             SpawnPiece(PieceType.pawn, x, y, PlayerNumber.Player1);
        //             //ShogiPieces[x, y].Promote();
        //     }
        
        player1.InitializePiecesInPlay();
        player2.InitializePiecesInPlay();
        // if (ShogiPieces[4, 4].IsAttacked()){
        //     player2.PlaceInCheck();
        // }
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
            if (selectionX >= 0 && selectionY >= 0 && selectionX < C.numberRows && selectionY < C.numberRows){
                if (!selectedShogiPiece || (ShogiPieces[selectionX, selectionY] && ShogiPieces[selectionX, selectionY].player == currentPlayer.playerNumber && !freeMove)){
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
        if (x >= 0 && y >= 0 && x <= C.numberRows && y <= C.numberRows){
            center.x += (C.tileSize * x) + C.tileOffset;
            center.z += (C.tileSize * y) + C.tileOffset;
            return center;
        }
        else if (x >= player1.captureBoard.minX && y >= player1.captureBoard.minY 
          && x <= player1.captureBoard.maxX && y <= player1.captureBoard.maxY){
              return player1.captureBoard.GetTileCenter(x, y);
        }
        else if (x >= player2.captureBoard.minX && y >= player2.captureBoard.minY 
          && x <= player2.captureBoard.maxX && y <= player2.captureBoard.maxY){
              return player2.captureBoard.GetTileCenter(x, y);
        }
        else return center;
    }
    protected void SpawnPiece(PieceType pieceType, int x, int y, PlayerNumber player){
        GameObject piece = Instantiate(piecePrefabs[(int)pieceType]) as GameObject;
        // piece.transform.SetParent(transform);
        ShogiPieces[x, y] = piece.GetComponent<ShogiPiece>();
        ShogiPieces[x, y].Init(x, y, player, pieceType, this);
        allPieces.Add(ShogiPieces[x, y]);
        ShogiPieces[x, y].SetID(allPieces.Count - 1);
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
    protected virtual void SelectShogiPiece(int x, int y, bool isSimulated = false){
        OnShogiPieceSelect(x, y);
    }
    protected void OnShogiPieceSelect(int x, int y){
        selectedShogiPiece = ShogiPieces[x, y];
        if (!freeMove){
            allowedMoves = selectedShogiPiece.PossibleMoves();
        }
        else{
            Array.Clear(allowedMoves, 0, C.numberRows*C.numberRows);
            for (int i=0; i < C.numberRows*C.numberRows; i++) allowedMoves[i%C.numberRows,i/C.numberRows] = true;
        }
    }
    protected virtual void MoveShogiPiece(int x, int y, bool isSimulated = false){
        OnShogiPieceMove(x, y, isSimulated);
    }
    protected virtual void OnShogiPieceMove(int x, int y, bool isSimulated = false){
        if (allowedMoves[x,y]){
            // Capture piece if possible
            CapturePieceIfPossible(x, y, isSimulated);
            // if (selectedShogiPiece){
            //     Debug.Log("A piece is selected");
            // }
            // else Debug.Log("no piece is selected");
            ShogiPieces[selectedShogiPiece.CurrentX, selectedShogiPiece.CurrentY] = null;
            PlacePiece(selectedShogiPiece, x, y, isSimulated);
            CheckForPromotion(isSimulated);
            // EndTurn(); // Moved to CheckForPromotion() method, so players cannot move while the other player is deciding on wheter to promote his piece
        }
        // BoardHighlights.Instance.HideHighlights();
        selectedShogiPiece = null;
        // Debug.Break();
    }
    protected virtual void SelectCapturedPiece(int x, int y){
        OnCapturedPieceSelect(x, y);
    }
    protected virtual void OnCapturedPieceSelect(int x, int y){
        (x, y) = currentPlayer.captureBoard.CoordinatesToIndeces(x, y);
        selectedShogiPiece = currentPlayer.captureBoard.capturedPieces[x, y];
        allowedMoves = selectedShogiPiece.PossibleDrops();
    }

    protected virtual void DropShogiPiece(int x, int y, bool isSimulated = false){
        OnShogiPieceDrop(x, y, isSimulated);
    }
    protected virtual void OnShogiPieceDrop(int x, int y, bool isSimulated = false){
        if (allowedMoves[x,y]){
            currentPlayer.DropPiece(selectedShogiPiece);
            currentPlayer.AddPieceInPlay(selectedShogiPiece);
            PlacePiece(selectedShogiPiece, x, y, isSimulated);
            EndTurn();
        }
        selectedShogiPiece = null;
        // BoardHighlights.Instance.HideHighlights();
    }
    protected virtual void CapturePieceIfPossible(int x, int y, bool isSimulated = false){
        ShogiPiece targetPiece = ShogiPieces[x,y];
        //if (targetPiece && (targetPiece.player != currentPlayer.playerNumber || freeMove)){
        if (targetPiece){
            //activePieceObjects.Remove(targetPiece.gameObject);
            opponentPlayer.RemovePieceInPlay(targetPiece);
            currentPlayer.CapturePiece(targetPiece, isSimulated);
            ShogiPieces[x, y] = null;
        }
    }
    protected void PlacePiece(ShogiPiece piece, int x, int y, bool isSimulated){
        ShogiPieces[x, y] = piece;
        piece.SetXY(x, y, !isSimulated);
        if (!isSimulated){
            // piece.transform.position = GetTileCenter (x, y);
            // piece.SetHeight();
            BoardHighlights.Instance.HighlightLastMove(x, y);
            currentPlayer.CalculatePossibleMoves();
            // Debug.Log(currentPlayer.playerNumber + " " + currentPlayer.hasPossibleMoves + " " + currentPlayer.nrMoves);
            opponentPlayer.CalculatePossibleMoves();
            // Debug.Log(currentPlayer.playerNumber + " " + currentPlayer.hasPossibleMoves + " " + currentPlayer.nrMoves);
            // Debug.Log(opponentPlayer.playerNumber + " " + opponentPlayer.hasPossibleMoves + " " + opponentPlayer.nrMoves);
        }

        // opponentPlayer.CheckIfKingIsBeingAttacked();
        if (currentPlayer.isAttackingKing) currentPlayer.nrOfChecksInARow ++;
        else currentPlayer.nrOfChecksInARow = 0;
    }
    public void EndTurn(){
        OnTurnEnd();
    }
    public virtual void EndTurnAfterPromotion(){
        OnTurnEnd();
    }
    protected virtual void OnTurnEnd(){
        // Debug.Break();

        // player1.CalculatePossibleMoves();
        // // SimulatedBoard simBoard = new SimulatedBoard(this);
        // // List<Move> simulatedMoves = simBoard.GetAllMoves(player1.playerNumber);
        selectedShogiPiece = null;
        // Check win condition
        if (CheckIfGameOver()){
            EndGame();
        }
        else if (CheckForRepetitionDraw()){
            EndGame("Repetition Draw");
        }
        else if (!freeMove){
            ChangePlayer();
        }
    }

    protected bool CheckForRepetitionDraw(bool addNewPosition = true)
    {
        for (int i = 0; i < gameStates.Count(); i++){
            bool isEqual = gameStates[i].state.Rank == ShogiPieces.Rank &&
                Enumerable.Range(0, gameStates[i].state.Rank).All(dimension => gameStates[i].state.GetLength(dimension) == ShogiPieces.GetLength(dimension)) &&
                gameStates[i].state.Cast<ShogiPiece>().SequenceEqual(ShogiPieces.Cast<ShogiPiece>());
            if (isEqual){
                gameStates[i] = (ShogiPieces.Clone() as ShogiPiece[,], gameStates[i].repetitions + 1);
                if (gameStates[i].repetitions >= 4) return true;
                else return false;
            } 
        }
        if (addNewPosition){
            gameStates.Add((ShogiPieces.Clone() as ShogiPiece[,], 1));
        }
        return false;
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
        if (!opponentPlayer.hasPossibleMoves) return true;
        if (!currentPlayer.hasPossibleMoves) return true;
        return false;
    }
    public virtual void EndGame(string message = "placeholder"){
        Debug.Log("Victory for " + currentPlayer.playerNumber);
        // Lock The game from being played further
        if (message != "placeholder"){
            GameUI.Instance.ShowEndScreen(message);
        }
        else{
            if (!opponentPlayer.hasPossibleMoves) GameUI.Instance.ShowEndScreen(currentPlayer.playerNumber);
            if (!currentPlayer.hasPossibleMoves) GameUI.Instance.ShowEndScreen(currentPlayer.playerNumber);
        }
    }
    public virtual void ResetGame(){
        DestroyAllPieces();
        InitializeGame();
    }
    protected void DestroyAllPieces(){
        player1.DestroyAllPieces();
        player2.DestroyAllPieces();
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (ShogiPieces[x, y]){
                    Destroy (ShogiPieces[x, y].gameObject);
                }
            }
        BoardHighlights.Instance.HideAllHighlights();
        ShogiPieces = new ShogiPiece[C.numberRows, C.numberRows];
    }
    protected virtual void CheckForPromotion(bool isSimulated = false){
        // If the game is over before the promotion then just end the game
        if (CheckIfGameOver()){
            EndTurn();
        }

        // Give the option to promote a piece when it enters the promotion zone
        // If a piece would have no more legal moves without promoting promote automatically
        // The promotion zone is the last three rows for a player
        bool willCheckForPromotion = false;
        if (!selectedShogiPiece.isPromoted && !freeMove){
            if (currentPlayer == player1){
                if (selectedShogiPiece.CurrentY >= C.numberRows - 3){
                    if (!(selectedShogiPiece is King) && !(selectedShogiPiece is GoldGeneral))
                        willCheckForPromotion = true;
                    selectedShogiPiece.CheckForPromotion();
                }
            }
            else{
                if (selectedShogiPiece.CurrentY <= 2){
                    if (!(selectedShogiPiece is King) && !(selectedShogiPiece is GoldGeneral))
                        willCheckForPromotion = true;
                    selectedShogiPiece.CheckForPromotion();
                }   
            }
        }
        if (!willCheckForPromotion){
            EndTurn();
        }
    }
    public virtual void PromotePiece(ShogiPiece piece, bool isSimulated = false){
        OnPiecePromote(piece.CurrentX, piece.CurrentY, isSimulated);
    }
    public virtual void OnPiecePromote(int x, int y, bool isSimulated = false){
        ShogiPieces[x, y].Promote(!isSimulated);

        currentPlayer.CalculatePossibleMoves();
        opponentPlayer.CalculatePossibleMoves();
        if (CheckIfGameOver()){
            EndGame();
        }
    }
}
