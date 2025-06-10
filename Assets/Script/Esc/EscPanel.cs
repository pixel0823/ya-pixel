using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject panelPrefab; // 패널 프리팹
    public GameObject buttonPrefab; // 버튼 프리팹
    public Transform canvasTransform; // 패널이 붙을 캔버스

    private GameObject currentPanel; // 현재 열린 패널

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPanel == null)
            {
                // 패널 생성
                currentPanel = Instantiate(panelPrefab, canvasTransform);
                // 버튼 생성 예시 (3개)
                for (int i = 0; i < 3; i++)
                {
                    GameObject button = Instantiate(buttonPrefab, currentPanel.transform);
                    // 버튼 위치 조정 (예시)
                    RectTransform rt = button.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0, -50 * (i + 1));
                    // 버튼 텍스트 변경
                    button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "버튼 " + (i + 1);
                    // 버튼 클릭 이벤트 추가
                    int index = i;
                    button.GetComponent<Button>().onClick.AddListener(() => OnButtonClick(index));
                }
            }
            else
            {
                // 패널 닫기
                Destroy(currentPanel);
                currentPanel = null;
            }
        }
    }

    void OnButtonClick(int index)
    {
        Debug.Log("버튼 " + (index + 1) + " 클릭됨");
    }
}
