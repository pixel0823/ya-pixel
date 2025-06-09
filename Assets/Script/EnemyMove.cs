using UnityEngine;

public class EnenmyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    public int nextMove;

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
