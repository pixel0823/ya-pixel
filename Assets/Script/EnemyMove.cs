using UnityEngine;

public class EnenmyMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseRange = 5f;
    public float attackRange = 1.5f;
    public int maxHealth = 100;
    private int currentHealth;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private enum State { Idle, Move, Attack, Hit, Dead }
    private State currentState = State.Move;

    private bool isFacingRight = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (currentState == State.Dead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Move:
                animator.SetBool("isMoving", true);
                if (distance < attackRange)
                {
                    currentState = State.Attack;
                }
                else if (distance < chaseRange)
                {
                    ChasePlayer();
                }
                else
                {
                    Patrol();
                }
                break;

            case State.Attack:
                animator.SetBool("isMoving", false);
                animator.SetBool("isAttacking", true);
                rb.linearVelocity = Vector2.zero;
                LookAtPlayer();

                if (distance > attackRange)
                {
                    animator.SetBool("isAttacking", false);
                    currentState = State.Move;
                }
                break;

            case State.Hit:
                break; // 애니메이션 이벤트 후 상태 전환

            case State.Idle:
                animator.SetBool("isMoving", false);
                break;
        }
    }

    private void Patrol()
    {
        // 간단한 좌우 패트롤
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        rb.linearVelocity = direction * moveSpeed;
        if (HitWall())
        {
            Flip();
        }
    }

    private void ChasePlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private bool HitWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, isFacingRight ? Vector2.right : Vector2.left, 0.2f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    public void TakeDamage(int damage)
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;
        animator.SetTrigger("isHit");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            currentState = State.Hit;
        }
    }

    public void OnHitEnd() // 애니메이션 이벤트에서 호출
    {
        currentState = State.Move;
    }

    private void Die()
    {
        currentState = State.Dead;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isDead", true);
        GetComponent<Collider2D>().enabled = false;
    }

    public void OnAttackHit() // 애니메이션 이벤트에서 공격 판정
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < attackRange)
        {
            // player.GetComponent<PlayerHealth>().TakeDamage(10); // 예시
        }
    }
}
