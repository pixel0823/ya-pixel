using UnityEngine;

public class PlayerAttackCollider : MonoBehaviour
{

    [Header("Attack Settings")]
    [Tooltip("공격 데미지 (PlayerAttack에서 동적으로 설정됨)")]
    [SerializeField]
    private float damage = 25f;
    
    [Tooltip("한 번의 공격으로 여러 몬스터를 맞출지")]
    public bool canHitMultiple = true;
    
    [Tooltip("공격 효과음")]
    public AudioClip attackSound;

    // 상태 관리
    private bool hasHit = false;                // 이번 공격에서 이미 맞혔는지
    private bool isActive = false;              // 현재 공격 콜라이더가 활성화되었는지

    // 컴포넌트 참조
    private AudioSource audioSource;
    private Collider2D attackCollider;

    void Awake()
    {
        // 오디오 소스 가져오기 (없으면 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && attackSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 콜라이더 컴포넌트 가져오기
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            // 트리거로 설정 (물리 충돌 방지)
            attackCollider.isTrigger = true;
        }
    }

    void Start()
    {
        // 기본적으로 비활성화 상태
        gameObject.SetActive(false);
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
        Debug.Log($"공격 데미지 설정: {damage:F1}");
    }

    /// <summary>
    /// 공격 시작 (콜라이더 활성화)
    /// PlayerAttack에서 애니메이션 타이밍에 맞춰 호출
    /// </summary>
    public void StartAttack()
    {
        hasHit = false;                     // 타격 상태 리셋
        isActive = true;                    // 공격 활성화
        gameObject.SetActive(true);         // 콜라이더 오브젝트 활성화
        
        Debug.Log("공격 콜라이더 활성화!");
    }

    /// <summary>
    /// 공격 종료 (콜라이더 비활성화)
    /// PlayerAttack에서 공격이 끝날 때 호출
    /// </summary>
    public void EndAttack()
    {
        isActive = false;                   // 공격 비활성화
        gameObject.SetActive(false);        // 콜라이더 오브젝트 비활성화
        hasHit = false;                     // 타격 상태 리셋
        
        Debug.Log("공격 콜라이더 비활성화!");
    }


}
