using UnityEngine;

public enum MonsterState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Dead
}

public class BaseMonsters : MonoBehaviour
{
    protected Rigidbody2D rigid;
    protected Animator anim;
    protected SpriteRenderer spriteRenderer;

    [Header("몬스터 기본 능력치")]
    public float moveSpeed = 1f;
    public float chaseSpeed = 2f;
    public float detectRange = 5f;
    public float attackRange = 1f;
    public LayerMask playerLayer;
    
    protected Transform player;
    protected MonsterState state = MonsterState.Patrol;

    protected virtual void Aawake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        switch (state)
        {
            case MonsterState.Patrol:
                Patrol();
                DetectPlayer();
                break;
            case MonsterState.Chase:
                ChasePlayer();
                CheckAttackRange();
                break;
            case MonsterState.Attack:
                break;
            case MonsterState.Idle:
                Idle();
                DetectPlayer();
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
        if (distance < detectRange)
        {
            state = MonsterState.Chase;
        }
    }
    protected virtual void ChasePlayer()
    {
        if (player == null) return;
        float direction = player.position.x - transform.position.x;
        rigid.linearVelocity = new Vector2(Mathf.Sign(direction) * chaseSpeed, rigid.linearVelocityY);
        spriteRenderer.flipX = direction < 0;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > detectRange * 1.2f)
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
    protected virtual void Dead()
    {

    }

}
