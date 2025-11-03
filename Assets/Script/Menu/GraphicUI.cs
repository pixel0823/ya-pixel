using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicUI : MonoBehaviour
{
    [Header("Screen Mode Controls")]
    public TMP_Dropdown screenModeDropdown;

    [Header("Resolution Settings")]
    public int windowedWidth = 1280;
    public int windowedHeight = 720;

    private void Start()
    {
        // Dropdown 옵션 설정
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();

            List<string> options = new List<string>
            {
                "FullScreen Mode",
                "Windowed Mode"
            };

            screenModeDropdown.AddOptions(options);

            // 현재 화면 모드에 따라 Dropdown 값 설정
            screenModeDropdown.value = Screen.fullScreen ? 0 : 1;
            screenModeDropdown.RefreshShownValue();

            // Dropdown 리스너 추가
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        }
    }

    // Dropdown 값 변경 시 호출
    private void OnScreenModeChanged(int index)
    {
        switch (index)
        {
            case 0: // 전체 화면
                if (!Screen.fullScreen)
                {
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    Screen.fullScreen = true;
                }
                break;

            case 1: // 창모드
                if (Screen.fullScreen)
                {
                    Screen.fullScreen = false;
                    Screen.SetResolution(windowedWidth, windowedHeight, false);
                }
                break;
        }
    }

    // 해상도 변경 함수 (필요시 사용)
    public void SetResolution(int width, int height)
    {
        windowedWidth = width;
        windowedHeight = height;

        if (!Screen.fullScreen)
        {
            Screen.SetResolution(width, height, false);
        }
    }

    private void OnDestroy()
    {
        // 리스너 제거
        if (screenModeDropdown != null)
        {
            screenModeDropdown.onValueChanged.RemoveListener(OnScreenModeChanged);
        }
    }
}
