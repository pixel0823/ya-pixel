using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 인벤토리의 각 슬롯을 제어하는 클래스 (구조 변경 버전)
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Image slotImage;
    private Sprite defaultSprite;

    [HideInInspector]
    public Item item;
    [HideInInspector]
    public Inventory inventory;
    [HideInInspector]
    public int slotIndex;

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
    }

    void Update()
    {
        if (isMouseOver && Input.GetKeyDown(KeyCode.Q))
        {
            if (item != null && inventory != null)
            {
                inventory.DropItem(slotIndex);
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
        }
        else
        {
            slotImage.sprite = defaultSprite;
            slotImage.color = Color.white;
        }
    }

    // 드래그 상태에 따라 UI를 변경합니다.
    public void SetDragState(bool isDragging)
    {
        if (slotImage != null)
        {
            // isDragging이 true이면 투명하게, false이면 불투명하게 만듭니다.
            slotImage.color = isDragging ? Color.clear : Color.white;
        }
    }

    // --- IPointer 인터페이스 구현 ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }

    // --- IDrag & IDrop 인터페이스 구현 ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null && inventory != null)
        {
            inventory.OnBeginDrag(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventory != null && inventory.IsDragging())
        {
            inventory.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventory != null && inventory.IsDragging())
        {
            inventory.OnEndDrag();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (inventory != null)
        {
            inventory.OnDrop(this);
        }
    }
}
