using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Biome 선택 UI를 관리합니다.
/// 미리 만든 버튼 2개에 각각 Biome을 지정하고, 선택 시 순간이동합니다.
/// </summary>
public class BiomeSelectionUI : MonoBehaviour
{
    [Header("UI 레퍼런스")]
    [Tooltip("UI가 포함된 Canvas 또는 Panel GameObject (PortalPanel)")]
    public GameObject uiPanel;

    [Header("Biome 버튼 설정")]
    [Tooltip("첫 번째 Biome 버튼")]
    public Button biomeButton1;

    [Tooltip("첫 번째 버튼에 할당할 Biome 인덱스 (MapManager.biomes[0], [1], [2]...)")]
    public int biome1Index = 0;

    [Tooltip("두 번째 Biome 버튼")]
    public Button biomeButton2;

    [Tooltip("두 번째 버튼에 할당할 Biome 인덱스")]
    public int biome2Index = 1;

    [Tooltip("세 번째 Biome 버튼")]
    public Button biomeButton3;

    [Tooltip("세 번째 버튼에 할당할 Biome 인덱스")]
    public int biome3Index = 2;

    [Tooltip("네 번째 Biome 버튼")]
    public Button biomeButton4;

    [Tooltip("네 번째 버튼에 할당할 Biome 인덱스")]
    public int biome4Index = 3;


    [Header("설정")]
    [Tooltip("MapManager 참조")]
    public MapManager mapManager;

    private GameObject currentPlayer; // 현재 상호작용 중인 플레이어
    private TeleportManager teleportManager;

    void Start()
    {
        // TeleportManager 찾기 (비활성화된 오브젝트도 포함)
        teleportManager = FindObjectOfType<TeleportManager>(true);
        if (teleportManager == null)
        {
            Debug.LogError("[BiomeSelectionUI] TeleportManager를 찾을 수 없습니다. Scene에 TeleportManager가 있는지 확인하세요.");
        }

        // MapManager 자동 찾기 (비활성화된 오브젝트도 포함)
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>(true);
        }

        // 버튼 이벤트 등록
        if (biomeButton1 != null)
        {
            biomeButton1.onClick.AddListener(() => OnBiomeSelected(biome1Index));
        }

        if (biomeButton2 != null)
        {
            biomeButton2.onClick.AddListener(() => OnBiomeSelected(biome2Index));
        }

        if (biomeButton3 != null)
        {
            biomeButton3.onClick.AddListener(() => OnBiomeSelected(biome3Index));
        }

        if (biomeButton4 != null)
        {
            biomeButton4.onClick.AddListener(() => OnBiomeSelected(biome4Index));
        }


        // 시작 시 UI 숨김
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }

        // 버튼 텍스트 업데이트
        UpdateButtonTexts();
    }

    /// <summary>
    /// UI를 엽니다.
    /// </summary>
    /// <param name="player">상호작용한 플레이어</param>
    public void OpenUI(GameObject player)
    {
        currentPlayer = player;

        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }

        // 게임 시간 일시정지 (선택사항)
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// UI를 닫습니다.
    /// </summary>
    public void CloseUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }

        currentPlayer = null;

        // 게임 시간 재개 (선택사항)
        // Time.timeScale = 1f;
    }

    /// <summary>
    /// UI가 열려있는지 확인합니다.
    /// </summary>
    /// <returns>UI가 열려있으면 true, 아니면 false</returns>
    public bool IsOpen()
    {
        return uiPanel != null && uiPanel.activeSelf;
    }

    /// <summary>
    /// UI를 토글합니다. (열려있으면 닫고, 닫혀있으면 엽니다)
    /// </summary>
    /// <param name="player">상호작용한 플레이어</param>
    public void ToggleUI(GameObject player)
    {
        if (IsOpen())
        {
            CloseUI();
        }
        else
        {
            OpenUI(player);
        }
    }

    /// <summary>
    /// 버튼 텍스트를 MapManager의 Biome 이름으로 업데이트합니다.
    /// </summary>
    private void UpdateButtonTexts()
    {
        if (mapManager == null)
        {
            Debug.LogWarning("[BiomeSelectionUI] MapManager가 없습니다. 버튼 텍스트를 업데이트할 수 없습니다.");
            return;
        }

        var activeBiomes = GetActiveBiomeList();
        if (activeBiomes == null || activeBiomes.Count == 0)
        {
            Debug.LogWarning("[BiomeSelectionUI] Biome 목록이 비어있습니다. 버튼 텍스트를 업데이트할 수 없습니다.");
            return;
        }

        // 첫 번째 버튼 텍스트 업데이트
        if (biomeButton1 != null && biome1Index >= 0 && biome1Index < activeBiomes.Count)
        {
            TextMeshProUGUI buttonText = biomeButton1.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = activeBiomes[biome1Index].name;
            }
        }

        // 두 번째 버튼 텍스트 업데이트
        if (biomeButton2 != null && biome2Index >= 0 && biome2Index < activeBiomes.Count)
        {
            TextMeshProUGUI buttonText = biomeButton2.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = activeBiomes[biome2Index].name;
            }
        }

        // 세 번째 버튼 텍스트 업데이트
        if (biomeButton3 != null && biome3Index >= 0 && biome3Index < activeBiomes.Count)
        {
            TextMeshProUGUI buttonText = biomeButton3.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = activeBiomes[biome3Index].name;
            }
        }

        // 네 번째 버튼 텍스트 업데이트
        if (biomeButton4 != null && biome4Index >= 0 && biome4Index < activeBiomes.Count)
        {
            TextMeshProUGUI buttonText = biomeButton4.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = activeBiomes[biome4Index].name;
            }
        }
    }

    /// <summary>
    /// MapManager에서 섞인 Biome 리스트를 반환합니다. 섞인 리스트가 없으면 원본 리스트를 반환합니다.
    /// </summary>
    private System.Collections.Generic.List<MapManager.Biome> GetActiveBiomeList()
    {
        var shuffled = mapManager.GetShuffledBiomes();
        if (shuffled != null && shuffled.Count > 0) return shuffled;
        return mapManager.biomes;
    }

    /// <summary>
    /// Biome가 선택되었을 때 호출됩니다.
    /// </summary>
    /// <param name="biomeIndex">선택된 Biome의 인덱스</param>
    private void OnBiomeSelected(int biomeIndex)
    {
        if (teleportManager == null)
        {
            Debug.LogError("[BiomeSelectionUI] TeleportManager가 없습니다.");
            return;
        }

        if (currentPlayer == null)
        {
            Debug.LogError("[BiomeSelectionUI] 현재 플레이어가 없습니다.");
            return;
        }

        // 순간이동 실행
        teleportManager.TeleportToRandomBiomePosition(currentPlayer, biomeIndex);

        // UI 닫기
        CloseUI();
    }
}
