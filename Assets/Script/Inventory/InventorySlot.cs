using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 인벤토리의 각 슬롯을 제어하는 클래스
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Image slotImage;
    private Sprite defaultSprite;
    private TextMeshProUGUI amountText;

    [HideInInspector] public Item item;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public InventoryUI inventoryUI; // 부모 UI 컨트롤러
    [HideInInspector] public int slotIndex;

    private bool isMouseOver = false;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage == null)
        {
            Debug.LogError($"인벤토리 슬롯 '{gameObject.name}'에 Image 컴포넌트가 없습니다!", gameObject);
            return;
        }
        defaultSprite = slotImage.sprite;

        amountText = GetComponentInChildren<TextMeshProUGUI>();
        if (amountText == null)
        {
            Debug.LogError($"인벤토리 슬롯 '{gameObject.name}'에 TextMeshProUGUI 컴포넌트가 없습니다!", gameObject);
            return;
        }
        amountText.raycastTarget = false;
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
        if (slotImage == null) return;

        if (item != null)
        {
            slotImage.sprite = item.icon;
            slotImage.color = Color.white;

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
            slotImage.sprite = defaultSprite;
            slotImage.color = Color.white;
            if (amountText != null)
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    public void SetDragState(bool isDragging)
    {
        if (slotImage != null)
        {
            slotImage.color = isDragging ? new Color(1, 1, 1, 0.5f) : Color.white;
        }
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