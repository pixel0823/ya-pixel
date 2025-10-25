using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// [공용] 방 생성 UI와 관련된 로직을 처리합니다.
/// 게임 모드(Single/Multi)에 따라 다르게 동작합니다.
/// </summary>
public class CreateRoom : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public TMP_InputField roomNameInput;
    public Toggle privateRoomToggle;
    public GameObject passwordPanel;
    public TMP_InputField passwordInput;
    public Button[] playerCountButtons;

    [Header("Button Colors")]
    public Color selectedColor = Color.green;
    public Color defaultColor = Color.white;

    private byte selectedPlayerCount = 2;
    private const string SaveKey = "SinglePlayerWorlds";

    void Start()
    {
        OnPrivateRoomToggleChanged(privateRoomToggle.isOn);
        SelectPlayerCount(2);
    }

    private void OnEnable()
    {
        bool isMultiplayer = GameModeManager.Instance.CurrentMode == GameMode.Multi;
        privateRoomToggle.gameObject.SetActive(isMultiplayer);
        passwordPanel.SetActive(isMultiplayer && privateRoomToggle.isOn);
        foreach (var button in playerCountButtons)
        {
            button.gameObject.SetActive(isMultiplayer);
        }
    }

    public void OnPrivateRoomToggleChanged(bool isPrivate)
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(isPrivate);
        }
    }

    public void SelectPlayerCount(int count)
    {
        selectedPlayerCount = (byte)count;
        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            int buttonPlayerValue = i + 2;
            Image buttonImage = playerCountButtons[i].GetComponent<Image>();
            if (buttonImage == null) { continue; }
            buttonImage.color = (buttonPlayerValue == count) ? selectedColor : defaultColor;
        }
    }

    public void OnCreateRoomButtonClicked()
    {
        if (GameModeManager.Instance.CurrentMode == GameMode.Single)
        {
            CreateSinglePlayerWorld();
        }
        else if (GameModeManager.Instance.CurrentMode == GameMode.Multi)
        {
            CreateMultiPlayerRoom();
        }
    }

    private void CreateSinglePlayerWorld()
    {
        string worldName = roomNameInput.text;
        if (string.IsNullOrEmpty(worldName))
        {
            Debug.LogError("월드 이름이 비어있습니다.");
            return;
        }
        PhotonNetwork.OfflineMode = true;
        // 오프라인 모드에서는 JoinOrCreateRoom을 사용하는 것이 더 안정적일 수 있습니다.
        RoomOptions roomOptions = new RoomOptions { IsVisible = false, MaxPlayers = 1 };
        PhotonNetwork.JoinOrCreateRoom(worldName, roomOptions, TypedLobby.Default);
    }

    private void CreateMultiPlayerRoom()
    {
        if (!PhotonNetwork.InLobby)
        {
            Debug.LogError("로비에 접속해 있지 않습니다. 잠시 후 다시 시도하거나, 메인 메뉴로 돌아가 다시 시도하세요.");
            return;
        }
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("방 이름이 비어있습니다. 방 이름을 입력해주세요.");
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = selectedPlayerCount;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        int mapSeed = (int)System.DateTime.Now.Ticks;
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.CustomRoomProperties.Add("mapSeed", mapSeed);

        if (privateRoomToggle != null && privateRoomToggle.isOn)
        {
            string password = passwordInput.text;
            if (!string.IsNullOrEmpty(password))
            {
                roomOptions.CustomRoomProperties.Add("password", password);
            }
        }
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "password" };

        // ConnectionManager를 통해 방 생성을 요청합니다.
        ConnectionManager.Instance.CreateRoom(roomName, roomOptions);
    }

    // --- 싱글플레이 월드 이름 저장/불러오기 (ConnectionManager로 옮겨도 좋음) ---
    private List<string> GetSavedWorlds()
    {
        string json = PlayerPrefs.GetString(SaveKey, "{}");
        WorldList worldList = JsonUtility.FromJson<WorldList>(json);
        return worldList?.names ?? new List<string>();
    }

    private void SaveWorldName(string worldName)
    {
        List<string> worlds = GetSavedWorlds();
        if (!worlds.Contains(worldName))
        {
            worlds.Add(worldName);
            WorldList worldList = new WorldList { names = worlds };
            string json = JsonUtility.ToJson(worldList);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }

    [System.Serializable]
    private class WorldList { public List<string> names; }
}