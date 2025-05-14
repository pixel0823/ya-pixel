/*using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Header("스킬 리스트")]
    public List<SkillBase> skills = new List<SkillBase>(); // 동적 추가/삭제 가능

    [Header("발사 위치")]
    public Transform skillSpawnPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && skills.Count > 0)
            skills[0].Activate(gameObject);

        if (Input.GetKeyDown(KeyCode.Alpha2) && skills.Count > 1)
            skills[1].Activate(gameObject);

        if (Input.GetKeyDown(KeyCode.Alpha3) && skills.Count > 2)
            skills[2].Activate(gameObject);
    }
    
    // 스킬 추가 함수
    public void AddSkill(SkillBase newSkill)
    {
        if (!skills.Contains(newSkill))
        {
            skills.Add(newSkill);
            Debug.Log($"{newSkill.skillName} 스킬을 획득했습니다!");
        }
        else
        {
            Debug.Log($"{newSkill.skillName} 스킬은 이미 보유 중입니다.");
        }
    }
}*/
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public SkillBase[] skills; // Inspector에서 ScriptableObject 에셋 할당
    public Transform projectilePoint;


    void Update()
    {
        Transform enemy = FindEnemyTransform();

        if (Input.GetKeyDown(KeyCode.Alpha1) && skills.Length > 0)
            skills[0].Activate(gameObject, projectilePoint);

        if (Input.GetKeyDown(KeyCode.Alpha2) && skills.Length > 1)
            skills[1].Activate(gameObject, enemy);

        if (Input.GetKeyDown(KeyCode.Alpha3) && skills.Length > 2)
            skills[2].Activate(gameObject, enemy);

         // 타겟(보스/몬스터) Transform을 찾는 함수 예시
        Transform FindEnemyTransform()
        {
            // 보스가 Boss라는 태그를 가지고 있다고 가정
            GameObject enemyObj = GameObject.FindGameObjectWithTag("Enemy");
            return enemyObj != null ? enemyObj.transform : null;
        }
    }
}

