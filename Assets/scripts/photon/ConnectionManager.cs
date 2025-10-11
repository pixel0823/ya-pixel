using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public GameObject MainMenuUI;
    public GameObject MultiUI;

    void Start()
    {
        // We will connect on button click, not on start
        // PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        if (MainMenuUI != null) MainMenuUI.SetActive(false);
        if (MultiUI != null) MultiUI.SetActive(true);
        
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() // 포톤 서버 (마스터 서버)에 접속
    {
        base.OnConnectedToMaster();
        print("Connected to Master Server");

        // 로비 진입 요청
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() // 로비 접속
    {
        base.OnJoinedLobby();
        print("Joined Lobby");

        SceneManager.LoadScene("Multi");
    }

    public override void OnLeftLobby() // 로비 퇴장
    {
        base.OnLeftLobby();
        print("Left Lobby");    

        SceneManager.LoadScene("MainMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        print("Room list updated");

        foreach (RoomInfo r in roomList)
        {
            print("Room Name: " + r.Name + " Player Count: " + r.PlayerCount + "/" + r.MaxPlayers); 
            
        }
    }
}
