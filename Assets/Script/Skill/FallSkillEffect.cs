using UnityEngine;


// 낙하 효과 스크립트
public class FallingSkillEffect : MonoBehaviour
{
    public float fallSpeed;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }
}

