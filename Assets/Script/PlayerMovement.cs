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

    private float xInput = 0f; // 좌우 입력 저장

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        PlayerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // 좌우 입력 저장
        xInput = Input.GetAxisRaw("Horizontal");

        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            PlayerAnimator.SetInteger("state", 2);
        }

        // 대쉬 (LeftShift, 쿨타임 적용)
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f)
        {
            StartDash(xInput);
            dashCooldownTimer = dashCooldown;
        }

        // 쿨타임 타이머
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // 대쉬 중이면 일반 이동 안 함
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

        // 좌우 이동
        rigid.linearVelocity = new Vector2(xInput * speed, rigid.linearVelocity.y);

        // 방향에 따라 캐릭터 뒤집기
        if (xInput > 0)
            transform.localScale = new Vector3(1, 1, 1); // 오른쪽 방향
        else if (xInput < 0)
            transform.localScale = new Vector3(-1, 1, 1); // 왼쪽 방향
        
        // 애니메이션 상태 설정
        if (!isGrounded)
        {
            // 공중에 있을 때
            if (rigid.linearVelocity.y < -0.1f)
                PlayerAnimator.SetInteger("state", 4); // Fall
        }
        else
        {
            // 땅에 있을 때
            if (Mathf.Abs(xInput) > 0.1f)
                PlayerAnimator.SetInteger("state", 1); // Run
            else
                PlayerAnimator.SetInteger("state", 0); // Idle
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
            // 착지시 Land 애니메이션
            PlayerAnimator.SetInteger("state", 5); // Land
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.name == "Tilemap"){
        isGrounded = false; // 여기도 수정 필요 - 땅에서 벗어나는 것이므로 false로 설정
        
        // 지상에서 떨어질 때
        if (rigid.linearVelocity.y < 0)
            PlayerAnimator.SetInteger("state", 4); // Fall
        else
            PlayerAnimator.SetInteger("state", 3); // Transition
        }
    }
}
