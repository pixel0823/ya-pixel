using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject escPanel;
    public GameObject settingPanel;

    [Header("Buttons")]
    public Button settingBtn;
    public Button exitBtn;
    public Button resumeBtn;

    private bool isEscUIOpen = false;

    private void Start()
    {
        Debug.Log("[EscUI] Start() 호출됨");

        // 초기 상태: EscUI와 SettingUI 모두 비활성화
        if (escPanel != null)
        {
            escPanel.SetActive(false);
            Debug.Log("[EscUI] escPanel 비활성화");
        }
        else
        {
            Debug.LogWarning("[EscUI] escPanel이 할당되지 않았습니다!");
        }

        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }

        // 버튼 리스너 추가
        if (settingBtn != null)
        {
            settingBtn.onClick.AddListener(OnSettingBtnClicked);
        }

        if (exitBtn != null)
        {
            exitBtn.onClick.AddListener(OnExitBtnClicked);
        }

        if (resumeBtn != null)
        {
            resumeBtn.onClick.AddListener(OnResumeBtnClicked);
        }
    }

    private void Update()
    {
        // ESC 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[EscUI] ESC 키 감지됨!");

            // SettingUI가 열려있으면 SettingUI만 닫기
            if (settingPanel != null && settingPanel.activeSelf)
            {
                Debug.Log("[EscUI] SettingPanel 닫기");
                CloseSettingPanel();
            }
            // SettingUI가 닫혀있으면 EscUI 토글
            else
            {
                Debug.Log("[EscUI] EscUI 토글");
                ToggleEscUI();
            }
        }
    }

    // ESC UI 토글
    private void ToggleEscUI()
    {
        isEscUIOpen = !isEscUIOpen;
        Debug.Log($"[EscUI] ToggleEscUI - isEscUIOpen: {isEscUIOpen}");

        if (escPanel != null)
        {
            escPanel.SetActive(isEscUIOpen);
            Debug.Log($"[EscUI] escPanel 상태: {isEscUIOpen}");
        }
        else
        {
            Debug.LogWarning("[EscUI] escPanel이 null입니다!");
        }

        // 게임 일시정지/재개 (필요하면 사용)
        Time.timeScale = isEscUIOpen ? 0f : 1f;
    }

    // 설정 버튼 클릭
    private void OnSettingBtnClicked()
    {
        Debug.Log("[EscUI] SettingBTN 클릭됨!");

        if (settingPanel != null)
        {
            settingPanel.SetActive(true);
            Debug.Log($"[EscUI] SettingPanel 활성화됨! 이름: {settingPanel.name}");
            Debug.Log($"[EscUI] SettingPanel Position: {settingPanel.transform.position}");

            // Canvas 계층 확인
            Canvas canvas = settingPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"[EscUI] SettingPanel의 Canvas: {canvas.name}, RenderMode: {canvas.renderMode}");
            }
            else
            {
                Debug.LogWarning("[EscUI] SettingPanel이 Canvas 안에 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("[EscUI] settingPanel이 null입니다!");
        }

        // EscUI는 숨기기 (선택사항)
        //if (escPanel != null)
        //{
            //escPanel.SetActive(false);
        //}
    }

    // 나가기 버튼 클릭 (Connection 씬으로 이동)
    private void OnExitBtnClicked()
    {
        // 게임 재개
        Time.timeScale = 1f;

        // Connection 씬으로 이동
        SceneManager.LoadScene("Connection");
    }

    // 계속하기 버튼 클릭
    private void OnResumeBtnClicked()
    {
        isEscUIOpen = false;

        if (escPanel != null)
        {
            escPanel.SetActive(false);
        }

        // 게임 재개
        Time.timeScale = 1f;
    }

    // SettingPanel 닫기 (SettingUI의 CloseSettings()가 호출할 수 있음)
    public void CloseSettingPanel()
    {
        if (settingPanel != null)
        {
            settingPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // 게임 재개 (혹시 모를 timeScale 문제 방지)
        Time.timeScale = 1f;

        // 리스너 제거
        if (settingBtn != null)
        {
            settingBtn.onClick.RemoveListener(OnSettingBtnClicked);
        }

        if (exitBtn != null)
        {
            exitBtn.onClick.RemoveListener(OnExitBtnClicked);
        }

        if (resumeBtn != null)
        {
            resumeBtn.onClick.RemoveListener(OnResumeBtnClicked);
        }
    }
}
