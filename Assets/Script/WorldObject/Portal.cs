using UnityEngine;

/// <summary>
/// Portal과의 상호작용을 처리합니다. IInteractable을 구현합니다.
/// 플레이어가 Portal과 상호작용하면 Biome 선택 UI를 토글합니다.
/// </summary>
public class Portal : MonoBehaviour, IInteractable
{
    [Tooltip("Portal 이름 (UI에 표시될 이름)")]
    public string portalName = "외부 세계";

    private BiomeSelectionUI biomeUI;

    void Start()
    {
        Debug.Log("[Portal] Portal Start() 호출됨!");

        // BiomeSelectionUI를 미리 찾아서 캐시 (비활성화된 오브젝트도 포함)
        biomeUI = FindObjectOfType<BiomeSelectionUI>(true);
        if (biomeUI == null)
        {
            Debug.LogError("[Portal] ❌ BiomeSelectionUI를 찾을 수 없습니다. Scene에 BiomeSelectionUI가 있는지 확인하세요.");
        }
        else
        {
            Debug.Log("[Portal] ✅ BiomeSelectionUI를 찾았습니다!");
        }

        // Collider 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("[Portal] ❌ Collider2D가 없습니다!");
        }
        else
        {
            Debug.Log($"[Portal] ✅ Collider2D 있음 (IsTrigger: {col.isTrigger})");
        }
    }

    public string GetInteractText()
    {
        Debug.Log("[Portal] GetInteractText() 호출됨!");

        // UI가 열려있으면 "닫기" 메시지, 닫혀있으면 "이동" 메시지
        if (biomeUI != null && biomeUI.IsOpen())
        {
            return "'F' 키를 눌러 닫기";
        }
        return $"'F' 키를 눌러 {portalName}로 이동";
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"[Portal] ✅ Interact() 호출됨! 상호작용자: {interactor.name}");

        // Portal 위치를 PortalReturnManager에 등록 (귀환석 사용 시 돌아올 위치)
        PortalReturnManager returnManager = FindObjectOfType<PortalReturnManager>();
        if (returnManager != null)
        {
            returnManager.RegisterPortalUsage(interactor, transform.position);
        }

        // Portal과 상호작용 중 = 도시에 있음 = 온도 감소 비활성화
        if (StatusManager.Instance != null)
        {
            StatusManager.Instance.DisableTemperatureDecrease();
        }

        if (biomeUI != null)
        {
            // UI를 토글 (열려있으면 닫고, 닫혀있으면 열기)
            biomeUI.ToggleUI(interactor);
            Debug.Log($"[Portal] UI 상태: {(biomeUI.IsOpen() ? "열림" : "닫힘")}");
        }
        else
        {
            Debug.LogError("[Portal] ❌ biomeUI가 null입니다!");
        }
    }
}
