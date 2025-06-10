using UnityEngine;

[CreateAssetMenu(fileName = "AreaAttackSkill", menuName = "SkillBase/AreaAttackSkill")]
public class AttackSkill : SkillBase
{
    public float areaRadius = 2.5f;
    public float damage = 10f;

    public override void Activate(GameObject player, Transform target = null)
    {
        // 플레이어 주변에서 가장 가까운 적 위치를 중심으로 이펙트 생성
        GameObject closest = FindClosestEnemy.FindClosestEnemyObject(player.transform.position);
        if (closest == null) return;

        Vector3 center = closest.transform.position;

        if (effectPrefab != null)
        {
            GameObject effect = GameObject.Instantiate(effectPrefab, center, Quaternion.identity);

            // SkillDamageDealer를 동적으로 추가하고 파라미터 전달
            var dealer = effect.AddComponent<SkillDamageDealer>();
            dealer.damage = damage; // SkillBase의 damage 사용
            dealer.areaRadius = areaRadius;
        }
    }
}
