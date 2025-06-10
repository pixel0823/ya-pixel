using UnityEngine;

public enum State { Idle, Patrol, Chase, Attack, Dead }

public class EnemyMove : MonoBehaviour
{
    public float hp = 100f;
    public float moveSpeed = 2f;
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

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Patrol: Patrol(); break;
            case State.Attack: Attack(); break;
            case State.Chase: Chase(); break;
            //case State.Dead: Dead(); break;
        }

        if (hp <= 0 && currentState != State.Dead)
        {
            currentState = State.Dead;
        }


    }
    public void Chase()
    {
        if (target == null) return;
        float dist = Vector2.Distance(transform.position, target.position);

        if (dist <= 3f)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                currentState = State.Attack;
                return;
            }
            if (dist > 4f)
            {
                currentState = State.Patrol;
                return;
            }


        }
    }

    // 플레이어 공격에서 호출할 TakeDamage 함수
    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead) return;
        
        hp -= damage;
        hp = Mathf.Clamp(hp, 0f, 100f);
        
        Debug.Log($"{gameObject.name}이(가) {damage} 데미지를 받았습니다. 현재 HP: {hp}");
        
        // 피격 시 플레이어를 추적하도록 상태 변경
        if (currentState == State.Patrol || currentState == State.Idle)
        {
            currentState = State.Chase;
        }
        
        //if (hp <= 0)
        //{
            //Die();
        //}
    }

   
    public void Attack()
    {
        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        rigid.linearVelocity = Vector2.zero;

        if (!PlayerInRange(3f))
        {
            currentState = State.Patrol;
            return;
        }

        isAttacking = true;
        lastAttackTime = Time.time;

        if (target != null)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            spriteRenderer.flipX = dir.x > 0;
            anim.SetBool("isAttacking", isAttacking);
            Invoke("BackToChase", 0.25f);
        }
    }

    void ActivateMonsterAttack()
    {
        MonsterAttackCollider attackCollider = GetComponentInChildren<MonsterAttackCollider>();
        if (attackCollider != null)
        {
            attackCollider.SetDamage(10f); // 몬스터 공격력
            attackCollider.StartAttack();
            
            // 0.2초 후 비활성화
            Invoke("DeactivateMonsterAttack", 0.2f);
        }
    }

    void DeactivateMonsterAttack()
    {
        MonsterAttackCollider attackCollider = GetComponentInChildren<MonsterAttackCollider>();
        if (attackCollider != null)
        {
            attackCollider.EndAttack();
        }
    }

    private void BackToChase()
    {
        isAttacking = false;
        if (PlayerInRange(3f))
        {
            currentState = State.Chase;
            anim.SetBool("isAttacking", isAttacking);
        }
        else
        {
            currentState = State.Patrol;
            anim.SetBool("isAttacking", isAttacking);
        }
    }

    public void Patrol()
    {
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocityY);
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove, rigid.position.y);
        float rayLength = 3f;
        Debug.DrawRay(frontVec, Vector3.down * rayLength, Color.green);
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, rayLength, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {
            nextMove *= -1;
            spriteRenderer.flipX = nextMove == 1;
            CancelInvoke();
            Invoke("Think", 5);
            Debug.Log("턴");
        }

        Vector2 moveDir = new Vector2(nextMove, 0);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
        if (PlayerInRange(3f)) currentState = State.Chase;



    }

    public bool PlayerInRange(float range)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= range;
    }

    public void Idle()
    {

    }

    void Think()
    {
        nextMove = Random.Range(-1, 2);
        float nextThinkTime = Random.Range(2f, 5f);


        Invoke("Think", nextThinkTime);
        anim.SetInteger("WalkSpeed", nextMove);

        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }
    }
}
