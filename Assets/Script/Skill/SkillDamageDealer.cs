using UnityEngine;

public class SkillDamageDealer : MonoBehaviour
{
    public float damage;
    public float areaRadius = 0f; // 0이면 단일 타겟

    private void Start()
    {
        if (areaRadius > 0f)
        {
            // 범위 공격: 생성 시점에 주변 적 탐색 후 데미지
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Monster"))
                {
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null)
                        damageable.TakeDamage(damage);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (areaRadius > 0f) return;
        if (collision.CompareTag("Monster"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);
        }
    }

}
