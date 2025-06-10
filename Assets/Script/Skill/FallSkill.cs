using UnityEngine;

[CreateAssetMenu(fileName = "FallSkill", menuName = "SkillBase/FallSkill")]
public class FallSkill : SkillBase
{
    
    [Header("낙하 설정")]
    public float height = 2f;     // 생성 높이
    public float fallSpeed = 20f;  // 낙하 속도
    public float duration = 3f;    //지속 시간


    public override void Activate(GameObject player, Transform target = null)
    {
        if (target == null) return;
        Vector3 spawnPosition = target.position + Vector3.up * height;
        GameObject fallingObj = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);
        fallingObj.AddComponent<FallingSkillEffect>().fallSpeed = fallSpeed;
        Destroy(fallingObj, duration);
    }
}


