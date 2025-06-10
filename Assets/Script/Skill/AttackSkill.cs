using UnityEngine;

[CreateAssetMenu(fileName = "AreaAttackSkill", menuName = "SkillBase/AreaAttackSkill")]
public class AttackSkill : SkillBase
{
    public float areaRadius = 2.5f;
    public int damage = 10;

    public override void Activate(GameObject player, Transform target = null)
    {
        // static 클래스의 메서드 직접 호출
        GameObject closest = FindClosestEnemy.FindClosestEnemyObject(player.transform.position);
        if (closest == null) return;

        Vector2 center = closest.transform.position;

        if (effectPrefab != null)
            GameObject.Instantiate(effectPrefab, target.position, Quaternion.identity);

        // Collider2D[] hits = Physics2D.OverlapCircleAll(center, areaRadius);
        // foreach (var hit in hits)
        // {
        //     if (hit.CompareTag("Enemy"))
        //     {
        //         EnemyHp enemy = hit.GetComponent<EnemyHp>();
        //         if (enemy != null)
        //             enemy.TakeDamage(damage);
        //     }
        // }
    }
}
