using UnityEngine;

// 인벤토리 UI를 관리하는 클래스.
// 평상시에는 핫바(인벤토리 1-10번)만 보이고, 'E' 키를 누르면 핫바는 사라지고 전체 인벤토리(1-20번)가 나타납니다.
public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject hotbarPanel;      // 핫바 슬롯들의 부모 패널. 슬롯들을 직접 포함해야 합니다.
    public GameObject inventoryPanel;   // 인벤토리 슬롯들의 부모 패널. 슬롯들을 직접 포함해야 합니다.

    [Header("UI Elements")]
    public GameObject selectionHighlight; // 선택된 핫바 슬롯을 표시할 UI 오브젝트

    private Inventory inventory;
    private InventorySlot[] hotbarSlots;
    private InventorySlot[] inventorySlots;
    private int selectedSlot = 0;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        inventory.onItemChangedCallback += UpdateUI;

        // 각 패널에서 직접 슬롯들을 가져옵니다.
        hotbarSlots = hotbarPanel.GetComponentsInChildren<InventorySlot>();
        inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>();

        // 각 슬롯에 인벤토리 참조와 고유 인덱스를 설정합니다.
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].inventory = inventory;
            hotbarSlots[i].slotIndex = i;
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].inventory = inventory;
            inventorySlots[i].slotIndex = i;
        }

        // 초기 상태 설정: 핫바는 보이고, 인벤토리는 숨깁니다.
        hotbarPanel.SetActive(true);
        inventoryPanel.SetActive(false);

        // UI 초기화 및 첫 번째 핫바 슬롯 선택
        selectedSlot = 0;
        UpdateUI();

        // UI 위치 강제 계산 후 선택 UI 업데이트
        Canvas.ForceUpdateCanvases();
        UpdateSelectionVisual();
    }

    void Update()
    {
        // 'E' 키를 눌러 핫바와 인벤토리 패널을 토글합니다.
        if (Input.GetKeyDown(KeyCode.E))
        {
            bool isInventoryOpen = !inventoryPanel.activeSelf;

            inventoryPanel.SetActive(isInventoryOpen);
            hotbarPanel.SetActive(!isInventoryOpen);

            // 인벤토리가 열리면 선택 표시는 사라지고, 닫히면 다시 나타납니다.
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(!isInventoryOpen);
            }

            // 인벤토리를 닫고 핫바를 다시 표시하는 경우
            if (!isInventoryOpen)
            {
                // UI 레이아웃을 강제로 즉시 계산하도록 합니다.
                Canvas.ForceUpdateCanvases();
                // 강제 계산 후, 선택 UI 위치를 업데이트합니다.
                UpdateSelectionVisual();
            }
        }

        // 핫바가 활성화 상태일 때, 'Q' 키를 누르면 선택된 아이템을 버립니다.
        if (hotbarPanel.activeSelf && Input.GetKeyDown(KeyCode.Q))
        {
            Item selectedItem = GetSelectedItem();
            if (selectedItem != null)
            {
                inventory.DropItem(selectedItem);
            }
        }

        // 핫바가 활성화 상태일 때만 마우스 스크롤로 핫바를 순환합니다.
        if (hotbarPanel.activeSelf)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (scroll > 0f) selectedSlot--; // 위로 스크롤
                else selectedSlot++; // 아래로 스크롤

                // 실제 핫바 슬롯 개수(hotbarSlots.Length)를 기준으로 순환하도록 수정
                if (hotbarSlots.Length > 0)
                {
                    if (selectedSlot >= hotbarSlots.Length)
                    {
                        selectedSlot = 0;
                    }
                    if (selectedSlot < 0)
                    {
                        selectedSlot = hotbarSlots.Length - 1;
                    }
                }

                UpdateSelectionVisual();
            }
        }
    }

    // 전체 인벤토리 UI(핫바 + 인벤토리)를 업데이트합니다.
    void UpdateUI()
    {
        // 핫바 UI 업데이트 (인벤토리의 첫 10개 아이템)
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                hotbarSlots[i].AddItem(inventory.items[i]);
            }
            else
            {
                hotbarSlots[i].ClearSlot();
            }
        }

        // 전체 인벤토리 UI 업데이트 (인벤토리의 모든 아이템)
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                inventorySlots[i].AddItem(inventory.items[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }

        UpdateSelectionVisual();
    }

    // 선택된 핫바 슬롯의 UI를 갱신하는 함수
    void UpdateSelectionVisual()
    {
        // 핫바가 비활성화 상태이면 선택 UI도 업데이트하지 않습니다.
        if (!hotbarPanel.activeSelf || selectionHighlight == null || hotbarSlots.Length == 0)
        {
            if (selectionHighlight != null) selectionHighlight.SetActive(false);
            return;
        }

        selectionHighlight.SetActive(true);
        selectionHighlight.transform.position = hotbarSlots[selectedSlot].transform.position;
    }

    // 현재 선택된 핫바의 아이템을 반환하는 함수
    public Item GetSelectedItem()
    {
        if (selectedSlot >= 0 && selectedSlot < inventory.items.Count)
        {
            return inventory.items[selectedSlot];
        }
        return null;
    }
}
