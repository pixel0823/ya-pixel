using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        // Check if we are in a room
        if (PhotonNetwork.InRoom)
        {
            // 인스펙터에서 할당한 프리팹의 이름 사용
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
            Debug.Log("Player instantiated.");
        }
        else
        {
            Debug.LogWarning("Not in a room, player not instantiated.");
        }
    }
}
