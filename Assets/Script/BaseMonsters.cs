using Unity.VisualScripting;
using UnityEngine;

public enum MonsterState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Dead,
    Dodge
}

public class BaseMonsters : MonoBehaviour
{
    protected Rigidbody2D rigid;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;

    [Header("몬스터 기본 능력치")]
    public float EnemyHp = 3f;
    public float moveSpeed = 1f;
    public float chaseRange = 5f;
    public float dodgeDistance = 5f;
    public float attackRange = 1f;
    public LayerMask playerLayer;
    
    protected Transform player;
    protected MonsterState state = MonsterState.Patrol;

    [SerializeField] protected Collider2D attackHitbox;

    protected int nextMove = 0;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (attackHitbox != null) attackHitbox.enabled = false;

    }

    protected virtual void Update()
    {
        if (state != MonsterState.Dead)
        {
            StateHandler();
        }
    }

    protected virtual void StateHandler()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case MonsterState.Patrol:
                Patrol();
                if (distanceToPlayer < chaseRange) state = MonsterState.Chase;
                break;
            case MonsterState.Chase:
                ChasePlayer();
                if (distanceToPlayer < attackRange) state = MonsterState.Attack;
                else if (distanceToPlayer > chaseRange) state = MonsterState.Patrol;
                break;
            case MonsterState.Attack:
                CheckAttackRange();
                if (distanceToPlayer > attackRange) state = MonsterState.Chase;
                break;
            case MonsterState.Dodge:
                rigid.linearVelocity = Vector2.zero;
                break;
                
        }
    }

    protected virtual void Idle()
    {
        rigid.linearVelocity = Vector2.zero;
        anim.SetInteger("WalkSpeed", 0);
    }
    protected virtual void Patrol()
    {
        rigid.linearVelocity = new Vector2(Random.Range(-1, 2) * moveSpeed, rigid.linearVelocityY);
    }
    protected virtual void DetectPlayer()
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < dodgeDistance)
        {
            state = MonsterState.Chase;
        }
    }
    protected virtual void ChasePlayer()
    {
        if (player == null) return;
        float direction = player.position.x - transform.position.x;
        rigid.linearVelocity = new Vector2(Mathf.Sign(direction) * chaseRange, rigid.linearVelocityY);
        spriteRenderer.flipX = direction < 0;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > dodgeDistance * 1.2f)
        {
            state = MonsterState.Idle;
        }
    }
    protected virtual void CheckAttackRange()
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < attackRange)
        {
            state = MonsterState.Attack;
            StartCoroutine(AttackRoutine());
        }
    }
    protected virtual System.Collections.IEnumerator AttackRoutine()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);

        state = MonsterState.Chase;
    }

    protected virtual void Dodge()
    {
        Vector2 dodgeDir = (transform.position - player.position).normalized;
        rigid.AddForce(dodgeDir * dodgeDistance, ForceMode2D.Impulse);
        anim.SetTrigger("Dodge");
    }

    protected void Think()
    {
        nextMove = Random.Range(-1, 2);
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    protected void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove != 1;

        CancelInvoke();
        Invoke("Think", 2f);
    }
    protected virtual void Dead()
    {
        state = MonsterState.Dead;
        anim.SetTrigger("Die");

        rigid.linearVelocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 1f);
    }

}
