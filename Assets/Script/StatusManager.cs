using UnityEngine;
using UnityEngine.UI;

public class StatusManager : MonoBehaviour
{
    public static StatusManager Instance { get; private set; } // 싱글톤 패턴

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider temperatureSlider;

    [Header("Status Values")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float maxTemperature = 100f;

    [Header("Hunger Decrease Settings")]
    [SerializeField] private float hungerDecreaseOnMove = 5f; // 움직일 때 5초마다 감소할 배고픔
    [SerializeField] private float hungerDecreaseOnMining = 3f; // 채광 시 감소할 배고픔
    [SerializeField] private float hungerDecreaseOnAttack = 2f; // 공격 시 감소할 배고픔
    [SerializeField] private float moveCheckInterval = 5f; // 움직임 체크 간격 (초)


    private float currentHealth;
    private float currentHunger;
    private float currentTemperature;

    // 온도 감소 속도 계산: 100을 15분(900초)에 걸쳐 0으로 만들기
    private float temperatureDecreaseRate = 100f / 900f; // 약 0.111 per second

    // 움직임 감지 관련 변수
    private float moveTimer = 0f;
    private bool wasWalking = false; // 이전 프레임에 걷고 있었는지 여부

    // 온도 감소 활성화 여부 (도시에선 false, 야외에선 true)
    private bool isTemperatureDecreaseEnabled = false;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 초기값 설정
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentTemperature = maxTemperature;

        // Slider 최대값 설정 및 조작 불가능하게 만들기
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            healthSlider.interactable = false; // 마우스로 조작 불가
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = currentHunger;
            hungerSlider.interactable = false; // 마우스로 조작 불가
        }

        if (temperatureSlider != null)
        {
            temperatureSlider.maxValue = maxTemperature;
            temperatureSlider.value = currentTemperature;
            temperatureSlider.interactable = false; // 마우스로 조작 불가
        }

    }

    void Update()
    {
        // 온도 자동 감소 (야외에 있을 때만)
        if (isTemperatureDecreaseEnabled && currentTemperature > 0)
        {
            currentTemperature -= temperatureDecreaseRate * Time.deltaTime;
            currentTemperature = Mathf.Max(0, currentTemperature); // 0 이하로 내려가지 않게
            UpdateTemperatureUI();
        }
    }

    // 온도 감소 활성화 (야외로 나갈 때 호출)
    public void EnableTemperatureDecrease()
    {
        isTemperatureDecreaseEnabled = true;
        Debug.Log("온도 감소 활성화 - 야외 환경");
    }

    // 온도 감소 비활성화 (도시로 돌아올 때 호출)
    public void DisableTemperatureDecrease()
    {
        isTemperatureDecreaseEnabled = false;
        Debug.Log("온도 감소 비활성화 - 도시 환경");
    }

    // 걷기 시작/종료 추적 (PlayerMovement에서 호출)
    public void UpdateWalkingState(bool isWalking)
    {
        if (isWalking)
        {
            moveTimer += Time.deltaTime;

            // 5초마다 배고픔 감소
            if (moveTimer >= moveCheckInterval)
            {
                DecreaseHunger(hungerDecreaseOnMove);
                moveTimer = 0f; // 타이머 초기화
            }
        }
        else
        {
            // 걷지 않으면 타이머 초기화
            if (wasWalking)
            {
                moveTimer = 0f;
            }
        }

        wasWalking = isWalking;
    }

    // 배고픔 감소 (내부 메서드)
    private void DecreaseHunger(float amount)
    {
        currentHunger -= amount;
        currentHunger = Mathf.Max(0, currentHunger);
        UpdateHungerUI();
    }

    // 체력 피해 받기
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }

    // 체력 회복 (배고픔 100일 때만 가능, 체력 30 회복 시 배고픔 20 소모)
    public void RestoreHealth(float amount)
    {
        if (currentHunger < 100f)
        {
            Debug.Log("배고픔이 100이 아니어서 체력을 회복할 수 없습니다!");
            return;
        }

        // 체력 회복
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        UpdateHealthUI();

        // 배고픔 소모 계산 (체력 30당 배고픔 20)
        float hungerCost = (amount / 30f) * 20f;
        currentHunger -= hungerCost;
        currentHunger = Mathf.Max(0, currentHunger);
        UpdateHungerUI();

        Debug.Log($"체력 {amount} 회복! 배고픔 {hungerCost} 소모!");
    }

    // 음식 섭취 (배고픔 회복)
    public void ConsumeFood(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Min(maxHunger, currentHunger);
        UpdateHungerUI();

        Debug.Log($"배고픔 {amount} 회복!");
    }

    // 채광 시 배고픔 감소 (외부 스크립트에서 호출)
    public void OnMining()
    {
        DecreaseHunger(hungerDecreaseOnMining);
        Debug.Log($"채광으로 배고픔 {hungerDecreaseOnMining} 감소!");
    }

    // 공격 시 배고픔 감소 (외부 스크립트에서 호출)
    public void OnAttack()
    {
        DecreaseHunger(hungerDecreaseOnAttack);
        Debug.Log($"공격으로 배고픔 {hungerDecreaseOnAttack} 감소!");
    }

    // 온도 회복 (예: 모닥불 근처 등)
    public void RestoreTemperature(float amount)
    {
        currentTemperature += amount;
        currentTemperature = Mathf.Min(maxTemperature, currentTemperature);
        UpdateTemperatureUI();

        Debug.Log($"온도 {amount} 상승!");
    }

    // UI 업데이트 메서드들
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    private void UpdateHungerUI()
    {
        if (hungerSlider != null)
        {
            hungerSlider.value = currentHunger;
        }
    }

    private void UpdateTemperatureUI()
    {
        if (temperatureSlider != null)
        {
            temperatureSlider.value = currentTemperature;
        }
    }

    // 플레이어 사망 처리
    private void OnPlayerDeath()
    {
        Debug.Log("플레이어가 사망했습니다!");
        // 여기에 사망 처리 로직 추가 (게임 오버, 리스폰 등)
    }

    // 현재 상태값 가져오기 (다른 스크립트에서 참조용)
    public float GetCurrentHealth() => currentHealth;
    public float GetCurrentHunger() => currentHunger;
    public float GetCurrentTemperature() => currentTemperature;
    public float GetMaxHealth() => maxHealth;
    public float GetMaxHunger() => maxHunger;
    public float GetMaxTemperature() => maxTemperature;
}
