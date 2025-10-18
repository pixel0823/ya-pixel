using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 인벤토리의 각 슬롯을 제어하는 클래스
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI Components")]
    [Tooltip("아이템 아이콘을 표시할 이미지 컴포넌트")]
    public Image itemIcon; // 아이템 아이콘을 표시할 전용 이미지

    private Image slotImage; // 슬롯 자체의 배경 이미지
    private TextMeshProUGUI amountText;

    [HideInInspector] public Item item;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public InventoryUI inventoryUI; // 부모 UI 컨트롤러
    [HideInInspector] public int slotIndex;

    private bool isMouseOver = false;
    private bool isSelected = false;
    private bool isDragging = false;

    private Color normalColor;
    private readonly Color selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f); // 선택 시 색상
    private readonly Color draggingColor = new Color(1f, 1f, 1f, 0.5f); // 드래그 시 색상

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage == null)
        {
            Debug.LogError($"인벤토리 슬롯 '{gameObject.name}'에 Image 컴포넌트가 없습니다!", gameObject);
            return;
        }
        normalColor = slotImage.color; // 기본 배경색 저장

        amountText = GetComponentInChildren<TextMeshProUGUI>();
        if (amountText == null)
        {
            Debug.LogError($"인벤토리 슬롯 '{gameObject.name}'에 TextMeshProUGUI 컴포넌트가 없습니다!", gameObject);
            // return; // Text가 없어도 일단은 동작하도록 주석 처리
        }
        else
        {
            amountText.raycastTarget = false;
        }

        if (itemIcon != null)
        {
            itemIcon.raycastTarget = false; // 아이콘이 마우스 이벤트를 막지 않도록 설정
        }
        else
        {
            Debug.LogError($"인벤토리 슬롯 '{gameObject.name}'에 itemIcon이 할당되지 않았습니다!", gameObject);
        }

        UpdateSlotUI(); // 초기 상태 업데이트
    }

    void Update()
    {
        // 인벤토리가 열려있고, 마우스가 슬롯 위에 있을 때만 키 입력 처리
        if (isMouseOver && item != null && inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (isCtrlPressed && Input.GetKeyDown(KeyCode.Q))
            {
                inventory.DropItem(slotIndex, true); // 전체 드랍
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                inventory.DropItem(slotIndex, false); // 한 개 드랍
            }
        }
    }

    public void UpdateSlotUI()
    {
        if (item != null)
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = true;
            }

            if (amountText != null)
            {
                if (item.isStackable && item.amount > 1)
                {
                    amountText.text = item.amount.ToString();
                    amountText.gameObject.SetActive(true);
                }
                else
                {
                    amountText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }

            if (amountText != null)
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    // 슬롯의 색상을 상태(드래그, 선택)에 따라 업데이트합니다.
    private void UpdateSlotColor()
    {
        if (slotImage == null) return;

        if (isDragging)
        {
            slotImage.color = draggingColor;
        }
        else if (isSelected)
        {
            slotImage.color = selectedColor;
        }
        else
        {
            slotImage.color = normalColor;
        }
    }

    // InventoryUI에서 호출하여 슬롯의 선택 상태를 변경합니다.
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSlotColor();
    }

    public void SetDragState(bool dragging)
    {
        isDragging = dragging;
        UpdateSlotColor();
    }

    // --- 인터페이스 구현 ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 아이템이 없으면 드래그 시작 안함
        if (item == null) return;
        
        if (inventoryUI != null)
        {
            inventoryUI.OnBeginDrag(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI != null && inventoryUI.IsDragging())
        {
            inventoryUI.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventoryUI != null && inventoryUI.IsDragging())
        {
            inventoryUI.OnEndDrag();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnDrop(this);
        }
    }
}