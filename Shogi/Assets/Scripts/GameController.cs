using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    private PhotonView photonView{set; get;}
    public static GameController Instance {set; get;}
    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject singleplayerBoard;
    [SerializeField] private GameObject multiplayerBoard;
    [SerializeField] private GameObject AIBoard;
    [SerializeField] private GameObject singleplayerCamera;
    [SerializeField] private GameObject player1Camera;
    [SerializeField] private GameObject player2Camera;

    public bool gameStarted{set; get;}
    public bool tryToStartGame{set; get;}
    private void Start()
    {
        // Make game cap out at 60 fps for less stress on the computer
        Application.targetFrameRate = 5;
        Instance = this;
        gameStarted = false;
        singleplayerCamera.GetComponent<Camera>().enabled = true;
        photonView = GetComponent<PhotonView>();
    }
    private void Update(){
        if (tryToStartGame){
            if (networkManager.GetComponent<NetworkManager>().IsRoomFull())
                StartMultiplayerGame();
        }    
    }
    public void TryToStartGame(){
        tryToStartGame = true;
    }
    public void StartSingleplayerGame(){
        singleplayerBoard.SetActive(true);
        //multiplayerBoard.SetActive(false);
        // SPBoardManager.Instance.InitializeGame();
        singleplayerBoard.transform.Find("ShogiBoard").GetComponent<SPBoardManager>().InitializeGame();
        gameStarted = true;
        SetupSinglePlayerCamera();
    }
    // public void StartMultiplayerGame(){
    //     photonView.RPC(nameof(RPC_InitializeMultiplayerGame), RpcTarget.AllBuffered);
    // }
    public void StartMultiplayerGame(){
        multiplayerBoard.SetActive(true);
        singleplayerBoard.SetActive(false);
        gameStarted = true;
        tryToStartGame = false;
        SetupMultiplayerCamera();
    }
    private void SetupSinglePlayerCamera(){
        player1Camera.SetActive(false);
        player2Camera.SetActive(false);
        singleplayerCamera.SetActive(true);
    }
    private void SetupMultiplayerCamera(){
        singleplayerCamera.SetActive(false);
        player1Camera.SetActive(false);
        player2Camera.SetActive(false);
        multiplayerBoard.transform.Find("ShogiBoard").GetComponent<MPBoardManager>().SetupCamera(player1Camera, player2Camera);
        //MPBoardManager.Instance.SetupCamera(player1Camera, player2Camera);
    }
    public void StartAIGame(){
        AIBoard.SetActive(true);
        singleplayerBoard.SetActive(false);
        AIBoard.transform.Find("ShogiBoard").GetComponent<AIBoardManager>().InitializeGame();
        gameStarted = true;
        SetupSinglePlayerCamera();
    }
    public void QuitMatch(){
        singleplayerBoard.SetActive(true);
        multiplayerBoard.SetActive(false);
        gameStarted = false;
    }
}
