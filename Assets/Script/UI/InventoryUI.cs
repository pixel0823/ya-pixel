using UnityEngine;

// 인벤토리 UI를 관리하는 클래스.
public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject hotbarPanel;      // 핫바 슬롯들의 부모 패널
    public GameObject inventoryPanel;   // 인벤토리 슬롯들의 부모 패널

    [Header("UI Elements")]
    public GameObject selectionHighlight; // 선택된 핫바 슬롯을 표시할 UI 오브젝트

    private Inventory inventory;
    private InventorySlot[] hotbarSlots;
    private InventorySlot[] inventorySlots;
    private int selectedSlot = 0;

    void Start()
    {
        hotbarSlots = hotbarPanel.GetComponentsInChildren<InventorySlot>();
        inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>();

        // 핫바 패널에 ScrollRect가 있다면 비활성화하여 스크롤 입력을 가로채지 못하게 합니다.
        var scrollRect = hotbarPanel.GetComponent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.enabled = false;
        }

        hotbarPanel.SetActive(true);
        inventoryPanel.SetActive(false);

        // selectionHighlight가 마우스 이벤트를 가로채지 않도록 설정합니다.
        var highlightGraphic = selectionHighlight.GetComponent<UnityEngine.UI.Graphic>();
        if (highlightGraphic != null)
        {
            highlightGraphic.raycastTarget = false;
        }

        selectedSlot = 0;

        // Try to find the inventory immediately
        TryInitializeInventory();
    }

    void TryInitializeInventory()
    {
        // 이미 초기화되었으면 실행하지 않음
        if (inventory != null) return;

        inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.onItemChangedCallback += UpdateUI;

            // 각 슬롯에 인벤토리 참조와 고유 인덱스를 설정합니다.
            AssignSlotDetails(hotbarSlots);
            AssignSlotDetails(inventorySlots);

            UpdateUI();
            Canvas.ForceUpdateCanvases();
            UpdateSelectionVisual();
        }
    }

    // 슬롯들에 인벤토리 참조와 인덱스를 할당하는 도우미 함수
    void AssignSlotDetails(InventorySlot[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].inventory = inventory; // 이 시점에는 inventory가 null이 아니어야 합니다.
                slots[i].slotIndex = i;
            }
        }
    }

    void Update()
    {
        // 인벤토리를 아직 찾지 못했다면 계속 찾습니다.
        if (inventory == null)
        {
            TryInitializeInventory();
            // 여전히 찾지 못했다면 에러 방지를 위해 Update를 종료합니다.
            if (inventory == null) return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            bool isInventoryOpen = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isInventoryOpen);
            hotbarPanel.SetActive(!isInventoryOpen);

            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(!isInventoryOpen);
            }

            if (!isInventoryOpen)
            {
                Canvas.ForceUpdateCanvases();
                UpdateSelectionVisual();
            }
        }

        // 수정된 DropItem(index) 메서드를 사용합니다.
        if (hotbarPanel.activeSelf && Input.GetKeyDown(KeyCode.Q))
        {
            // 현재 선택된 핫바 슬롯의 아이템을 버립니다.
            inventory.DropItem(selectedSlot);
        }

        if (hotbarPanel.activeSelf)
        {
            // GetAxisRaw는 입력 값을 보정 없이 그대로 가져오므로, 스크롤과 같은 단발성 입력에 더 적합합니다.
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");

            if (scroll != 0)
            {
                if (scroll > 0f) // 위로 스크롤
                {
                    selectedSlot--;
                }
                else // 아래로 스크롤
                {
                    selectedSlot++;
                }

                if (hotbarSlots.Length > 0)
                {
                    if (selectedSlot < 0) selectedSlot = hotbarSlots.Length - 1;
                    if (selectedSlot >= hotbarSlots.Length) selectedSlot = 0;
                }
                UpdateSelectionVisual();
            }
        }
    }

    // UI가 인벤토리 데이터를 정확히 반영하도록 업데이트합니다.
    void UpdateUI()
    {
        // 핫바 슬롯 업데이트
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                hotbarSlots[i].item = inventory.items[i]; // 1. 데이터 할당
            }
            else
            {
                hotbarSlots[i].item = null; // 아이템이 없으면 슬롯을 비웁니다.
            }
            hotbarSlots[i].UpdateSlotUI(); // 2. UI 갱신 요청 (항상)
        }

        // 전체 인벤토리 슬롯 업데이트
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                inventorySlots[i].item = inventory.items[i]; // 1. 데이터 할당
            }
            else
            {
                inventorySlots[i].item = null; // 아이템이 없으면 슬롯을 비웁니다.
            }
            inventorySlots[i].UpdateSlotUI(); // 2. UI 갱신 요청 (항상)
        }

        UpdateSelectionVisual();
    }

    // 선택된 핫바 슬롯의 UI를 갱신합니다.
    void UpdateSelectionVisual()
    {
        // 레이아웃이 완전히 계산되도록 강제로 업데이트합니다.
        // 이렇게 하면 비활성화 상태에서 위치를 가져오거나, 레이아웃 변경 직후 위치를 가져올 때 발생할 수 있는 문제를 방지합니다.
        Canvas.ForceUpdateCanvases();

        if (!hotbarPanel.activeSelf || selectionHighlight == null || hotbarSlots.Length == 0)
        {
            if (selectionHighlight != null) selectionHighlight.SetActive(false);
            return;
        }

        selectionHighlight.SetActive(true);
        if (selectedSlot >= 0 && selectedSlot < hotbarSlots.Length)
        {
            selectionHighlight.transform.position = hotbarSlots[selectedSlot].transform.position;
        }
    }
}