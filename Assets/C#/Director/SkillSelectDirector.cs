using System.Collections.Generic;
using UnityEngine;

public class SkillSelectDirector : MonoBehaviour
{
    public GameObject skillManagerPanelPrefab; // SkillManager 프리팹
    public GameObject skillSelectedPanelPrefab; //SkillSelectedPanel 프리팹
    public Transform canvasTransform;          // Canvas 오브젝트
    public SkillManager skillManager;        // ColorSelect 오브젝트의 SkillManager

    public List<string> selectedSkills = new List<string>();



    void Start()
    {
        ShowSkillSelectPanel();
        ShowSkillSelectedPanel();
    }

    public void ShowSkillSelectPanel()
    {
        // 1. 패널 프리팹 인스턴스화 (Canvas의 자식으로)
        GameObject skillPanel = Instantiate(skillManagerPanelPrefab, canvasTransform);

        // 태그가 "Panel"인 자식 오브젝트를 찾음
        Transform skillButtonParent = null;
        foreach (Transform child in skillPanel.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("SkillPanel"))
            {
                skillButtonParent = child;
                break;
            }
        }

        // 3. 씬에 이미 존재하는 SkillManager에 skillButtonParent와 selectedSkills 할당
        skillManager.skillButtonParent = skillButtonParent;
        skillManager.selectedSkills = selectedSkills;

        // 4. 스킬 버튼 생성
        skillManager.ShowAllSkills();
    }
    public void ShowSkillSelectedPanel()
    {
        // 1. 패널 프리팹 인스턴스화 (Canvas의 자식으로)
        GameObject sSkillPanel = Instantiate(skillSelectedPanelPrefab, canvasTransform);

        // 태그가 "Panel"인 자식 오브젝트를 찾음
        Transform sSkillButtonParent = null;
        foreach (Transform child in sSkillPanel.GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("SelectedSkillPanel"))
            {
                sSkillButtonParent = child;
                break;
            }
        }

        // 3. 씬에 이미 존재하는 SkillManager에 skillButtonParent와 selectedSkills 할당
        skillManager.sSkillButtonParent = sSkillButtonParent;
        skillManager.selectedSkills = selectedSkills;

        // 4. 스킬 버튼 생성
        skillManager.ShowSelectedSkills();
    }

}
