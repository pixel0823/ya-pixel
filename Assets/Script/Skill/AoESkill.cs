using UnityEngine;

[CreateAssetMenu(fileName = "AoESkill", menuName = "SkillBase/AoESkill")]
public class AoESkill : SkillBase
{

    [Header("장판 설정")]
    public float areaRadius = 2.5f; // 범위
    public float duration = 3f;
    public float damage = 10f;

    public override void Activate(GameObject player, Transform target = null)
    {
        if (target == null) return;
        Vector3 spawnPosition = target.position;
        GameObject aoe = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

        // 데미지 딜러 컴포넌트에 범위 값 전달
        var dealer = aoe.AddComponent<SkillDamageDealer>();
        dealer.damage = damage;
        dealer.areaRadius = areaRadius;

        Destroy(aoe, duration);
    }

}

