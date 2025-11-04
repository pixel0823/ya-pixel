using UnityEngine;
using YAPixel;

// 아이템 정보를 담는 ScriptableObject. Asset 메뉴에서 생성하여 사용합니다.
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IDatabaseItem
{
    public string Name => itemName;

    // 'name'은 Object에 이미 있으므로 'itemName'을 사용합니다.
    public string itemName = "New Item"; // 아이템 이름 (데이터 식별자)
    public string description = "Item Description"; // 아이템 설명
    public Sprite icon = null; // 아이템 아이콘

    public bool isStackable = true; // 아이템이 겹칠 수 있는지 여부
    public int maxStackSize = 99; // 최대 겹칠 수 있는 개수

    public int amount = 1; // 현재 아이템의 개수 (인벤토리 내에서 사용)

    public Item GetCopy()
    {
        Item copy = CreateInstance<Item>();
        copy.itemName = itemName;
        copy.description = description;
        copy.icon = icon;
        copy.isStackable = isStackable;
        copy.maxStackSize = maxStackSize;
        copy.amount = amount;
        return copy;
    }

    //스택의 개수를 설정하는 함수
    public void SetAmount(int newAmount)
    {
        amount = Mathf.Clamp(newAmount, 1, maxStackSize);
    }
}
