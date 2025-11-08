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

    private void Awake()
    {
        if (playerPrefab == null)
        {
            playerPrefab = Resources.Load<GameObject>("PlayerPrefab");
        }
    }

    private void Start()
    {
        // PlayerManager는 플레이어 생성 역할만 하고 파괴되어도 괜찮습니다.
        // 만약 게임 내내 유지되어야 하는 로직이 있다면 이 코드를 제거하세요.
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerManager: Player prefab이 할당되지 않았습니다.");
            return;
        }

        // PlayerManager가 방에 참여한 후 인스턴스화된 경우를 대비합니다.
        if (PhotonNetwork.InRoom)
        {
            CreatePlayer();
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        CreatePlayer();
    }

    private void CreatePlayer()
    {
        if (playerPrefab != null)
        {
            // 플레이어 캐릭터 생성
            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 0, -2), Quaternion.identity);
            Debug.Log("플레이어 생성 완료.");

            // 닉네임 설정
            string nickname = UserDataManager.Instance.Nickname;
            if (!string.IsNullOrEmpty(nickname))
            {
                PlayerName playerName = player.GetComponentInChildren<PlayerName>();
                if (playerName != null)
                {
                    playerName.SetName(nickname);
                }
                else
                {
                    Debug.LogError("Player prefab에서 PlayerName 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("UserDataManager에서 닉네임을 가져올 수 없습니다.");
            }

            // PlayerManager는 플레이어 생성 역할만 하고 파괴되어도 괜찮습니다.
<<<<<<< Updated upstream
            // Destroy(gameObject); // 이 라인을 제거하여 PlayerManager가 계속 유지되도록 합니다.
=======
            // 만약 게임 내내 유지되어야 하는 로직이 있다면 이 코드를 제거하세요.
            //Destroy(gameObject);
>>>>>>> Stashed changes
        }
        else
        {
            Debug.LogError("PlayerManager: Player prefab이 할당되지 않았습니다.");
        }
    }
}
