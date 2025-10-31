using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingPanel;

    public void OnClickOnlineButton()
    {
        Debug.Log("Click Online");
    }

    public void OnClickSettingButton()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
        }
    }

    public void OnClickQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
