using UnityEngine;
using UnityEngine.UI; // Image, Text 같은 UI 컴포넌트를 사용하기 위해 필요합니다.

// 인벤토리의 각 슬롯을 제어하는 클래스
public class InventorySlot : MonoBehaviour
{
    public Image icon; // 아이템의 아이콘을 표시할 이미지
    // public Text amountText; // 아이템 수량을 표시할 텍스트 (필요 시 사용)

    private Item item; // 이 슬롯에 할당된 아이템 정보

    // 슬롯에 아이템을 추가하고 UI를 갱신하는 함수
    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true; // 아이콘 이미지를 활성화
    }

    // 슬롯을 비우는 함수
    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false; // 아이콘 이미지를 비활성화
    }
}
