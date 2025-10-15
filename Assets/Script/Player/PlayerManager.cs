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

    /// <summary>
    /// 이 스크립트가 활성화될 때 Photon 콜백을 등록합니다.
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable(); // 반드시 호출해야 합니다.
    }

    /// <summary>
    /// 클라이언트가 방에 성공적으로 입장했을 때 Photon에 의해 호출됩니다.
    /// Start() 대신 이 콜백을 사용하여 플레이어를 생성하면 타이밍 문제를 해결할 수 있습니다.
    /// </summary>
    public override void OnJoinedRoom()
    {
        // Check if we are in a room
        if (PhotonNetwork.InRoom)
        {
            // 인스펙터에서 할당한 프리팹의 이름 사용
            PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
            Debug.Log("Player instantiated.");

            // PlayerManager는 플레이어 생성 역할만 하고 파괴되어도 괜찮습니다.
            // 만약 게임 내내 유지되어야 하는 로직이 있다면 이 코드를 제거하세요.
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 이 스크립트가 비활성화될 때 Photon 콜백을 해제합니다.
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable(); // 반드시 호출해야 합니다.
    }
}
