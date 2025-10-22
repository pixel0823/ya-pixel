using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// [공용] 포톤 서버 연결 및 방 목록 관리를 처리합니다.
/// 게임 모드(Single/Multi)에 따라 다르게 동작합니다.
/// </summary>
public class ConnectionManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public GameObject MainMenuUI;
    public GameObject MultiUI; // 로비 UI (방 목록, 생성/참가 버튼 포함)
    public GameObject RoomListPanel; // 방 목록을 담고 있는 UI 패널
    public GameObject PasswordPanel; // 비밀번호 입력 패널

    [Header("Room List")]
    public GameObject roomListItemPrefab;
    public Transform roomListContent;

    private Dictionary<string, GameObject> roomListEntries = new Dictionary<string, GameObject>();
    private const string SaveKey = "SinglePlayerWorlds";

    // 멀티플레이 모드에서 포톤 서버에 연결을 시작합니다.
    public void Connect()
    {
        if (PhotonNetwork.IsConnected) return;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("포톤 마스터 서버에 접속을 시도합니다...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("<color=green>OnConnectedToMaster:</color> 포톤 마스터 서버에 접속했습니다.");
        // 멀티플레이 모드일 때만 로비에 접속합니다.
        if (GameModeManager.Instance.CurrentMode == GameMode.Multi)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("<color=green>OnJoinedLobby:</color> 로비에 접속했습니다. 이제 방 목록을 받을 수 있습니다.");

        // MainMenuManager 등을 통해 ConnectingUI를 끄고 LobbyChoiceUI를 켜주는 것이 이상적입니다.
        // 여기서는 MultiUI(LobbyChoiceUI)를 직접 켭니다.
        var mainMenuManager = FindObjectOfType<MainMenuManager>();
        if (mainMenuManager != null)
        {
            if (mainMenuManager.connectingUI != null) mainMenuManager.connectingUI.SetActive(false);
            if (mainMenuManager.lobbyChoiceUI != null) mainMenuManager.lobbyChoiceUI.SetActive(true);
        }
    }

    // 이 함수는 '방 목록 보기' UI가 활성화될 때 호출됩니다.
    public void RefreshRoomList()
    {
        // 기존 목록 정리
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListEntries.Clear();

        if (GameModeManager.Instance.CurrentMode == GameMode.Single)
        {
            PopulateForSinglePlayer();
        }
        // 멀티플레이 모드는 OnRoomListUpdate 콜백이 목록을 채우므로 여기서는 아무것도 하지 않습니다.
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
        // 싱글플레이 모드에서는 이 콜백을 무시합니다.
        if (GameModeManager.Instance.CurrentMode != GameMode.Multi) return;

        Debug.Log($"<color=cyan>OnRoomListUpdate:</color> 방 목록 업데이트 수신.");

        foreach (var entry in roomListEntries)
        {
            entry.Value.SetActive(false); //일단 비활성화
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
                // 기존 항목 업데이트
                roomListEntries[info.Name].SetActive(true);
                RoomListItem listItem = roomListEntries[info.Name].GetComponent<RoomListItem>();
                listItem.SetupForMultiplayer(info, this);
            }
            else
            {
                // 새 항목 추가
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

    // --- 방 참가 로직 (공용) ---
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

    // --- 기타 콜백 및 유틸리티 ---
    public override void OnJoinedRoom()
    {
        Debug.Log("<color=green>OnJoinedRoom:</color> 방에 입장했습니다.");
        // 씬 로딩은 CreateRoom.cs에서 담당하므로 여기서는 UI만 숨깁니다.
        var mainMenuManager = FindObjectOfType<MainMenuManager>();
        if (mainMenuManager != null && mainMenuManager.lobbyChoiceUI != null)
        {
            mainMenuManager.lobbyChoiceUI.SetActive(false);
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("<color=orange>OnLeftRoom:</color> 방에서 퇴장했습니다.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"<color=red>OnDisconnected:</color> 연결이 끊어졌습니다. 원인: {cause}");

        var mainMenuManager = FindObjectOfType<MainMenuManager>();
        if (mainMenuManager != null)
        {
            if (mainMenuManager.mainMenuUI != null) mainMenuManager.mainMenuUI.SetActive(true);
            if (mainMenuManager.lobbyChoiceUI != null) mainMenuManager.lobbyChoiceUI.SetActive(false);
            if (mainMenuManager.connectingUI != null) mainMenuManager.connectingUI.SetActive(false);
        }
    }

    // --- 싱글플레이 월드 이름 불러오기 ---
    private List<string> GetSavedWorlds()
    {
        string json = PlayerPrefs.GetString(SaveKey, "{}");
        WorldList worldList = JsonUtility.FromJson<WorldList>(json);
        return worldList?.names ?? new List<string>();
    }

    [System.Serializable]
    private class WorldList { public List<string> names; }
}
