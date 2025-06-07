using UnityEngine;

[CreateAssetMenu(fileName = "SkillBase", menuName = "Scriptable Objects/SkillBase")]
public abstract class SkillBase : ScriptableObject
{
    [Header("공통 설정")]
    public string skillName;
    public Sprite icon;
    public float cooldown;
    public float manaCost;
    public GameObject effectPrefab;

    // 플레이어가 사용할 때 호출
    public abstract void Activate(GameObject player, Transform target);
}
