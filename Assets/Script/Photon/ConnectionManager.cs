using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviourPunCallbacks
{

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public virtual void OnConnected()
    {
        base.OnConnected();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);
    }

    public override void OnConnectedToMaster() // 포톤 서버 (마스터 서버)에 접속
    {
        base.OnConnectedToMaster();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        // 로비 진입 요청
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() // 로비 접속
    {
        base.OnJoinedLobby();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        SceneManager.LoadScene("Multi");
    }

    public override void OnLeftLobby() // 로비 퇴장
    {
        base.OnLeftLobby();
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);    

        SceneManager.LoadScene("MainMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        print(System.Reflection.MethodBase.GetCurrentMethod().Name);

        foreach (RoomInfo r in roomList)
        {
            print("Room Name: " + r.Name + " Player Count: " + r.PlayerCount + "/" + r.MaxPlayers); 
            
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
