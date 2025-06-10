using UnityEngine;

public abstract class BaseMonster : MonoBehaviour, IDamageable
{
    public float hp = 100f;
    public float moveSpeed = 2f;
    public Transform target;
    public int nextMove;
    protected Animator animator;
    SpriteRenderer spriteRenderer;

    protected enum State { Idle, Patrol, Chase, Attack, Dead, Hit }
    protected State currentState;

    protected virtual void Start()
    {
        currentState = State.Patrol;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        animator = GetComponent<Animator>();
    }

    protected virtual void FixedUpdate()
    {

        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: break;
            case State.Hit: Hit(); break;
            case State.Dead: Dead(); break;
        }

        if (hp <= 0 && currentState != State.Dead)
        {
            currentState = State.Dead;
        }
    }

    protected abstract void Idle();
    protected abstract void Patrol();
    protected abstract void Chase();
    protected abstract void Attack();
    protected abstract void Hit();

    protected bool PlayerInRange(float range)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= range;
    }

    void Think()
    {
        nextMove = Random.Range(-1, 2);
        float nextThinkTime = Random.Range(2f, 5f);


        Invoke("Think", nextThinkTime);
        animator.SetInteger("WalkSpeed", nextMove);

        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            currentState = State.Dead;
        }
    }

    protected virtual void Dead()
    {
        currentState = State.Dead;
        animator.SetTrigger("Die");

        if (TryGetComponent<Rigidbody2D>(out var rigid))
        {
            rigid.linearVelocity = Vector2.zero;
            rigid.bodyType = RigidbodyType2D.Kinematic;
        }
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }

        Destroy(gameObject, 1f);
    }

}
