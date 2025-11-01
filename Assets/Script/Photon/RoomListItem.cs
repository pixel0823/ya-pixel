using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text roomNameText;
    public TMP_Text playerCountText; // 멀티플레이 전용
    public Image lockIcon; // 비밀방 표시용

    // 내부 상태
    private GameMode mode;
    private string roomName;
    private ConnectionManager connectionManager; // ConnectionManager 참조
    private RoomInfo roomInfo;

    // 멀티플레이 방 정보를 설정
    public void SetupForMultiplayer(RoomInfo info, ConnectionManager manager)
    {
        mode = GameMode.Multi;
        roomInfo = info;
        roomName = info.Name;
        connectionManager = manager;

        roomNameText.text = info.Name;
        playerCountText.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        playerCountText.gameObject.SetActive(true);

        bool isPasswordProtected = info.CustomProperties.ContainsKey("password");
        lockIcon.gameObject.SetActive(isPasswordProtected);
    }

    // 싱글플레이 월드 정보를 설정
    public void SetupForSingleplayer(string worldName, ConnectionManager manager)
    {
        mode = GameMode.Single;
        this.roomName = worldName;
        connectionManager = manager;

        roomNameText.text = worldName;
        playerCountText.gameObject.SetActive(false);
        lockIcon.gameObject.SetActive(false);
    }

    // '참가' 버튼에 연결될 함수
    public void OnJoinButtonClicked()
    {
        if (mode == GameMode.Single)
        {
            connectionManager.JoinSinglePlayerWorld(roomName);
        }
        else // GameMode.Multi
        {
            bool isPasswordProtected = roomInfo != null && roomInfo.CustomProperties.ContainsKey("password") && !string.IsNullOrEmpty(roomInfo.CustomProperties["password"].ToString());
            if (isPasswordProtected)
            {
                // 비밀번호가 있는 방 -> 비밀번호 입력 패널 표시
                connectionManager.ShowPasswordPanel(roomInfo);
            }
            else
            {
                // 비밀번호가 없는 방 -> 바로 참가
                connectionManager.JoinMultiPlayerRoom(roomName);
            }
        }
    }
}