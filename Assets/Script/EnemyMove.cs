using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public int nextMove;

    
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Invoke("Think", 5);
    }

    void FixedUpdate()
    {
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocityY);

        Vector2 frontVec = new Vector2(rigid.position.x + nextMove*0.2f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if (rayHit.collider == null)
        {
            Turn();
        }

    }
    
    void Think()
    {
        nextMove = Random.Range(-1, 2);
 

        anim.SetInteger("WalkSpeed", nextMove);
        if (nextMove != 0)
            spriteRenderer.flipX = nextMove!=1;

        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove!=1;

        CancelInvoke();
        Invoke("Think", 2);
    }
}
