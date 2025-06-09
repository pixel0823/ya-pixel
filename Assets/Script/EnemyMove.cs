using UnityEngine;

public enum State { Idle, Patrol, Chase, Attack, Dead }

public class EnemyMove : MonoBehaviour
{
    public float hp = 100f;
    public float moveSpeed = 2f;
    public float attackRange = 1.5f; // 공격 가능 거리
    public float chaseRange = 3f;    // 추적 가능 거리
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    public int nextMove;

    public State currentState;
    public Transform target;

    private bool isAttacking = false;
    private float attackCooldown = 1.5f;
    private float lastAttackTime = -Mathf.Infinity;

    void Awake()
    {
        currentState = State.Patrol;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Invoke("Think", 5);
    }

    void Update()
    {
        // 상태 전환 조건 체크
        if (hp <= 0 && currentState != State.Dead)
        {
            currentState = State.Dead;
            anim.SetTrigger("Dead");
            return;
        }

        if (currentState == State.Dead) return;

        float dist = Vector2.Distance(transform.position, target.position);

        // 상태 전환 로직
        if (currentState == State.Attack)
        {
            if (dist > attackRange)
            {
                if (dist > chaseRange)
                    currentState = State.Patrol;
                else
                    currentState = State.Chase;
            }
        }
        else if (currentState == State.Chase)
        {
            if (dist > chaseRange)
                currentState = State.Patrol;
            else if (dist <= attackRange)
                currentState = State.Attack;
        }
        else if (currentState == State.Patrol)
        {
            if (dist <= chaseRange)
                currentState = State.Chase;
        }

        // 애니메이션 파라미터 동기화
        anim.SetBool("isChasing", currentState == State.Chase);
        anim.SetBool("isAttacking", currentState == State.Attack);
        anim.SetBool("isPatrolling", currentState == State.Patrol);
    }

    void FixedUpdate()
    {
        // 상태별 동작(이동, 공격 등)
        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Patrol: Patrol(); break;
            case State.Attack: Attack(); break;
            case State.Chase: Chase(); break;
            case State.Dead: Dead(); break;
        }
    }

    public void Chase()
    {
        if (target == null) return;
        Vector2 dir = (target.position - transform.position).normalized;
        rigid.linearVelocity = new Vector2(dir.x * moveSpeed, rigid.linearVelocityY);
        spriteRenderer.flipX = dir.x > 0;
    }

    public void Attack()
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        rigid.linearVelocity = Vector2.zero;
        Vector2 dir = (target.position - transform.position).normalized;
        spriteRenderer.flipX = dir.x > 0;
        anim.SetTrigger("Attack");

        Invoke("BackToChase", 0.25f);
    }

    private void BackToChase()
    {
        isAttacking = false;
        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange)
            currentState = State.Attack;
        else if (dist <= chaseRange)
            currentState = State.Chase;
        else
            currentState = State.Patrol;
    }

    public void Patrol()
    {
        rigid.linearVelocity = new Vector2(nextMove * moveSpeed, rigid.linearVelocityY);
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove, rigid.position.y);
        float rayLength = 3f;
        Debug.DrawRay(frontVec, Vector2.down * rayLength, Color.green);
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, rayLength, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {
            nextMove *= -1;
            spriteRenderer.flipX = nextMove == 1;
            CancelInvoke();
            Invoke("Think", 5);
        }

        spriteRenderer.flipX = nextMove == 1;
    }

    public bool PlayerInRange(float range)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= range;
    }

    public void Idle()
    {
        rigid.linearVelocity = Vector2.zero;
    }

    public void Dead()
    {
        rigid.linearVelocity = Vector2.zero;
        anim.SetTrigger("Dead");
        Invoke("DestroyEnemy", 3f); // 3초 후 DestroyEnemy 호출
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }


    void Think()
    {
        nextMove = Random.Range(-1, 2); // -1, 0, 1 중 하나
        float nextThinkTime = Random.Range(2f, 5f);

        Invoke("Think", nextThinkTime);
        anim.SetInteger("WalkSpeed", nextMove);

        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }
    }
}
