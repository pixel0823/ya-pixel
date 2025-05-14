using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSkill", menuName = "SkillBase/ProjectileSkill")]
public class ProjectileSkill : SkillBase
{
    
    [Header("발사체 설정")]
    public float speed = 10f;  // 발사체 속도

    public override void Activate(GameObject player, Transform target = null)
    {
        if (target == null) return;
        GameObject projectile = Instantiate(effectPrefab, target.position, target.rotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = target.right * speed; // 캐릭터가 보는 방향으로 속도 부여
        }
    }
}


