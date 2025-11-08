using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Portal 위치를 기억하고 귀환 기능을 관리합니다.
/// </summary>
public class PortalReturnManager : MonoBehaviour
{
    [Header("귀환 설정")]
    [Tooltip("기본 귀환 위치 (Portal이 없을 경우)")]
    public Vector3 defaultReturnPosition = Vector3.zero;

    [Tooltip("귀환 시 Portal로부터의 오프셋 (플레이어가 Portal 안에 갇히지 않도록)")]
    public Vector2 spawnOffset = new Vector2(0, 2f);

    // 각 플레이어별 마지막 Portal 위치 저장
    private Dictionary<int, Vector3> playerPortalPositions = new Dictionary<int, Vector3>();

    // 싱글톤 패턴 (선택사항)
    private static PortalReturnManager instance;
    public static PortalReturnManager Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 모든 Portal을 찾아서 기본 귀환 위치로 설정
        Portal[] portals = FindObjectsOfType<Portal>(true);
        if (portals.Length > 0)
        {
            defaultReturnPosition = portals[0].transform.position;
            Debug.Log($"[PortalReturnManager] 기본 귀환 위치 설정: {defaultReturnPosition}");
        }
    }

    /// <summary>
    /// 플레이어가 Portal을 사용했을 때 호출하여 위치를 기록합니다.
    /// </summary>
    /// <param name="player">플레이어 GameObject</param>
    /// <param name="portalPosition">Portal 위치</param>
    public void RegisterPortalUsage(GameObject player, Vector3 portalPosition)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        int playerId = pv != null ? pv.ViewID : player.GetInstanceID();

        playerPortalPositions[playerId] = portalPosition;
        Debug.Log($"[PortalReturnManager] {player.name}의 귀환 위치 저장: {portalPosition}");
    }

    /// <summary>
    /// 플레이어를 마지막 사용한 Portal 위치로 귀환시킵니다.
    /// </summary>
    /// <param name="player">귀환할 플레이어</param>
    /// <returns>귀환 성공 여부</returns>
    public bool ReturnToPortal(GameObject player)
    {
        PhotonView pv = player.GetComponent<PhotonView>();

        // Photon 네트워크 체크
        if (pv != null && !pv.IsMine)
        {
            Debug.LogWarning("[PortalReturnManager] 내 캐릭터가 아닙니다.");
            return false;
        }

        int playerId = pv != null ? pv.ViewID : player.GetInstanceID();
        Vector3 returnPosition;

        // 저장된 Portal 위치 확인
        if (playerPortalPositions.TryGetValue(playerId, out returnPosition))
        {
            Debug.Log($"[PortalReturnManager] {player.name}을(를) 저장된 Portal 위치로 귀환: {returnPosition}");
        }
        else
        {
            // 저장된 위치가 없으면 기본 위치로
            returnPosition = defaultReturnPosition;
            Debug.Log($"[PortalReturnManager] {player.name}을(를) 기본 Portal 위치로 귀환: {returnPosition}");
        }

        // 오프셋 적용 (플레이어가 Portal에 겹치지 않도록)
        Vector3 finalPosition = returnPosition + new Vector3(spawnOffset.x, spawnOffset.y, 0);

        // 순간이동
        player.transform.position = finalPosition;
        Debug.Log($"[PortalReturnManager] ✅ 귀환 완료: {finalPosition}");

        // 도시로 돌아왔으므로 온도 감소 비활성화
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.DisableTemperatureDecrease();
        }

        return true;
    }

    /// <summary>
    /// 모든 플레이어의 기본 귀환 위치를 설정합니다.
    /// </summary>
    /// <param name="position">기본 귀환 위치</param>
    public void SetDefaultReturnPosition(Vector3 position)
    {
        defaultReturnPosition = position;
        Debug.Log($"[PortalReturnManager] 기본 귀환 위치 변경: {position}");
    }
}
