using UnityEngine;
using UnityEngine.EventSystems;

public class DarkMonster : BaseMonster
{

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private float patrolTimer = 0f;
    private float changeDirectionTime = 2f;
    private int moveDirection = -1;
    private bool isAttacking = false;
    private float attackCooldown = 1.5f;
    private float lastAttackTime = -Mathf.Infinity;


    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    protected override void FixedUpdate()
    {
        if (currentState == State.Dead) return;

        if (currentState == State.Attack)
        {
            if (!isAttacking)
            {
                Attack();
            }
            return;
        }
        base.FixedUpdate();

        if (currentState == State.Attack)
        {
            if (!isAttacking)
            {
                Attack();
            }
            else
            {
                if (IsAnimationFinished("Attack") || IsAnimationFinished("Attack") || IsAnimationFinished("Hit"))
                {
                    isAttacking = false;
                    currentState = State.Chase;
                }
            }
        }
        if (currentState == State.Patrol && PlayerInRange(5f))
        {
            MoveTowardsTarget();
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null || isAttacking) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x > 0;
        }
    }
    private bool IsAnimationFinished(string animationName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime >= 1.0f;
    }


    protected override void Attack()
    {

        if (isAttacking) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        int randomAttack = Random.Range(0, 10);


        animator.Play("Attack");
        Debug.Log("attack 2 공격");
        Invoke(nameof(BackToChase), 1.500f);

        Debug.Log("공격");


    }

    private void BackToChase()
    {
        isAttacking = false;
        currentState = State.Chase;
    }

    protected override void Chase()
    {
        if (target == null || isAttacking) return;
        float dist = Vector2.Distance(transform.position, target.position);
        Debug.Log($"[Chase] dist = {dist:F2}");

        if (dist <= 5f)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                currentState = State.Attack;
                Debug.Log("→ State changed to Attack");
                return;
            }
        }
        if (dist > 7f)
        {
            currentState = State.Patrol;
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        transform.Translate(dir * moveSpeed * Time.deltaTime);
        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x > 0;
    }

    protected override void Dead()
    {
        animator.Play("Dead");
        Debug.Log("사망");

    }

    protected override void Idle()
    {
        animator.Play("Idle");

    }

    protected override void Patrol()
    {
        animator.Play("Move");

        patrolTimer += Time.deltaTime;
        if (patrolTimer >= changeDirectionTime)
        {
            moveDirection *= -1;
            patrolTimer = 0f;
        }

        Vector2 moveDir = new Vector2(moveDirection, 0);
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDir.x > 0;
        }
        if (PlayerInRange(3f)) currentState = State.Chase;
    }

    protected override void Hit()
    {
        Debug.Log("몬스터 피격");
        animator.Play("Hit");

        CancelInvoke(nameof(BackToChase));
        Invoke(nameof(BackToChase), 0.5f);


    }
    public void TakeDamage(float damage)
    {
        if (currentState == State.Dead) return;

        hp -= damage;

        if (hp > 0)
        {
            currentState = State.Hit;
        }
        else
        {
            currentState = State.Dead;
        }
    }
}
