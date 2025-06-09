using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpPower = 7f;
    public float dashPower = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 10f;

    public Animator PlayerAnimator;

    private Rigidbody2D rigid;
    private bool isGrounded = false;
    private int jumpCount = 0;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private float xInput = 0f;

    // 콤보 공격 관련 변수
    private int clickCount = 0;
    private float clickTimer = 0f;
    private float comboDelay = 0.5f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        PlayerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            PlayerAnimator.SetInteger("state", 2);
        }

        // 대쉬 입력
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f)
        {
            StartDash(xInput);
            dashCooldownTimer = dashCooldown;
            PlayerAnimator.SetInteger("state", 6);
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // 콤보 공격 입력
        if (Input.GetKeyDown(KeyCode.Z))
        {
            clickCount++;
            clickTimer = comboDelay;

            PlayerAnimator.SetInteger("attackClick", clickCount);

            if (clickCount == 1)
                PlayerAnimator.Play("attack1"); // 애니메이션 이름은 Animator에 맞게 설정
        }

        if (clickTimer > 0)
        {
            clickTimer -= Time.deltaTime;
        }
        else
        {
            // 시간 초과 시 초기화
            clickCount = 0;
            PlayerAnimator.SetInteger("attackClick", 0);
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer += Time.fixedDeltaTime;
            if (dashTimer >= dashDuration)
            {
                isDashing = false;
                dashTimer = 0f;
            }
            return;
        }

        rigid.linearVelocity = new Vector2(xInput * speed, rigid.linearVelocity.y);

        if (xInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (xInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        if (!isGrounded)
        {
            if (rigid.linearVelocity.y < -0.1f)
                PlayerAnimator.SetInteger("state", 4);
        }
        else
        {
            if (Mathf.Abs(xInput) > 0.1f)
                PlayerAnimator.SetInteger("state", 1);
            else
                PlayerAnimator.SetInteger("state", 0);
        }
    }

    void StartDash(float direction)
    {
        if (direction == 0f)
            direction = transform.localScale.x >= 0 ? 1f : -1f;

        rigid.linearVelocity = new Vector2(direction * dashPower, 0f);
        isDashing = true;
        dashTimer = 0f;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            isGrounded = true;
            jumpCount = 0;
            PlayerAnimator.SetInteger("state", 5);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Tilemap")
        {
            isGrounded = false;

            if (rigid.linearVelocity.y < 0)
                PlayerAnimator.SetInteger("state", 4);
            else
                PlayerAnimator.SetInteger("state", 3);
        }
    }
}
