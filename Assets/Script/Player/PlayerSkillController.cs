using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public SkillManager skillManager;
    public List<string> selectedSkills;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) UseSkillByIndex(0);
        if (Input.GetKeyDown(KeyCode.X)) UseSkillByIndex(1);
        if (Input.GetKeyDown(KeyCode.C)) UseSkillByIndex(2);
        if (Input.GetKeyDown(KeyCode.V)) UseSkillByIndex(3);
    }

    void UseSkillByIndex(int idx)
    {
        if (skillManager != null && idx < selectedSkills.Count)
        {
            
            string skillColor = selectedSkills[idx];
            // 플레이어 오브젝트와 타겟 Transform을 넘김
            skillManager.UseSkill(skillColor, gameObject);
        }
    }
}
