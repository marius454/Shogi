using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private bool connectionAttempted = false;
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
    public void Disconnect(){
        if (PhotonNetwork.IsConnected){
            PhotonNetwork.Disconnect();
        }
    }
    public bool IsRoomFull(){
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
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
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("You disconnected from the network");
        if (BoardManager.Instance)
            BoardManager.Instance.EndGame("Defeat by resignation");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogError("The other player left the room");
        if (BoardManager.Instance)
            BoardManager.Instance.EndGame("Victory by resignation");
    }
}
