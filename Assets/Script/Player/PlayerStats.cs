using UnityEngine;
using Photon.Pun; // 포톤 네임스페이스 추가

/// <summary>
/// 플레이어의 주요 스탯(체력, 배고픔, 체온, 산소 등)을 관리하는 클래스입니다.
/// </summary>
public class PlayerStats : MonoBehaviourPunCallbacks // MonoBehaviourPunCallbacks 상속
{
    // PhotonView는 MonoBehaviourPun에서 제공되므로 별도 필드를 두지 않습니다.

    [Header("스탯")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    public float attackDamage = 10f; // 공격력

    [Header("배고픔")]
    [SerializeField] private float maxHunger = 100f;
    private float currentHunger;

    [Header("체온")]
    [SerializeField] private float defaultTemperature = 36.5f;
    private float currentTemperature;

    [Header("산소")]
    [SerializeField] private float maxOxygen = 100f;
    private float currentOxygen;

    #region Public Properties
    // 다른 스크립트에서 현재 스탯 값을 읽기 위한 프로퍼티
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentHunger => currentHunger;
    public float MaxHunger => maxHunger;
    public float CurrentTemperature => currentTemperature;
    public float CurrentOxygen => currentOxygen;
    public float MaxOxygen => maxOxygen;
    #endregion

    private void Start()
    {
        // 게임 시작 시 스탯 초기화
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentTemperature = defaultTemperature;
        currentOxygen = maxOxygen;
    }

    /// <summary>
    /// 플레이어가 데미지를 입었을 때 호출됩니다. (RPC로 네트워크 동기화)
    /// </summary>
    /// <param name="amount">데미지 양</param>
    [PunRPC]
    public void TakeDamage(float amount)
    {
        // 이 RPC를 수신한 클라이언트 중, 이 플레이어의 소유자만 데미지 로직을 실행합니다.
        if (!photonView.IsMine) return;

        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"플레이어가 {amount}의 데미지를 입었습니다. 현재 체력: {currentHealth}");

        // StatusManager UI 업데이트
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.TakeDamage(amount);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 플레이어의 체력을 회복합니다.
    /// </summary>
    /// <param name="amount">회복 양</param>
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"플레이어가 {amount}만큼 회복했습니다. 현재 체력: {currentHealth}");

        // StatusManager UI 업데이트
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.RestoreHealth(amount);
        }
    }

    /// <summary>
    /// 플레이어 사망 시 처리할 로직입니다.
    /// </summary>
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다.");
        // 여기에 사망 관련 로직을 추가하세요. (예: 게임 오버 UI 표시, 캐릭터 애니메이션 변경 등)
    }

    // 참고: 배고픔, 체온, 산소 등이 시간에 따라 변화하는 로직은
    // Update() 메서드에서 별도로 처리하거나, 게임 매니저에서 일정 시간마다 호출해주는 것이 좋습니다.
    // public void UpdatePerSecond()
    // {
    //     currentHunger -= 0.1f;
    // }
}
