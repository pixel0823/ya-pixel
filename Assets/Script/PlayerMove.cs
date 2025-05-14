using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float maxSpeed;
    Rigidbody2D rigid;
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.position.x > maxSpeed) 
            rigid.position = new Vector2(maxSpeed, rigid.position.y);
        else if (rigid.position.x < maxSpeed*(-1))
            rigid.position = new Vector2(maxSpeed*(-1), rigid.position.y);
    }
}
