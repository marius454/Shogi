using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance {set; get;}
    [SerializeField] private GameObject singleplayerBoard;
    [SerializeField] private GameObject multiplayerBoard;
    public bool gameStarted{set; get;}
    private void Start()
    {
        // Make game cap out at 60 fps for less stress on the computer
        Application.targetFrameRate = 60;
        Instance = this;
        gameStarted = false;
    }
    public void StartSingleplayerGame(){
        singleplayerBoard.SetActive(true);
        multiplayerBoard.SetActive(false);
        gameStarted = true;
        SPBoardManager.Instance.InitializeGame();
    }
    public void StartMultiplayerGame(){
        multiplayerBoard.SetActive(true);
        singleplayerBoard.SetActive(false);
        gameStarted = true;
        MPBoardManager.Instance.InitializeGame();
    }
    public void StartAIGame(){
        // TO DO
    }
    public void QuitGame(){
        // TO DO
    }
    public void QuitMatch(){
        singleplayerBoard.SetActive(true);
        multiplayerBoard.SetActive(false);
        gameStarted = false;
    }
}
