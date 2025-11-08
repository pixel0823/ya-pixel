using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// [네트워크 전용] 포톤 서버 연결 및 방 관련 로직을 처리합니다.
/// UI를 직접 제어하지 않고, MainMenuManager에게 상태를 알려주는 역할을 합니다.
/// </summary>
public class ConnectionManager : MonoBehaviourPunCallbacks
{
    public static ConnectionManager Instance { get; private set; }

    [Header("Managers")]
    public MainMenuManager mainMenuManager; // UI 전환을 담당하는 매니저

    [Header("UI Panels")]
    public GameObject RoomListPanel; // 방 목록을 담고 있는 UI 패널
    public GameObject PasswordPanel; // 비밀번호 입력 패널

    [Header("Room List")]
    public GameObject roomListItemPrefab;
    public Transform roomListContent;

    private Dictionary<string, GameObject> roomListEntries = new Dictionary<string, GameObject>();
    private const string SaveKey = "SinglePlayerWorlds";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // 멀티플레이 모드에서 포톤 서버에 연결을 시작합니다.
    public void Connect()
    {
        if (PhotonNetwork.IsConnected) return;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("포톤 마스터 서버에 접속을 시도합니다...");
    }

    public void CreateRoom(string roomName, RoomOptions roomOptions)
    {
        Debug.Log($"ConnectionManager: 방 생성 시도: '{roomName}'");
        bool sent = PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
        if (!sent)
        {
            Debug.LogError("ConnectionManager: PhotonNetwork.CreateRoom failed to send. Client may be in wrong state.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("<color=green>OnConnectedToMaster:</color> 포톤 마스터 서버에 접속했습니다.");
        if (GameModeManager.Instance.CurrentMode == GameMode.Multi)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("<color=green>OnJoinedLobby:</color> 로비에 접속했습니다. 이제 방 목록을 받을 수 있습니다.");
        
        if (mainMenuManager != null)
        {
            mainMenuManager.OnJoinedLobby();
        }
        else
        {
            Debug.LogError("mainMenuManager가 할당되지 않았습니다!");
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("ConnectionManager: 방 생성 성공! 방 이름: " + PhotonNetwork.CurrentRoom.Name);
        // 방장(MasterClient)만 맵을 로드할 책임이 있습니다.
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("방장입니다. 'Map1' 씬을 로드합니다.");
            PhotonNetwork.LoadLevel("Map1");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"ConnectionManager: 방 생성 실패. 코드: {returnCode}, 메시지: {message}");
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log($"방 '{PhotonNetwork.CurrentRoom.Name}'에 성공적으로 참가했습니다. 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // 싱글플레이 월드에 재접속한 경우, 씬을 로드할 필요가 있을 수 있습니다.
        // 멀티플레이에서는 OnCreatedRoom에서 방장이 씬을 로드하므로, 여기서는 중복 동작을 피합니다.
        if (PhotonNetwork.OfflineMode && PhotonNetwork.IsMasterClient)
        {
             Debug.Log("오프라인 모드 방장입니다. 'Map1' 씬을 로드합니다.");
             PhotonNetwork.LoadLevel("Map1");
        }
    }

    public void RefreshRoomList()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();

        if (GameModeManager.Instance.CurrentMode == GameMode.Single)
        {
            PopulateForSinglePlayer();
        }
    }

    private void PopulateForSinglePlayer()
    {
        Debug.Log("싱글플레이 모드: 저장된 월드 목록을 불러옵니다.");
        List<string> worlds = GetSavedWorlds();
        foreach (string worldName in worlds)
        {
            GameObject entry = Instantiate(roomListItemPrefab, roomListContent);
            entry.SetActive(true);
            RoomListItem listItem = entry.GetComponent<RoomListItem>();
            if (listItem != null)
            {
                listItem.SetupForSingleplayer(worldName, this);
            }
            roomListEntries.Add(worldName, entry);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (GameModeManager.Instance.CurrentMode != GameMode.Multi) return;
        Debug.Log($"<color=cyan>OnRoomListUpdate:</color> 방 목록 업데이트 수신.");

        foreach (var entry in roomListEntries)
        {
            entry.Value.SetActive(false);
        }

        foreach (RoomInfo info in roomList)
        {
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (roomListEntries.ContainsKey(info.Name))
                {
                    Destroy(roomListEntries[info.Name]);
                    roomListEntries.Remove(info.Name);
                }
                continue;
            }

            if (roomListEntries.ContainsKey(info.Name))
            {
                roomListEntries[info.Name].SetActive(true);
                RoomListItem listItem = roomListEntries[info.Name].GetComponent<RoomListItem>();
                listItem.SetupForMultiplayer(info, this);
            }
            else
            {
                GameObject entry = Instantiate(roomListItemPrefab, roomListContent);
                entry.SetActive(true);
                RoomListItem listItem = entry.GetComponent<RoomListItem>();
                if (listItem != null)
                {
                    listItem.SetupForMultiplayer(info, this);
                }
                roomListEntries.Add(info.Name, entry);
            }
        }
    }

    public void JoinSinglePlayerWorld(string worldName)
    {
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.JoinRoom(worldName);
    }

    public void JoinMultiPlayerRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void HidePasswordPanel()
    {
        if (PasswordPanel != null)
        {
            PasswordPanel.SetActive(false);
        }
    }

    public void ShowPasswordPanel(RoomInfo roomInfo)
    {
        if (PasswordPanel != null)
        {
            PasswordPanel.SetActive(true);
            PasswordPanelController controller = PasswordPanel.GetComponent<PasswordPanelController>();
            if (controller != null)
            {
                controller.Setup(roomInfo, this);
            }
            else
            {
                Debug.LogError("PasswordPanel에 PasswordPanelController 컴포넌트가 없습니다.");
            }
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("<color=orange>OnLeftRoom:</color> 방에서 퇴장했습니다.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"<color=red>OnDisconnected:</color> 연결이 끊어졌습니다. 원인: {cause}");

        if (mainMenuManager != null)
        {
            mainMenuManager.OnDisconnected();
        }
    }

    private List<string> GetSavedWorlds()
    {
        string json = PlayerPrefs.GetString(SaveKey, "{}");
        WorldList worldList = JsonUtility.FromJson<WorldList>(json);
        return worldList?.names ?? new List<string>();
    }

    [System.Serializable]
    private class WorldList { public List<string> names; }
}