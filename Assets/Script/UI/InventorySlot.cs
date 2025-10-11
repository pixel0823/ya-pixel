using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 인벤토리의 각 슬롯을 제어하는 클래스
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    [HideInInspector]
    public Item item;
    [HideInInspector]
    public Inventory inventory;
    [HideInInspector]
    public int slotIndex; // 슬롯의 인덱스

    private bool isMouseOver = false; // 개별 슬롯의 마우스 오버 상태
    private static GameObject dragIcon; // 드래그 중인 아이콘
    private static Item draggedItem; // 드래그 중인 아이템
    private static InventorySlot originalSlot; // 드래그 시작 슬롯
    private static bool dropSuccessful; // 드롭 성공 여부

    void Update()
    {
        // 이 슬롯 위에 마우스가 있고, 'Q' 키가 눌렸다면
        if (isMouseOver && Input.GetKeyDown(KeyCode.Q))
        {
            if (item != null && inventory != null)
            {
                inventory.DropItem(item);
            }
        }
    }

    public void AddItem(Item newItem)
    {
        item = newItem;

        // icon이 할당되지 않았을 경우, 오류를 기록하고 함수를 종료합니다.
        if (icon == null)
        { 
            Debug.LogError($"인벤토리 슬롯 '{gameObject.name}'에 아이콘(Image) 컴포넌트가 할당되지 않았습니다!", gameObject);
            return;
        }

        icon.sprite = newItem?.icon;
        icon.enabled = newItem != null;
    }

    public void ClearSlot()
    {
        item = null;
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

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
        if (item != null)
        {
            originalSlot = this;
            draggedItem = item;
            dropSuccessful = false;

            // 드래그 아이콘 생성
            dragIcon = new GameObject("Drag Icon");
            var rt = dragIcon.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50);
            var img = dragIcon.AddComponent<Image>();
            img.sprite = item.icon;
            dragIcon.transform.SetParent(transform.root);
            dragIcon.transform.SetAsLastSibling();
            img.raycastTarget = false;

            // 원래 슬롯 아이콘 숨김
            icon.enabled = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(dragIcon);

        // OnDrop이 호출되지 않았다면(즉, 유효한 슬롯에 드롭되지 않았다면)
        // 아이템을 월드에 버리는 것으로 처리합니다.
        if (!dropSuccessful)
        {
            if (inventory != null && draggedItem != null)
            {
                inventory.DropItem(draggedItem);
            }
            else
            {
                // 인벤토리나 아이템 정보가 없으면, 원래 슬롯으로 아이콘을 복원합니다.
                if (originalSlot != null) originalSlot.icon.enabled = true;
            }
        }

        // static 변수 초기화
        draggedItem = null;
        originalSlot = null;
        dropSuccessful = false; // 다음 드래그를 위해 리셋
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 드롭된 아이템의 원본 슬롯 정보를 가져옵니다.
        InventorySlot draggedSlot = originalSlot;

        // 자기 자신에게 드롭했거나, 원본 슬롯 정보가 없거나, 인벤토리 정보가 없으면 무시합니다.
        if (draggedSlot == null || draggedSlot == this || inventory == null)
        {
            return;
        }

        // 인벤토리의 SwapItems 메서드를 호출하여 아이템 위치를 바꿉니다.
        // 이 때, 각 슬롯의 인덱스 정보를 사용합니다.
        inventory.SwapItems(draggedSlot.slotIndex, this.slotIndex);

        // 드롭이 성공적으로 완료되었음을 표시합니다.
        dropSuccessful = true;
    }
}