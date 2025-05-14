using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Setting")]

    public float JumpForce;
    public float MoveSpeed;

    [Header("Reference")]

    public Rigidbody2D rigid;
    public bool isGrounded = true;
    private float MoveInput;
    SpriteRenderer spriteRenderer;
    Animator animator;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();  
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        if(rigid.linearVelocityX > MoveSpeed) // right max speed
        {
            rigid.linearVelocity = new Vector2(MoveSpeed, rigid.linearVelocityY);
        }
        else if (rigid.linearVelocityX < MoveSpeed*(-1))// left max speed
        {
            rigid.linearVelocity = new Vector2(MoveSpeed*(-1), rigid.linearVelocityY);
        }

        Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider != null)
        {
            if (rayHit.distance < 0.5f)
                {
                    //Debug.Log(rayHit.collider.name);
                }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded){
            rigid.AddForceY(JumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
        MoveInput = Input.GetAxisRaw("Horizontal"); 
        if (Input.GetButtonDown("Horizontal"))
            spriteRenderer.flipX = MoveInput == -1;

        if (Mathf.Abs(rigid.linearVelocityX) < 0.3)
            animator.SetBool("isWalking", false);
        else
            animator.SetBool("isWalking", true);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.name == "Tilemap"){
            isGrounded = true;
        }
    }
}
