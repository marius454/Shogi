using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance{set;get;}
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject promotionMenu;
    [SerializeField] private GameObject ingameUI;
    private ShogiPiece tempPiece;
    private void Start(){
        Instance = this;
    }
    public void ShowEndScreen(PlayerNumber player){
        gameOverMenu.SetActive(true);
        ingameUI.SetActive(false);
        GameObject resultText = gameOverMenu.transform.Find("ResultText").gameObject;
        resultText.GetComponent<TextMeshProUGUI>().text = "Victory for " + player;
    }
    public void ShowPromotionMenu(ShogiPiece piece){
        promotionMenu.SetActive(true);
        GameObject promotionText = promotionMenu.transform.Find("PromotionText").gameObject;
        promotionText.GetComponent<TextMeshProUGUI>().text = "Do you wish to promote this " + piece.GetType() + "?";
        
        tempPiece = piece;
    }
    public void ResetGame(){
        BoardManager.Instance.ResetGame();
        if (gameOverMenu.activeSelf){
            gameOverMenu.SetActive(false);
        }
        if (!ingameUI.activeSelf){
            ingameUI.SetActive(true);
        }
        HidePromotionMenu();
    }
    public void PromotePiece(){
        BoardManager.Instance.PromotePiece(tempPiece);
        HidePromotionMenu();
        tempPiece = null;
    }

    public void DontPromotePiece(){
        HidePromotionMenu();
        tempPiece = null;
    }
    private void HidePromotionMenu(){
        if (promotionMenu.activeSelf){
            promotionMenu.SetActive(false);
        }
    }
}
