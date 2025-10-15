using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// 방 생성 UI와 관련된 로직을 처리합니다.
/// 이 스크립트의 역할은 방을 만들고, 맵 시드를 Room Properties에 저장하는 것까지입니다.
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

    void Awake()
    {
        // 방장이 LoadLevel을 호출하면 모든 클라이언트가 동일한 씬을 로드하도록 설정합니다.
        // 이 설정은 여전히 유효하며, MapManager가 씬에 있는 모든 클라이언트에게 동일하게 작동하도록 보장합니다.
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        // UI 초기 상태 설정
        if (passwordPanel != null && privateRoomToggle != null)
        {
            passwordPanel.SetActive(privateRoomToggle.isOn);
        }
        SelectPlayerCount(2);
    }

    // 비공개 토글 UI의 값이 변경될 때 호출되는 함수입니다.
    public void OnPrivateRoomToggleChanged(bool isPrivate)
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(isPrivate);
        }
    }

    // 플레이어 수 선택 버튼을 눌렀을 때 호출되는 함수입니다.
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

    // "방 만들기" 버튼을 눌렀을 때 호출되는 함수입니다.
    public void OnCreateRoomButtonClicked()
    {
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

        // 맵 시드를 생성하여 Room Properties에 저장하는 핵심 로직은 그대로 유지합니다.
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

    // --- 포톤 콜백 함수들 ---

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공! 이제 방에 참가합니다...");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패. 코드: {returnCode}, 메시지: {message}");
    }

    // 방에 참가했을 때 호출됩니다.
    public override void OnJoinedRoom()
    {
        Debug.Log($"방 '{PhotonNetwork.CurrentRoom.Name}'에 성공적으로 참가했습니다. 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}");
        // 방장만 GameScene을 로드합니다. 다른 클라이언트들은 AutomaticallySyncScene 설정에 따라 자동으로 따라갑니다.
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("방장입니다. 'GameScene'을 로드합니다. 맵 생성은 GameScene의 MapManager가 담당합니다.");
            PhotonNetwork.LoadLevel("Map1");
        }
    }
}
