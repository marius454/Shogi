using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndScreen : MonoBehaviour
{
    public static GameEndScreen Instance{set;get;}
    [SerializeField] private GameObject menu;
    private void Start(){
        Instance = this;
    }
    public void ShowEndScreen(PlayerNumber player){
        menu.SetActive(true);
        GameObject resultText = menu.transform.Find("ResultText").gameObject;
        resultText.GetComponent<TextMeshProUGUI>().text = "Victory for " + player;
    }
    public void resetGame(){
        BoardManager.Instance.resetGame();
        menu.SetActive(false);
    }
}
