using UnityEngine;

[CreateAssetMenu(fileName = "FallSkill", menuName = "SkillBase/FallSkill")]
public class FallSkill : SkillBase
{
    [Header("낙하 설정")]
    public float height = 2f;      // 생성 높이
    public float fallSpeed = 20f;  // 낙하 속도
    public float duration = 3f;    // 지속 시간
    public float areaRadius = 0f;  // 낙하 충돌 범위(0이면 단일 타겟)
    public float damage = 10f;

    public override void Activate(GameObject player, Transform target = null)
    {
        if (target == null) return;
        Vector3 spawnPosition = target.position + Vector3.up * height;
        GameObject fallingObj = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

        // 낙하 효과 부여
        var fallEffect = fallingObj.AddComponent<FallingSkillEffect>();
        fallEffect.fallSpeed = fallSpeed;

        // 데미지 딜러 부착 및 파라미터 전달
        var dealer = fallingObj.AddComponent<SkillDamageDealer>();
        dealer.damage = damage;
        dealer.areaRadius = areaRadius;

        Destroy(fallingObj, duration);
    }
}
