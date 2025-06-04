using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMove : MonoBehaviour
{

    public enum State { Idle, Patrol, Chase, Attack, Dodge, Dead }
    private State currentState = State.Patrol;
    public EnemyGenerator generator;
    
    public float hp = 3f;
    public float moveSpeed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1f;
    public float dodgeDistance = 2f;

    private Rigidbody2D rigid;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    [SerializeField] private Collider2D attackHitbox;
    

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (attackHitbox != null) attackHitbox.enabled = false;

        Invoke("Think", Random.Range(2f, 5f));
    }

    void Update()
    {
        if (currentState != State.Dead)
        {
            StateHandler();
        }   
    }

    void EnableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = true;
    }

    void DisableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    void StateHandler()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (distanceToPlayer < chaseRange)
                {
                    currentState = State.Chase;
                }
                break;
            case State.Chase:
                chasePlayer();
                if (distanceToPlayer < attackRange)
                {
                    currentState = State.Attack;
                }
                else if (distanceToPlayer > chaseRange)
                {
                    currentState = State.Patrol;
                }
                break;
            case State.Attack:
                AttackPlayer();
                if (distanceToPlayer > attackRange)
                {
                    currentState = State.Chase;
                }
                break;
            case State.Dodge:
                Dodge();
                currentState = State.Patrol;
                break;
            case State.Idle:
                rigid.linearVelocity = Vector2.zero;
                break;
        }
    }

    void Patrol()
    {
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocityY);

        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.2f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, Color.green);
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if (rayHit.collider == null)
        {
            Turn();
        }

        anim.SetInteger("WalkSpeed", nextMove);
        spriteRenderer.flipX = nextMove != 1;
    }

    void chasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rigid.linearVelocity = new Vector2(direction.x * moveSpeed, rigid.linearVelocityY);
        spriteRenderer.flipX = direction.x < 0;
        anim.SetInteger("WalkSpeed", (int)Mathf.Sign(direction.x));
    }

    void AttackPlayer()
    {
        anim.SetTrigger("Attack");
        rigid.linearVelocity = Vector2.zero;
    }

    void Dodge()
    {
        Vector2 dodgeDir = (transform.position - player.position).normalized;
        rigid.AddForce(dodgeDir * dodgeDistance, ForceMode2D.Impulse);
        anim.SetTrigger("Dodge");
    }

    int nextMove = 0;

    void Think()
    {
        nextMove = Random.Range(-1, 2);
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove != 1;

        CancelInvoke();
        Invoke("Think", 2);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack") && currentState != State.Dead)
        {
            hp--;
            if (hp <= 0)
            {
                Die();
            }
            else
            {
                currentState = State.Dodge;
            }

            if (attackHitbox != null && attackHitbox.enabled && other.CompareTag("Player"))
            {
                // other.GetComponent<PlayerHp>()?.TackeDamage(1);
            }
        }
    }

    void Die()
    {
        currentState = State.Dead;
        anim.SetTrigger("Die");

        rigid.linearVelocity = Vector2.zero;
        rigid.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 1f);

        if (gameObject.CompareTag("Boss"))
        {
            //generator.GetComponent<BossGenerator>().OnBossDefeated();
        }
        else
        {
            generator.OnEnemyDefeated();
        }
    }
}
