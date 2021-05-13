using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    //public static NetworkManager Instance {set; get;}
    private bool connectionAttempted = false;
    private void Start(){
        //Instance = this;
    }
    private void Awake(){
        //PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Update(){
        if (connectionAttempted){
            GameUI.Instance.SetNetworkText(PhotonNetwork.NetworkClientState.ToString());
        }
    }
    public void Connect(){
        connectionAttempted = true;
        if (PhotonNetwork.IsConnected){
            PhotonNetwork.JoinRandomRoom();
        }
        else {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogError("Connected to the server looking for random match");
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError($"Joining random room failed - {message}. Creating new room");
        PhotonNetwork.CreateRoom(null, new RoomOptions{
            MaxPlayers = 2,
        });
    }
    public override void OnJoinedRoom()
    {
        Debug.LogError($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined room");
        GameController.Instance.TryToStartGame();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogError($"Player {newPlayer.ActorNumber} joined room");
    }
    public bool IsRoomFull(){
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }
}
