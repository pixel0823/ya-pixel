using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections; // Coroutine을 위해 추가

public class MonsterAI : MonoBehaviourPunCallbacks, IPunObservable
{
    // --- 주요 컴포넌트 ---
        protected Animator animator;
        // (A* Pathfinding 등 다른 AI 컴포넌트가 있다면 이곳에)
        public PhotonView photonView;
    
        // --- 상태 변수 ---
        public float moveSpeed = 1.5f;
        protected Vector2 moveDirection = Vector2.zero;
        protected Vector2 lastMoveDirection = Vector2.down; // 기본값 (아래)
        protected bool isWalking = false;
        private bool isDead = false; // 죽음 상태 변수 추가
        protected bool isAttacking = false;

        // --- 체력 --
        public float maxHealth = 100f;
        public float currentHealth;
        public float attackDamage = 10f; // 몬스터의 공격력
    
        // --- 타겟 (플레이어) ---
        public Transform targetPlayer; // AI가 추적할 대상
    
        // --- 감지 범위 ---
        public float detectionRange = 1.5f; // 플레이어 감지 범위
        public float attackRange = 0.2f;   // 공격 범위
    
        // --- 공격 쿨다운 ---
        public float attackCooldown = 1.0f; // 공격 쿨다운 시간 (1초)
        private float lastAttackTime = -99f;  // 마지막 공격 시간
    
        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            photonView = GetComponent<PhotonView>();
    
            if (animator == null)
                Debug.LogError("MonsterAI에 Animator 컴포넌트가 없습니다.");
            if (photonView == null)
                Debug.LogError("MonsterAI에 PhotonView 컴포넌트가 없습니다.");
        }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        // 마스터 클라이언트 체크 제거, 바로 플레이어 탐색
        FindTargetPlayer();
    }

    protected virtual void Update()
    {
        // 죽었다면 더 이상 AI 로직을 실행하지 않음
        if (isDead)
        {
            return;
        }

        if (photonView.IsMine) // 마스터 클라이언트 또는 PhotonView 소유자만 AI 로직 실행
        {
            // 공격 상태일 경우, 0.7초 타이머를 기준으로 행동을 처리합니다.
            if (isAttacking)
            {
                // 공격 후 0.7초가 지나기 전까지는 움직이지 않고 대기합니다.
                if (Time.time < lastAttackTime + 0.7f) // 애니메이션 길이 0.7초
                {
                    isWalking = false;
                    moveDirection = Vector2.zero;
                    UpdateAnimatorParameters();
                    return; // 다른 AI 로직 실행 방지
                }
                else
                {
                    isAttacking = false; // 0.7초가 지났으면 공격 상태 해제
                }
            }

            // --- AI 로직 (isAttacking이 false일 때만 실행) ---

            // 1. 타겟이 없으면 플레이어 탐색
            if (targetPlayer == null)
            {
                FindTargetPlayer();
                isWalking = false;
                moveDirection = Vector2.zero;
                UpdateAnimatorParameters();
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

            // 2. 공격 범위 안에 있고 공격 쿨다운이 끝났는가? -> 공격
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                isWalking = false;
                moveDirection = Vector2.zero;
                lastMoveDirection = (targetPlayer.position - transform.position).normalized;
                Attack(); // isAttacking을 true로 설정하고 애니메이션을 발동합니다.
            }
            // 3. 감지 범위 안에 있는가? -> 추적 또는 대기
            else if (distanceToPlayer <= detectionRange)
            {
                // 공격 쿨다운이 아직 안 끝났다면(공격 후 1초가 안 지났다면) 대기
                if (Time.time < lastAttackTime + attackCooldown) // attackCooldown은 1.0f
                {
                    isWalking = false;
                    moveDirection = Vector2.zero;
                }
                // 쿨다운이 끝났으면 플레이어 추격
                else
                {
                    isWalking = true;
                    moveDirection = (targetPlayer.position - transform.position).normalized;
                    transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
                    lastMoveDirection = moveDirection;
                }
            }
            // 4. 범위 밖 -> 대기
            else
            {
                isWalking = false;
                moveDirection = Vector2.zero;
            }

            UpdateAnimatorParameters();

            // --- 테스트용 디버그 키 ---
            if (Input.GetKeyDown(KeyCode.H))
            {
                // Master Client를 통해 데미지 처리 요청
                photonView.RPC("TakeDamage", RpcTarget.MasterClient, 10f);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                Die();
            }
        }
        else // PhotonView 소유자가 아닌 클라이언트는 수신된 데이터로 애니메이션만 업데이트
        {
            UpdateAnimatorParameters();
        }
    }

    // AI가 계산한 값을 애니메이터에 적용
    protected void UpdateAnimatorParameters()
    {
        animator.SetBool("IsWalking", isWalking);
        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);

        if (!isWalking)
        {
            animator.SetFloat("LastMoveX", lastMoveDirection.x);
            animator.SetFloat("LastMoveY", lastMoveDirection.y);
        }
    }

    // 근처의 플레이어를 찾는 로직 (Photon 대신 Tag 사용)
    protected virtual void FindTargetPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            targetPlayer = playerObject.transform;
        }
        else
        {
            Invoke("FindTargetPlayer", 5.0f);
        }
    }

    // --- 네트워크 동기화 (RPC 및 SerializeView) ---

    // 공격 실행 (로컬)
    public virtual void Attack()
    {
        if (!photonView.IsMine) return; // Only owner can initiate attack

        // 쿨다운은 Update에서 체크하지만, 만약을 위해 이중 체크
        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }
        lastAttackTime = Time.time; // 공격 시작 시간 기록
        isAttacking = true; // 공격 상태로 전환

        // 모든 클라이언트에서 공격 애니메이션을 재생하도록 RPC 호출
        photonView.RPC("RPC_Attack", RpcTarget.All, lastMoveDirection.x, lastMoveDirection.y);

        // 0.35초 후에 데미지를 적용하는 코루틴 시작 (애니메이션 중간쯤)
        StartCoroutine(ApplyDamageAfterDelay(0.35f));
    }

    private IEnumerator ApplyDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 마스터 클라이언트이고, 타겟이 여전히 공격 범위 내에 있는지 확인
        if (photonView.IsMine && targetPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
            if (distanceToPlayer <= attackRange)
            {
                // 플레이어 오브젝트의 PhotonView를 통해 RPC를 호출해야 합니다.
                PhotonView playerPhotonView = targetPlayer.GetComponent<PhotonView>();
                if (playerPhotonView != null)
                {
                    // 모든 클라이언트에게 TakeDamage RPC를 호출합니다.
                    // 실제 데미지 처리는 PlayerStats의 IsMine 체크를 통해 소유자 클라이언트에서만 이루어집니다.
                    playerPhotonView.RPC("TakeDamage", RpcTarget.All, attackDamage);
                    Debug.Log($"[MonsterAI] {targetPlayer.name}에게 {attackDamage} 데미지 입힘!");
                }
            }
        }
    }

    [PunRPC]
    protected void RPC_Attack(float lastX, float lastY)
    {
        animator.SetFloat("LastMoveX", lastX);
        animator.SetFloat("LastMoveY", lastY);
        animator.SetTrigger("Attack");
    }

    // 플레이어의 공격에 의해 호출될 함수 (MasterClient에서만 체력 계산)
    [PunRPC]
    public void TakeDamage(float damage)
    {
        // 모든 클라이언트에서 일단 피격 애니메이션은 재생
        animator.SetTrigger("Hurt");

        // 마스터 클라이언트가 아니거나, 내가 컨트롤하는 몬스터가 아니거나, 이미 죽었다면 로직 중단
        if (!PhotonNetwork.IsMasterClient || !photonView.IsMine || isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 몬스터가 죽었을 때 호출될 함수
    public void Die()
    {
        if (!photonView.IsMine) return; // Only owner can initiate death
        if (isDead) return;

        // 모든 클라이언트에서 죽음 상태를 동기화하도록 RPC 호출
        photonView.RPC("RPC_Die", RpcTarget.All);
    }

    [PunRPC]
    protected void RPC_Die()
    {
        if (isDead) return; // 이미 죽음 상태라면 중복 처리 방지
        isDead = true;
        animator.SetBool("IsDead", true);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // 애니메이션 재생 시간보다 긴 고정된 시간 후에 파괴
        if (photonView.IsMine)
        {
            StartCoroutine(DestroyAfterDelay(0.5f)); // 0.5초 후 파괴
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 마스터 클라이언트가 데이터를 전송합니다.
            stream.SendNext(transform.position);
            stream.SendNext(isWalking);
            stream.SendNext(moveDirection.x);
            stream.SendNext(moveDirection.y);
            stream.SendNext(lastMoveDirection.x);
            stream.SendNext(lastMoveDirection.y);
            stream.SendNext(isAttacking);
            stream.SendNext(isDead);
            stream.SendNext(currentHealth);
        }
        else
        {
            // 다른 클라이언트가 데이터를 수신합니다.
            Vector3 networkPosition = (Vector3)stream.ReceiveNext();
            bool networkIsWalking = (bool)stream.ReceiveNext();
            float networkMoveX = (float)stream.ReceiveNext();
            float networkMoveY = (float)stream.ReceiveNext();
            float networkLastMoveX = (float)stream.ReceiveNext();
            float networkLastMoveY = (float)stream.ReceiveNext();
            bool networkIsAttacking = (bool)stream.ReceiveNext();
            bool networkIsDead = (bool)stream.ReceiveNext();
            float networkHealth = (float)stream.ReceiveNext();

            // 수신된 데이터를 보간하여 부드러운 움직임을 만듭니다.
            // (여기서는 간단히 직접 적용하지만, 실제 게임에서는 보간 로직이 필요합니다.)
            transform.position = Vector3.Lerp(transform.position, networkPosition, 0.1f); // 보간
            isWalking = networkIsWalking;
            moveDirection = new Vector2(networkMoveX, networkMoveY);
            lastMoveDirection = new Vector2(networkLastMoveX, networkLastMoveY);
            isAttacking = networkIsAttacking;
            isDead = networkIsDead;
            currentHealth = networkHealth;
        }
    }
}