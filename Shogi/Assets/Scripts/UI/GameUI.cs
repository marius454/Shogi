using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance{set;get;}
    private void Awake(){
        ShowMainMenu();
        Instance = this;
    }

    #region Main Menu
    [SerializeField] private GameObject mainMenu;
    public void ShowMainMenu(){
        if (mainMenu)
            if (!mainMenu.activeSelf){
                mainMenu.SetActive(true);
            }
    }
    private void HideMainMenu(){
        if (mainMenu)
            if (mainMenu.activeSelf){
                mainMenu.SetActive(false);
            } 
    }
    public void SelectSingleplayerGame(){
        HideMainMenu();
        ShowIngameUI();
        GameController.Instance.StartSingleplayerGame();
    }
    public void SelectMultiplayerGame(){
        HideMainMenu();
        ShowNetworkConnectionMenu();
    }
    public void SelectVsAIGame(){
        HideMainMenu();
        ShowAIMenu();
    }
    public void QuitGame(){
        Application.Quit();
    }

    #endregion

    #region Game Over Menu
    [SerializeField] private GameObject gameOverMenu;
    public void ShowEndScreen(PlayerNumber player){
        if (gameOverMenu){
            gameOverMenu.SetActive(true);
            HideIngameUI();
            GameObject resultText = gameOverMenu.transform.Find("ResultText").gameObject;
            resultText.GetComponent<TextMeshProUGUI>().text = "Victory for " + player;
        }
    }
    public void ShowEndScreen(string message){
        if (gameOverMenu){
            gameOverMenu.SetActive(true);
            HideIngameUI();
            GameObject resultText = gameOverMenu.transform.Find("ResultText").gameObject;
            resultText.GetComponent<TextMeshProUGUI>().text = message;
        }
    }
    private void HideGameOverMenu(){
        if (gameOverMenu)
            if (gameOverMenu.activeSelf){
                gameOverMenu.SetActive(false);
            }
    }
    #endregion

    #region Promotion Menu Menu
    [SerializeField] private GameObject promotionMenu;
    private ShogiPiece tempPiece;
    public void ShowPromotionMenu(ShogiPiece piece){
        if (promotionMenu){
            promotionMenu.SetActive(true);
            GameObject promotionText = promotionMenu.transform.Find("PromotionText").gameObject;
            promotionText.GetComponent<TextMeshProUGUI>().text = "Do you wish to promote this " + piece.GetType() + "?";
            tempPiece = piece;
        }
    }
    public void ShowOpponentPromotionMenu(ShogiPiece piece){
        if (promotionMenu){
            promotionMenu.SetActive(true);
            promotionMenu.GetComponent<Image>().enabled = false;
            GameObject promotionText = promotionMenu.transform.Find("PromotionText").gameObject;
            promotionText.GetComponent<TextMeshProUGUI>().text = "Opponent is deciding whether to promote their " + piece.GetType();
            promotionMenu.transform.Find("PromoteButton").gameObject.SetActive(false);
            promotionMenu.transform.Find("DontPromoteButton").gameObject.SetActive(false);
        }
    }
    public void HidePromotionMenu(){
        if (promotionMenu)
            if (promotionMenu.activeSelf){
                promotionMenu.GetComponent<Image>().enabled = true;
                if (promotionMenu.transform.Find("PromoteButton").gameObject.activeSelf)
                    promotionMenu.transform.Find("PromoteButton").gameObject.SetActive(true);
                if (promotionMenu.transform.Find("DontPromoteButton").gameObject.activeSelf)
                    promotionMenu.transform.Find("DontPromoteButton").gameObject.SetActive(true);
                promotionMenu.SetActive(false);
            }
    }
    public void PromotePiece(){
        BoardManager.Instance.PromotePiece(tempPiece);
        HidePromotionMenu();
        tempPiece = null;
        BoardManager.Instance.EndTurnAfterPromotion();
    }
    public void DontPromotePiece(){
        HidePromotionMenu();
        tempPiece = null;
        BoardManager.Instance.EndTurnAfterPromotion();
    }

    #endregion

    #region IngameUI
    [SerializeField] private GameObject ingameUI;
    public void ShowIngameUI(){
        if (ingameUI){
            HideAllUI();
            ingameUI.SetActive(true);
        }
    }
    private void HideIngameUI(){
        if (ingameUI)
            if (ingameUI.activeSelf){
                ingameUI.SetActive(false);
            } 
    }
    public void ShowImpasseButton(){
        if (ingameUI){
            if (!ingameUI.transform.Find("ImpasseButton").gameObject.activeSelf){
                ingameUI.transform.Find("ImpasseButton").gameObject.SetActive(true);
            }
        }
    }
    public void HideImpasseButton(){
        if (ingameUI){
            if (ingameUI.transform.Find("ImpasseButton").gameObject.activeSelf){
                ingameUI.transform.Find("ImpasseButton").gameObject.SetActive(false);
            }
        }
    }
    public void AskForImpasse(){
        MPBoardManager board = BoardManager.Instance as MPBoardManager;
        board.AskForImpasse();
    }
    #endregion
    
    #region NetworkConnectionMenu
    [SerializeField] private GameObject networkMenu;
    [SerializeField] private NetworkManager networkManager;
    public void ShowNetworkConnectionMenu(){
        if (networkMenu)
            if (!networkMenu.activeSelf){
                networkMenu.SetActive(true);
            }
    }
    private void HideNetworkConnectionMenu(){
        if (networkMenu)
            if (networkMenu.activeSelf){
                networkMenu.SetActive(false);
            } 
    }
    public void SetNetworkText(string text){
        if (networkMenu){
            GameObject networkText = networkMenu.transform.Find("ConnectionText").gameObject;
            networkText.GetComponent<TextMeshProUGUI>().text = text;
        }
    }
    public void ConnectToRandomRoom(){
        networkManager.Connect();
    }
    public void DisconnectFromMultiplayer(){
        networkManager.Disconnect();
    }
    #endregion

    #region AI Difficulty Selection
    [SerializeField] private GameObject AIMenu;
    public void ShowAIMenu(){
        if (AIMenu)
            if (!AIMenu.activeSelf){
                AIMenu.SetActive(true);
            }
    }
    private void HideAIMenu(){
        if (AIMenu)
            if (AIMenu.activeSelf){
                AIMenu.SetActive(false);
            } 
    }
    public void RandomMoves(){
        HideAIMenu();
        ShowIngameUI();
        GameController.Instance.StartAIGame(0);
    }
    public void MinimaxDepth1(){
        HideAIMenu();
        ShowIngameUI();
        GameController.Instance.StartAIGame(1);
    }
    public void MinimaxDepth2(){
        HideAIMenu();
        ShowIngameUI();
        GameController.Instance.StartAIGame(2);
    }
    public void MinimaxDepth3(){
        HideAIMenu();
        ShowIngameUI();
        GameController.Instance.StartAIGame(3);
    }

    #endregion
    private void HideAllUI(){
        HideMainMenu();
        HideGameOverMenu();
        HidePromotionMenu();
        HideIngameUI();
        HideNetworkConnectionMenu();
    }
    public void ResetGame(){
        BoardManager.Instance.ResetGame();
        HideGameOverMenu();
        ShowIngameUI();
        HidePromotionMenu();
    }
    public void BackToMainMenu(){
        networkManager.Disconnect();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
