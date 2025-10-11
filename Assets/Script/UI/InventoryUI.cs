using UnityEngine;

// 인벤토리 UI를 관리하는 클래스. 콜백을 받아 UI를 효율적으로 업데이트합니다.
public class InventoryUI : MonoBehaviour
{
    public Transform itemsParent; // 아이템 슬롯들의 부모 트랜스폼

    private Inventory inventory; // 플레이어 인벤토리 참조
    private InventorySlot[] slots; // 인벤토리 슬롯들 배열

    void Start()
    {
        // 플레이어의 Inventory 컴포넌트를 찾습니다.
        inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            // 인벤토리의 onItemChangedCallback에 UpdateUI 함수를 등록(구독)합니다.
            inventory.onItemChangedCallback += UpdateUI;
        }

        // 자식 오브젝트들에서 모든 InventorySlot 컴포넌트를 가져와 배열에 저장합니다.
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        // 초기 UI 업데이트
        UpdateUI();
    }

    // 인벤토리 UI를 업데이트하는 함수
    void UpdateUI()
    {
        // 모든 슬롯을 순회합니다.
        for (int i = 0; i < slots.Length; i++)
        {
            // 인벤토리에 해당 인덱스의 아이템이 존재하면
            if (i < inventory.items.Count)
            {
                // 슬롯에 아이템 정보를 채웁니다.
                slots[i].AddItem(inventory.items[i]);
            }
            else
            {
                // 그렇지 않으면 슬롯을 비웁니다.
                slots[i].ClearSlot();
            }
        }
    }
}
