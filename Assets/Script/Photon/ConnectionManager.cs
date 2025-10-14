using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public GameObject MainMenuUI;
    public GameObject RoomListPanel; // 방 목록을 담고 있는 UI 패널
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
        Debug.Log("<color=green>OnConnectedToMaster:</color> 포톤 마스터 서버에 접속했습니다.");

        // 로비 진입 요청
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby() // 로비 접속
    {
        base.OnJoinedLobby();
        Debug.Log("<color=green>OnJoinedLobby:</color> 로비에 접속했습니다. 이제 방 목록을 받을 수 있습니다.");
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
    
    // RoomInfo 객체를 캐싱하여 방 목록을 관리합니다.
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    // 생성된 UI 아이템(프리팹)을 관리합니다.
    private Dictionary<string, GameObject> roomListEntries = new Dictionary<string, GameObject>();

    /// <summary>
    /// '방 목록 보기' 버튼에 연결할 함수입니다.
    /// </summary>
    public void OnRoomListButtonClicked()
    {
        Debug.Log("<color=yellow>UI Event:</color> '방 목록 보기' 버튼이 클릭되었습니다.");
        if (RoomListPanel != null) RoomListPanel.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"<color=cyan>OnRoomListUpdate:</color> 방 목록 업데이트 수신. {roomList.Count}개의 변경사항 감지.");
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

        // 변경된 방 목록 정보를 캐시에 업데이트합니다.
        UpdateCachedRoomList(roomList);
        // 캐시된 정보를 바탕으로 UI를 업데이트합니다.
        UpdateRoomListView();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // 목록에서 제거되었거나, 닫혔거나, 보이지 않는 방은 캐시에서 삭제합니다.
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
                continue;
            }

            // 캐시된 방 정보를 갱신하거나 새로 추가합니다.
            cachedRoomList[info.Name] = info;
        }
    }

    private void UpdateRoomListView()
    {
        // 기존에 생성된 UI 리스트를 모두 삭제합니다.
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();

        // 캐시된 방 목록을 기반으로 새로운 UI 리스트를 생성합니다.
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            Debug.Log($"<color=lightblue>UpdateRoomListView:</color> '{info.Name}' 방 UI 생성 중...");

            GameObject entry = Instantiate(roomListItemPrefab, roomListContent);
            entry.SetActive(true);
            
            RoomListItem listItem = entry.GetComponent<RoomListItem>();
            if (listItem != null)
            {
                listItem.SetRoomInfo(info);
            }
            else
            {
                Debug.LogError("'RoomListItem' 프리팹의 최상단에 RoomListItem.cs 스크립트가 없습니다.");
            }

            roomListEntries.Add(info.Name, entry);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("<color=green>OnJoinedRoom:</color> 방에 입장했습니다. 방 목록 UI를 숨깁니다.");
        // 방에 입장하면 캐시와 UI 목록을 초기화합니다.
        cachedRoomList.Clear();
        // 방 목록 패널을 비활성화합니다.
        if (RoomListPanel != null) RoomListPanel.SetActive(false);
    }
}
