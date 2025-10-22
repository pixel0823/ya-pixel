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
        if (GameModeManager.Instance.CurrentMode == GameMode.Multi)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("<color=green>OnJoinedLobby:</color> 로비에 접속했습니다. 이제 방 목록을 받을 수 있습니다.");

        // MainMenuManager에게 로비에 접속했음을 알려 UI를 전환하도록 합니다.
        // if (mainMenuManager != null)
        // {
        //     mainMenuManager.OnJoinedLobby();
        // }
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

    public override void OnJoinedRoom()
    {
        Debug.Log("<color=green>OnJoinedRoom:</color> 방에 입장했습니다.");
        // 씬 로딩은 CreateRoom.cs에서 담당합니다.
    }

    public override void OnLeftRoom()
    {
        Debug.Log("<color=orange>OnLeftRoom:</color> 방에서 퇴장했습니다.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"<color=red>OnDisconnected:</color> 연결이 끊어졌습니다. 원인: {cause}");

        // MainMenuManager에게 연결이 끊어졌음을 알려 UI를 초기 상태로 되돌리도록 합니다.
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