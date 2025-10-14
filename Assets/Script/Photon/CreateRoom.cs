using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateRoom : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public TMP_InputField roomNameInput;
    public Toggle privateRoomToggle;      // 비공개방 여부를 선택하는 토글
    public GameObject passwordPanel;       // 비밀번호 입력 필드와 라벨을 포함하는 패널/게임오브젝트
    public TMP_InputField passwordInput;
    public Button[] playerCountButtons;

    [Header("Button Colors")]
    public Color selectedColor = Color.green;
    public Color defaultColor = Color.white;

    private byte selectedPlayerCount = 2;

    void Start()
    {
        // 게임 시작 시 토글 상태에 맞춰 비밀번호 패널의 활성화 여부를 설정
        if (passwordPanel != null && privateRoomToggle != null)
        {
            passwordPanel.SetActive(privateRoomToggle.isOn);
        }
        SelectPlayerCount(2);
    }

    // 비공개방 토글의 OnValueChanged 이벤트에 이 함수를 연결해야 합니다.
    public void OnPrivateRoomToggleChanged(bool isPrivate)
    {
        if (passwordPanel != null)
        {
            passwordPanel.SetActive(isPrivate);
        }
    }

    public void SelectPlayerCount(int count)
    {
        Debug.Log($"인원 수 버튼 클릭: {count}명");
        selectedPlayerCount = (byte)count;

        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            int buttonPlayerValue = i + 2;
            Image buttonImage = playerCountButtons[i].GetComponent<Image>();
            if (buttonImage == null) { continue; }

            if (buttonPlayerValue == count)
            {
                buttonImage.color = selectedColor;
            }
            else
            {
                buttonImage.color = defaultColor;
            }
        }
    }

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

        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        
        string password = ""; // 우선 비밀번호를 빈 문자열로 초기화
        // 비공개 토글이 켜져 있을 때만 비밀번호 입력 필드의 값을 가져옴
        if (privateRoomToggle != null && privateRoomToggle.isOn)
        {
            password = passwordInput.text;
        }

        roomOptions.CustomRoomProperties.Add("password", password);
        
        // 중요: 커스텀 속성을 로비에 공개할 때, "password" 외에 다른 기본 정보(예: "maxPlayers")를 함께 공개하여
        // 방 정보가 누락되는 것을 방지합니다.
        // "maxPlayers"는 RoomInfo.MaxPlayers로 이미 접근 가능하지만, 명시적으로 공개 목록에 추가하여 안정성을 높입니다.
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "password", "maxPlayers" };

        Debug.Log($"방 생성 시도: '{roomName}', 최대 인원: {selectedPlayerCount}명");
        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공! 방 이름: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("Map1"); 
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패. 코드: {returnCode}, 메시지: {message}");
    }
}