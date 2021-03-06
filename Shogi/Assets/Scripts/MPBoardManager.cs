using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.EventSystems;
using C = Constants;

public class MPBoardManager : BoardManager
{
    private PhotonView photonView;
    public ShogiPlayer localPlayer { set; get; }
    public ShogiPiece pieceSelectedByPlayer { set; get; }
    public bool[,] possibleMovesForSelectedPiece { set; get; }
    public bool player1Impasse;
    public bool player2Impasse;
    private void Awake(){
        checkForPerpetualCheck = true;
        possibleMovesForSelectedPiece = new bool[C.numberRows, C.numberRows];
        photonView = GetComponent<PhotonView>();
        player1Impasse = false;
        player2Impasse = false;
        InitializeGame();
        if (PhotonNetwork.LocalPlayer.ActorNumber == 1) this.localPlayer = player1;
        else this.localPlayer = player2;
        Instance = this;
        // this.gameObject.transform.parent.transform.Find("CaptureBoardPlayer1").GetComponent<MPCaptureBoard>().InitializeCaptureBoard();
        // this.gameObject.transform.parent.transform.Find("CaptureBoardPlayer2").GetComponent<MPCaptureBoard>().InitializeCaptureBoard();
    }
    private void Update(){
        if (GameController.Instance.gameStarted && this.gameObject.activeSelf){
            UpdateSelection();
            MarkSelectedPiece();
            MarkAllowedMoves();
            MarkCheckedKing();
            ComputeMouseClick();
        }
        if (player1Impasse && player2Impasse){
            CalculateImpasse();
        }
        if (player1.king.CurrentY >= C.numberRows - 3 && player2.king.CurrentY <= 2){
            if (localPlayer.playerNumber == PlayerNumber.Player1){
                if (player1Impasse) localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().HideImpasseButton();
                else localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowImpasseButton();
            }
            else {
                if (player2Impasse) localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().HideImpasseButton();
                else localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowImpasseButton();
            }
        }
        else {
            localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().HideImpasseButton();
        }
    }
    protected override void MarkSelectedPiece()
    {
        if (pieceSelectedByPlayer){
            BoardHighlights.Instance.HighlightSelection(pieceSelectedByPlayer.CurrentX, pieceSelectedByPlayer.CurrentY);
        }
        else {
            BoardHighlights.Instance.HideSelectionHighlight();
        }
    }
    protected override void MarkAllowedMoves()
    {
        if (pieceSelectedByPlayer){
            BoardHighlights.Instance.HighlightAllowedMoves(possibleMovesForSelectedPiece);
        }
        else {
            BoardHighlights.Instance.HideMoveHighlights();
        }
    }
    public void SetupCamera(GameObject camera1, GameObject camera2){
        if (localPlayer.playerNumber == PlayerNumber.Player1) localPlayer.SetupCamera(camera1);
        else localPlayer.SetupCamera(camera2);

        localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowIngameUI();
    }
    protected override void ComputeMouseClick(){
        if (Input.GetMouseButtonDown (0) && !EventSystem.current.IsPointerOverGameObject()){
            if (selectionX >= 0 && selectionY >= 0 && selectionX <= C.numberRows && selectionY <= C.numberRows){
                if (!pieceSelectedByPlayer || (ShogiPieces[selectionX, selectionY] && ShogiPieces[selectionX, selectionY].player == localPlayer.playerNumber && !freeMove)){
                    SelectShogiPiece(selectionX, selectionY);
                }else{
                    if (localPlayer.capturedPieces.Contains(pieceSelectedByPlayer)){
                        DropShogiPiece(selectionX, selectionY);
                    }
                    else{
                        MoveShogiPiece(selectionX, selectionY);
                    }
                }
            }
            else if (selectionX >= localPlayer.captureBoard.minX && selectionY >= localPlayer.captureBoard.minY 
              && selectionX <= localPlayer.captureBoard.maxX && selectionY <= localPlayer.captureBoard.maxY)
            {
                if (localPlayer.captureBoard.ExistsPiece(selectionX, selectionY)){
                    SelectCapturedPiece(selectionX, selectionY); // still needs to be changed with RPC
                }
            }
        }
    }
    protected override void SelectShogiPiece(int x, int y, bool isSimulated = false){
        if (!ShogiPieces[x, y])
            return;
        if (ShogiPieces[x, y].player != localPlayer.playerNumber)
            return;

        pieceSelectedByPlayer = ShogiPieces[x, y];
        possibleMovesForSelectedPiece = pieceSelectedByPlayer.PossibleMoves();
        // Debug.LogError("Piece Selected " + pieceSelectedByPlayer);

        if (currentPlayer != localPlayer) return;
        photonView.RPC(nameof(RPC_SelectShogiPiece), RpcTarget.AllBuffered, new object[] { x, y });
    }
    [PunRPC]
    private void RPC_SelectShogiPiece(int x, int y)
    {
        OnShogiPieceSelect(x, y);
    }

    protected override void MoveShogiPiece(int x, int y, bool isSimulated = false)
    {
        pieceSelectedByPlayer = null;
        if (currentPlayer != localPlayer) return;
        photonView.RPC(nameof(RPC_MoveShogiPiece), RpcTarget.All, new object[] { x, y });
    }
    [PunRPC]
    private void RPC_MoveShogiPiece(int x, int y)
    {
        OnShogiPieceMove(x, y);
    }
    protected override void OnShogiPieceMove(int x, int y, bool isSimulated = false)
    {
        base.OnShogiPieceMove(x, y);
        if (pieceSelectedByPlayer){
            possibleMovesForSelectedPiece = pieceSelectedByPlayer.PossibleMoves();
            if (currentPlayer == localPlayer){
                if (pieceSelectedByPlayer.CurrentX >= 0 && pieceSelectedByPlayer.CurrentY >= 0 && pieceSelectedByPlayer.CurrentX <= C.numberRows && pieceSelectedByPlayer.CurrentY <= C.numberRows)
                    SelectShogiPiece(pieceSelectedByPlayer.CurrentX, pieceSelectedByPlayer.CurrentY);
                else if (pieceSelectedByPlayer.CurrentX >= localPlayer.captureBoard.minX && pieceSelectedByPlayer.CurrentY >= localPlayer.captureBoard.minY 
                    && pieceSelectedByPlayer.CurrentX <= localPlayer.captureBoard.maxX && pieceSelectedByPlayer.CurrentY <= localPlayer.captureBoard.maxY)
                    SelectCapturedPiece(pieceSelectedByPlayer.CurrentX, pieceSelectedByPlayer.CurrentY);
            }
        }
        // Debug.Log("current player - " + currentPlayer.playerNumber + " local player - " + localPlayer.playerNumber);
    }
    protected override void SelectCapturedPiece(int x, int y){
        int i, j;
        (i, j) = localPlayer.captureBoard.CoordinatesToIndeces(x, y);
        if (!localPlayer.captureBoard.capturedPieces[i, j])
            return;
        if (localPlayer.captureBoard.capturedPieces[i, j].player != localPlayer.playerNumber)
            return;

        pieceSelectedByPlayer = localPlayer.captureBoard.capturedPieces[i, j];
        if (localPlayer == currentPlayer)
            possibleMovesForSelectedPiece = pieceSelectedByPlayer.PossibleDrops();
        else possibleMovesForSelectedPiece = pieceSelectedByPlayer.PossibleDrops(false);
        // Debug.LogError("Piece Selected " + pieceSelectedByPlayer);

        if (currentPlayer != localPlayer) return;
        photonView.RPC(nameof(RPC_SelectCapturedPiece), RpcTarget.AllBuffered, new object[] { x, y });
    }
    [PunRPC]
    private void RPC_SelectCapturedPiece(int x, int y)
    {
        OnCapturedPieceSelect(x, y);
    }
    protected override void OnCapturedPieceSelect(int x, int y)
    {
        base.OnCapturedPieceSelect(x, y);

        // player1.CheckIfKingIsBeingAttacked();
        // player2.CheckIfKingIsBeingAttacked();
    }

    protected override void DropShogiPiece(int x, int y, bool isSimulated = false)
    {
        pieceSelectedByPlayer = null;
        if (currentPlayer != localPlayer) return;
        photonView.RPC(nameof(RPC_DropShogiPiece), RpcTarget.AllBuffered, new object[] { x, y });
    }
    [PunRPC]
    private void RPC_DropShogiPiece(int x, int y)
    {
        OnShogiPieceDrop(x, y);
    }
    protected override void OnShogiPieceDrop(int x, int y, bool isSimulated = false)
    {
        base.OnShogiPieceDrop(x, y);
        if (pieceSelectedByPlayer){
            possibleMovesForSelectedPiece = pieceSelectedByPlayer.PossibleDrops();
            if (currentPlayer == localPlayer){
                if (pieceSelectedByPlayer.CurrentX >= 0 && pieceSelectedByPlayer.CurrentY >= 0 && pieceSelectedByPlayer.CurrentX <= C.numberRows && pieceSelectedByPlayer.CurrentY <= C.numberRows)
                    SelectShogiPiece(pieceSelectedByPlayer.CurrentX, pieceSelectedByPlayer.CurrentY);
                else if (pieceSelectedByPlayer.CurrentX >= localPlayer.captureBoard.minX && pieceSelectedByPlayer.CurrentY >= localPlayer.captureBoard.minY 
                    && pieceSelectedByPlayer.CurrentX <= localPlayer.captureBoard.maxX && pieceSelectedByPlayer.CurrentY <= localPlayer.captureBoard.maxY)
                    SelectCapturedPiece(pieceSelectedByPlayer.CurrentX, pieceSelectedByPlayer.CurrentY);
            }
        }
    }
    protected override void CheckForPromotion(bool isSimulated = false){
        if (CheckIfGameOver()){
            return;
        }

        bool willCheckForPromotion = false;
        if (!selectedShogiPiece.isPromoted){
            if (currentPlayer == player1){
                if (selectedShogiPiece.CurrentY >= C.numberRows - 3){
                    if (!(selectedShogiPiece is King) && !(selectedShogiPiece is GoldGeneral))
                        willCheckForPromotion = true;
                    if (selectedShogiPiece.player == localPlayer.playerNumber)
                        selectedShogiPiece.CheckForPromotion();
                    else if (selectedShogiPiece.pieceType == PieceType.king && selectedShogiPiece.pieceType == PieceType.gold)
                        localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowOpponentPromotionMenu(selectedShogiPiece);
                }
            }
            else{
                if (selectedShogiPiece.CurrentY <= 2){
                    if (!(selectedShogiPiece is King) && !(selectedShogiPiece is GoldGeneral))
                        willCheckForPromotion = true;
                    if (selectedShogiPiece.player == localPlayer.playerNumber)
                        selectedShogiPiece.CheckForPromotion();
                    else if (selectedShogiPiece.pieceType == PieceType.king && selectedShogiPiece.pieceType == PieceType.gold) 
                        localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowOpponentPromotionMenu(selectedShogiPiece);
                }
            }
        }
        if (!willCheckForPromotion){
            EndTurn();
        }
    }
    public override void PromotePiece(ShogiPiece piece, bool isSimulated)
    {
        int x, y;
        x = piece.CurrentX;
        y = piece.CurrentY;
        photonView.RPC(nameof(RPC_PromoteShogiPiece), RpcTarget.AllBuffered, new object[] { x, y });
    }
    [PunRPC]
    private void RPC_PromoteShogiPiece(int x, int y)
    {
        OnPiecePromote(x, y);
    }

    public override void EndGame(string message = "placeholder")
    {
        Debug.Log("Victory for " + currentPlayer.playerNumber);
        // Lock The game from being played further
        string endMessage;
        
        if (message != "placeholder"){
            endMessage = message;
        }
        else {
            if (localPlayer.hasPossibleMoves) endMessage = "Victory";
            else endMessage = "Defeat";
        }
        localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().ShowEndScreen(endMessage);
    }

    public override void EndTurnAfterPromotion()
    {
        photonView.RPC(nameof(RPC_EndTurnAfterPromotion), RpcTarget.AllBuffered, new object[] {});
    }
    [PunRPC]
    private void RPC_EndTurnAfterPromotion(){
        localPlayer.playerCamera.transform.Find("UI").GetComponent<GameUI>().HidePromotionMenu();
        OnTurnEnd();
    }
    protected override void OnTurnEnd()
    {
        player1Impasse = false;
        player2Impasse = false;
        base.OnTurnEnd();
    }
    public void AskForImpasse(){
        PlayerNumber player;
        if (localPlayer.playerNumber == PlayerNumber.Player1) player = PlayerNumber.Player1;
        else player = PlayerNumber.Player2;
        photonView.RPC(nameof(RPC_AskForImpasse), RpcTarget.AllBuffered, new object[] { player });
    }
    [PunRPC]
    private void RPC_AskForImpasse(PlayerNumber player)
    {
        if (player == PlayerNumber.Player1) player1Impasse = true;
        else player2Impasse = true;
    }
    private void CalculateImpasse(){
        int sumPieces = 0;
        foreach (ShogiPiece piece in localPlayer.allPieces){
            if (piece is King) continue;
            else if (piece is Rook || piece is Bishop){
                sumPieces += 5;
            }
            else{
                sumPieces += 1;
            }
        }
        if (sumPieces > 30) EndGame("Victory by impasse");
        else if (sumPieces > 24) EndGame("Draw by impasse");
        else EndGame("Defeat by impasse");
    }
}
