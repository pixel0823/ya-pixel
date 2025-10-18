using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public GameObject MainMenuUI;
    public GameObject MultiUI;
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public TMP_Text maxPlayerText;
    

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
    }

    public override void OnLeftLobby() // 로비 퇴장
    {
        base.OnLeftLobby();
        print("Left Lobby");

        if (MainMenuUI != null) MainMenuUI.SetActive(true);
        if (MultiUI != null) MultiUI.SetActive(false);
    }

    public GameObject roomListItemPrefab;
    public Transform roomListContent;
    private List<GameObject> roomListItems = new List<GameObject>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomListItemPrefab == null)
        {
            Debug.LogError("ConnectionManager: 'Room List Item Prefab'이(가) Inspector에 설정되지 않았습니다.");
            return;
        }
        if (roomListContent == null)
        {
            Debug.LogError("ConnectionManager: 'Room List Content'가 Inspector에 설정되지 않았습니다.");
            return;
        }

        // 기존 방 목록 UI 삭제
        foreach (GameObject item in roomListItems)
        {
            Destroy(item);
        }
        roomListItems.Clear();

        // 새로운 방 목록 UI 생성
        foreach (RoomInfo roomInfo in roomList)
        {
            // 닫혔거나, 보이지 않거나, 삭제된 방은 목록에 표시하지 않음
            if (!roomInfo.IsOpen || !roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                continue;
            }

            GameObject newItem = Instantiate(roomListItemPrefab, roomListContent);
            RoomListItem listItem = newItem.GetComponent<RoomListItem>();
            if (listItem != null)
            {
                listItem.SetRoomInfo(roomInfo);
            }
            else
            {
                Debug.LogError("'RoomListItem' 프리팹의 최상단에 RoomListItem.cs 스크립트가 없습니다.");
            }
            roomListItems.Add(newItem);
        }
    }
}
