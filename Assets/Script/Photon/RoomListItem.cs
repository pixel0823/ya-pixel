using UnityEngine;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text playerCountText;
    public GameObject privateIcon;

    public void SetRoomInfo(Photon.Realtime.RoomInfo roomInfo)
    {
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

        roomNameText.text = roomInfo.Name;
        playerCountText.text = $"{roomInfo.PlayerCount} / {roomInfo.MaxPlayers}";
        
        if (privateIcon != null)
        {
            privateIcon.SetActive(!roomInfo.IsOpen);
        }
    }
}
