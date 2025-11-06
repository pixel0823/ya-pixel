using UnityEngine;

/// <summary>
/// 귀환석 아이템. Portal 위치로 순간이동합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Return Stone", menuName = "Inventory/Return Stone")]
public class ReturnStone : Item
{
    [Header("귀환석 설정")]
    [Tooltip("사용 시 소모되는지 여부")]
    public bool isConsumable = false;

    [Tooltip("귀환 쿨타임 (초)")]
    public float cooldownTime = 5f;

    /// <summary>
    /// 귀환석을 사용합니다.
    /// </summary>
    /// <param name="player">사용하는 플레이어</param>
    /// <returns>사용 성공 여부</returns>
    public bool Use(GameObject player)
    {
        PortalReturnManager returnManager = Object.FindObjectOfType<PortalReturnManager>();

        if (returnManager == null)
        {
            Debug.LogError("[ReturnStone] PortalReturnManager를 찾을 수 없습니다.");
            return false;
        }

        // 귀환 실행
        bool success = returnManager.ReturnToPortal(player);

        if (success)
        {
            Debug.Log($"[ReturnStone] {player.name}이(가) 귀환석을 사용했습니다.");
            return true;
        }
        else
        {
            Debug.LogWarning($"[ReturnStone] 귀환에 실패했습니다.");
            return false;
        }
    }
}
