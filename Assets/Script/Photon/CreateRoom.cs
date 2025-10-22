using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic; // For List

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

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        // UI 초기 상태 설정
        OnPrivateRoomToggleChanged(privateRoomToggle.isOn);
        SelectPlayerCount(2);
    }

    private void OnEnable()
    {
        // 이 UI가 활성화될 때, 게임 모드에 따라 UI 상태를 조정합니다.
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

    // "방 만들기" 버튼을 눌렀을 때 호출되는 공용 함수입니다.
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
        // 오프라인 모드에서는 CreateRoom이 성공하면 즉시 OnJoinedRoom 콜백이 호출됩니다.
        if (PhotonNetwork.CreateRoom(worldName))
        {
            Debug.Log($"오프라인 월드 '{worldName}' 생성 성공.");
            SaveWorldName(worldName);
        }
        else
        {
            Debug.LogError("오프라인 월드 생성에 실패했습니다. 이미 같은 이름의 월드가 있을 수 있습니다.");
        }
    }

    private void CreateMultiPlayerRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("포톤 서버에 연결되어 있지 않습니다. 메인 메뉴로 돌아가 다시 시도하세요.");
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

        Debug.Log($"방 생성 시도: '{roomName}', 최대 인원: {selectedPlayerCount}명, 맵 시드: {mapSeed}");
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    // --- 포톤 콜백 함수들 (공용) ---

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공! 방 이름: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패. 코드: {returnCode}, 메시지: {message}");
    }

    // 방에 참가했을 때 호출됩니다. (싱글/멀티 공용)
    public override void OnJoinedRoom()
    {
        Debug.Log($"방 '{PhotonNetwork.CurrentRoom.Name}'에 성공적으로 참가했습니다. 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("방장입니다. 'Map1' 씬을 로드합니다.");
            PhotonNetwork.LoadLevel("Map1");
        }
    }

    // --- 싱글플레이 월드 이름 저장/불러오기 ---
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
            Debug.Log($"월드 '{worldName}'이(가) 로컬에 저장되었습니다.");
        }
    }

    [System.Serializable]
    private class WorldList { public List<string> names; }
}
