using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public GameObject privateIcon;
    
    private RoomInfo roomInfo;
    private ConnectionManager connectionManager;

    public void Setup(RoomInfo info, ConnectionManager manager)
    {
        this.roomInfo = info;
        this.connectionManager = manager;

        if (roomNameText == null)
        {
            Debug.LogError("RoomListItem: 'Room Name Text'가 Inspector에 설정되지 않았습니다.", this.gameObject);
            return;
        }
        if (playerCountText == null)
        {
            Debug.LogError("RoomListItem: 'Player Count Text'가 Inspector에 설정되지 않았습니다.", this.gameObject);
            return;
        }
        if (privateIcon == null)
        {
            Debug.LogWarning("RoomListItem: 'Private Icon'이(가) Inspector에 설정되지 않았습니다.", this.gameObject);
        }

        roomNameText.text = this.roomInfo.Name;
        playerCountText.text = $"{this.roomInfo.PlayerCount} / {this.roomInfo.MaxPlayers}";

        if (privateIcon != null)
        {
            // "password" 커스텀 프로퍼티가 존재하기만 하면 비공개 방으로 취급합니다.
            bool isPrivate = this.roomInfo.CustomProperties.ContainsKey("password");
            privateIcon.SetActive(isPrivate);
        }
    }

    public void OnItemClick()
    {
        if (roomInfo.CustomProperties.ContainsKey("password"))
        {
            // 비밀번호가 있는 방: ConnectionManager에게 비밀번호 패널을 보여달라고 요청
            connectionManager.ShowPasswordPanel(roomInfo);
        }
        else
        {
            // 비밀번호가 없는 방: 바로 입장 시도
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }
    }
}
