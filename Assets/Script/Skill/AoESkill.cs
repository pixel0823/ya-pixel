using UnityEngine;

[CreateAssetMenu(fileName = "AoESkill", menuName = "SkillBase/AoESkill")]
public class AoESkill : SkillBase
{
    
    [Header("장판 설정")]
    //public float radius = 5f;      // 범위
    public float duration = 3f;    // 지속시간

    public override void Activate(GameObject player, Transform target = null)
    {
        if (target == null) return;
        Vector3 spawnPosition = target.position;
        GameObject aoe = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);
        Destroy(aoe, duration);
    }
}

