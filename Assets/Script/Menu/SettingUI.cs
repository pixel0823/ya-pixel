using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [Header("Setting Panels")]
    public GameObject soundPanel;
    public GameObject graphicPanel;

    private void Start()
    {
        // 시작할 때 Sound 패널만 활성화
        ShowSoundPanel();
    }

    public void ShowSoundPanel()
    {
        if (soundPanel != null)
        {
            soundPanel.SetActive(true);
        }

        if (graphicPanel != null)
        {
            graphicPanel.SetActive(false);
        }
    }

    public void ShowGraphicPanel()
    {
        if (soundPanel != null)
        {
            soundPanel.SetActive(false);
        }

        if (graphicPanel != null)
        {
            graphicPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        // 설정창 닫기 (필요하면 사용)
        gameObject.SetActive(false);
    }
}
