using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 상자 슬롯의 클릭 이벤트를 처리하는 헬퍼 컴포넌트
/// InventorySlot과 독립적으로 작동
/// </summary>
public class ChestSlotHandler : MonoBehaviour, IPointerClickHandler
{
    private ChestUI chestUI;
    private int slotIndex;

    /// <summary>
    /// ChestUI와 슬롯 인덱스 설정
    /// </summary>
    public void Initialize(ChestUI ui, int index)
    {
        chestUI = ui;
        slotIndex = index;
    }

    /// <summary>
    /// 슬롯 클릭 시 호출
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭만 처리
        if (eventData.button == PointerEventData.InputButton.Left && chestUI != null)
        {
            chestUI.OnChestSlotClicked(slotIndex);
        }
    }
}
