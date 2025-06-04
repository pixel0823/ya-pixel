using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillManager : MonoBehaviour
{
    public GameObject skillButtonPrefab;
    public GameObject sSkillButtonPrefab;
    [HideInInspector] public Transform skillButtonParent; // 외부에서 할당
    [HideInInspector] public Transform sSkillButtonParent;
    public List<string> selectedSkills;

    string[] skillColors = { "Red", "Orange", "Yellow", "Green", "Blue", "Navy", "Purple" };

    public void ShowAllSkills()
    {
        foreach (Transform child in skillButtonParent)
            Destroy(child.gameObject);

        foreach (string color in skillColors)
        {
            GameObject btn = Instantiate(skillButtonPrefab, skillButtonParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = color;
            btn.GetComponent<Image>().color = GetColorFromName(color);
            btn.GetComponentInChildren<TextMeshProUGUI>().text += GetSkillDescription(color);

            string capturedColor = color;
            if (selectedSkills.Contains(color))
            {
                btn.GetComponent<Button>().interactable = false;
                btn.GetComponentInChildren<TextMeshProUGUI>().text += " (획득)";
            }
            else
            {
                btn.GetComponent<Button>().onClick.AddListener(() => OnSkillSelected(capturedColor));
            }
        }
    }
    
    public void ShowSelectedSkills()
    {
        // 기존에 생성된 선택된 스킬 슬롯(버튼) 모두 삭제
        foreach (Transform child in sSkillButtonParent)
            Destroy(child.gameObject);

        // 최대 4개까지만 생성
        int count = Mathf.Min(selectedSkills.Count, 4);
        for (int i = 0; i < count; i++)
        {
            string color = selectedSkills[i];
            GameObject btn = Instantiate(sSkillButtonPrefab, sSkillButtonParent);
            btn.GetComponent<Image>().color = GetColorFromName(color);
        }
    }



    void OnSkillSelected(string skillColor)
    {
        selectedSkills.Add(skillColor);

        // 태그가 "SkillPanel"인 오브젝트를 찾아서 삭제
        GameObject panel = GameObject.FindWithTag("SkillPanel");
        if (panel != null)
        {
            Destroy(panel);

        }
        ShowSelectedSkills();
    }

    Color GetColorFromName(string colorName)
    {
        switch (colorName)
        {
            case "Red": return Color.red;
            case "Orange": return new Color(1f, 0.5f, 0f);
            case "Yellow": return Color.yellow;
            case "Green": return Color.green;
            case "Blue": return Color.blue;
            case "Navy": return new Color(0f, 0f, 0.5f);
            case "Purple": return new Color(0.5f, 0f, 0.5f);
            default: return Color.white;
        }
    }
    
    string GetSkillDescription(string colorName)
    {
        switch (colorName)
        {
            case "Red":    return "\n레드";
            case "Orange": return "\n오렌지";
            case "Yellow": return "\n옐로우";
            case "Green":  return "\n그린";
            case "Blue":   return "\n블루";
            case "Navy":   return "\n네이비";
            case "Purple": return "\n퍼플";
            default:       return "\n알 수 없는 스킬입니다.";
        }
    }

}
