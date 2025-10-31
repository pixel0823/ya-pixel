using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 인벤토리 UI를 관리하는 클래스.
public class InventoryUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject hotbarPanel;      // 핫바 슬롯들의 부모 패널
    public GameObject inventoryPanel;   // 인벤토리 슬롯들의 부모 패널
    public GameObject itemCombPanel;    // 조합창 패널 (드래그 허용용)

    [Header("UI Elements")]
    public GameObject selectionHighlight; // 선택된 핫바 슬롯을 표시할 UI 오브젝트
    [SerializeField] private Transform rootCanvas; // UI의 최상위 Canvas Transform

    private Inventory inventory;
    private InventorySlot[] hotbarSlots;
    private InventorySlot[] inventorySlots;
    private int selectedSlot = 0;

    // 다른 스크립트에서 inventory에 접근할 수 있도록 프로퍼티 추가
    public Inventory GetInventory()
    {
        // inventory가 null이면 초기화 시도
        if (inventory == null)
        {
            TryInitializeInventory();
        }
        return inventory;
    }

    // --- 드래그 앤 드롭 상태 관리 변수 ---
    private GameObject dragIcon;
    private InventorySlot originalSlot;
    private bool dropSuccessful;

    void Start()
    {
        hotbarSlots = hotbarPanel.GetComponentsInChildren<InventorySlot>();
        inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>();

        var scrollRect = hotbarPanel.GetComponent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.enabled = false;
        }

        hotbarPanel.SetActive(true);
        inventoryPanel.SetActive(false);

        var highlightGraphic = selectionHighlight.GetComponent<UnityEngine.UI.Graphic>();
        if (highlightGraphic != null)
        {
            highlightGraphic.raycastTarget = false;
        }

        if (rootCanvas == null)
        {
            Debug.LogError("Root Canvas가 InventoryUI 컴포넌트에 할당되지 않았습니다! 드래그 기능이 작동하지 않습니다.", this);
        }
        selectedSlot = 0;
        TryInitializeInventory();
    }

    void TryInitializeInventory()
    {
        if (inventory != null) return;

        inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            Debug.Log($"[InventoryUI] Inventory 찾기 완료! 인스턴스 ID: {inventory.GetInstanceID()}");
            Debug.Log($"[InventoryUI] Inventory.items 개수: {inventory.items?.Count ?? 0}");
            inventory.onItemChangedCallback += UpdateUI;
            AssignSlotDetails(hotbarSlots);
            AssignSlotDetails(inventorySlots);
            UpdateUI();
            Canvas.ForceUpdateCanvases();
            UpdateSelectionVisual();
        }
        else
        {
            Debug.LogError("[InventoryUI] Inventory를 찾을 수 없습니다!");
        }
    }

    void AssignSlotDetails(InventorySlot[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].inventory = inventory;
                slots[i].inventoryUI = this;
                slots[i].slotIndex = i;
            }
        }
    }

    public bool IsInventoryOpen()
    {
        // 기본 인벤토리 또는 조합창이 열려있으면 true
        bool inventoryOpen = inventoryPanel != null && inventoryPanel.activeSelf;
        bool combOpen = itemCombPanel != null && itemCombPanel.activeSelf;
        return inventoryOpen || combOpen;
    }

    void Update()
    {
        if (inventory == null)
        {
            TryInitializeInventory();
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

        // 핫바 아이템 버리기 로직
        if (hotbarPanel.activeSelf && Input.GetKeyDown(KeyCode.Q))
        {
            bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            inventory.DropItem(selectedSlot, isCtrlPressed); // true: 전체 버리기, false: 하나 버리기
        }

        // 핫바 슬롯 선택 (마우스 휠)
        if (hotbarPanel.activeSelf)
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (scroll > 0f) selectedSlot--;
                else selectedSlot++;

                if (hotbarSlots.Length > 0)
                {
                    if (selectedSlot < 0) selectedSlot = hotbarSlots.Length - 1;
                    if (selectedSlot >= hotbarSlots.Length) selectedSlot = 0;
                }
                UpdateSelectionVisual();
            }
        }
    }

    void UpdateUI()
    {
        // 디버그: 인벤토리 아이템 목록 출력
        int itemCount = 0;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("[InventoryUI] 인벤토리 아이템: ");
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i] != null)
            {
                itemCount++;
                sb.Append($"{inventory.items[i].itemName} x{inventory.items[i].amount}, ");
            }
        }
        if (itemCount > 0)
        {
            Debug.Log(sb.ToString());
        }

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (i < inventory.items.Count) hotbarSlots[i].item = inventory.items[i];
            else hotbarSlots[i].item = null;
            hotbarSlots[i].UpdateSlotUI();
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inventory.items.Count) inventorySlots[i].item = inventory.items[i];
            else inventorySlots[i].item = null;
            inventorySlots[i].UpdateSlotUI();
        }

        UpdateSelectionVisual();
    }

    void UpdateSelectionVisual()
    {
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

    // --- 드래그 앤 드롭 로직 ---

    public bool IsDragging()
    {
        return originalSlot != null;
    }

    public InventorySlot GetDraggedSlot()
    {
        return originalSlot;
    }

    public void OnBeginDrag(InventorySlot slot)
    {
        if (!IsInventoryOpen() || slot.item == null || rootCanvas == null) return;

        originalSlot = slot;
        dropSuccessful = false;

        dragIcon = new GameObject("Drag Icon");
        var rt = dragIcon.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        var img = dragIcon.AddComponent<Image>();
        img.sprite = originalSlot.item.icon;
        img.raycastTarget = false;

        dragIcon.transform.SetParent(rootCanvas);
        dragIcon.transform.SetAsLastSibling();

        originalSlot.SetDragState(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnDrop(InventorySlot dropSlot)
    {
        if (originalSlot == null || originalSlot == dropSlot) return;

        inventory.SwapItems(originalSlot.slotIndex, dropSlot.slotIndex);
        dropSuccessful = true;
    }

    public void OnEndDrag()
    {
        if (originalSlot == null) return;

        // 마우스 포인터가 인벤토리 패널 또는 조합창 위에 있는지 확인합니다.
        bool isPointerOverInventory = false;

        // 기본 인벤토리 패널 체크
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            RectTransform invPanelRect = inventoryPanel.GetComponent<RectTransform>();
            isPointerOverInventory = RectTransformUtility.RectangleContainsScreenPoint(invPanelRect, Input.mousePosition, null);
        }

        // 조합창 패널 체크
        if (!isPointerOverInventory && itemCombPanel != null && itemCombPanel.activeSelf)
        {
            RectTransform combPanelRect = itemCombPanel.GetComponent<RectTransform>();
            isPointerOverInventory = RectTransformUtility.RectangleContainsScreenPoint(combPanelRect, Input.mousePosition, null);
        }

        if (!dropSuccessful)
        {
            if (!isPointerOverInventory)
            {
                // 드래그가 성공하지 않았고, 마우스가 인벤토리 UI 밖에 있을 때만 아이템을 버립니다.
                inventory.DropItem(originalSlot.slotIndex, true); // 전체 버리기
            }
            else
            {
                // 드래그가 성공하지 않았지만 마우스가 인벤토리 UI 안에 있다면, 아이템을 원래 슬롯으로 되돌립니다.
                // OnBeginDrag에서 호출된 SetDragState(true)를 되돌리기 위해 SetDragState(false)를 호출합니다.
                originalSlot.SetDragState(false);
            }
        }

        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
        dragIcon = null;

        // 드래그 작업이 끝났으므로 원래 슬롯에 대한 참조를 초기화합니다.
        originalSlot = null;
    }
}
