using UnityEngine;
using Photon.Pun;

/// <summary>
/// 게임 씬에서 플레이어 캐릭터를 생성하고 관리합니다.
/// MonoBehaviourPunCallbacks를 상속받아 Photon의 네트워크 이벤트를 직접 처리합니다.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Tooltip("Resources 폴더에 있는 플레이어 프리팹")]
    public GameObject playerPrefab;

    private void Start()
    {
        if (playerPrefab != null && PhotonNetwork.InRoom)
        {
            // 플레이어 캐릭터 생성
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
            Debug.Log("플레이어 생성 완료.");

            // PlayerManager는 플레이어 생성 역할만 하고 파괴되어도 괜찮습니다.
            // 만약 게임 내내 유지되어야 하는 로직이 있다면 이 코드를 제거하세요.
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("PlayerManager: Player prefab이 할당되지 않았거나, 방에 연결되지 않았습니다.");
        }
    }
}
