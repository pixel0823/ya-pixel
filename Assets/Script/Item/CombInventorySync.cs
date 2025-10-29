using UnityEngine;

/// <summary>
/// 조합창 안의 인벤토리 패널을 실제 인벤토리와 동기화하는 스크립트
/// ItemListPanel에 붙여서 사용
/// </summary>
public class CombInventorySync : MonoBehaviour
{
    private Inventory inventory;
    private InventoryUI inventoryUI;
    private InventorySlot[] slots;

    void Start()
    {
        // 슬롯들 가져오기
        slots = GetComponentsInChildren<InventorySlot>();

        // Inventory 찾기
        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory를 찾을 수 없습니다!");
            return;
        }

        // InventoryUI 찾기
        inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI == null)
        {
            Debug.LogError("InventoryUI를 찾을 수 없습니다!");
            return;
        }

        // 각 슬롯에 참조 할당
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].inventory = inventory;
                slots[i].inventoryUI = inventoryUI;
                slots[i].slotIndex = i;
            }
        }

        // Inventory 변경 시 업데이트되도록 콜백 등록
        inventory.onItemChangedCallback += UpdateUI;

        // 초기 UI 업데이트
        UpdateUI();
    }

    void UpdateUI()
    {
        if (slots == null || inventory == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                if (i < inventory.items.Count)
                {
                    slots[i].item = inventory.items[i];
                }
                else
                {
                    slots[i].item = null;
                }
                slots[i].UpdateSlotUI();
            }
        }
    }

    void OnDestroy()
    {
        // 콜백 해제
        if (inventory != null)
        {
            inventory.onItemChangedCallback -= UpdateUI;
        }
    }
}
