using UnityEngine;

// 아이템 정보를 담는 ScriptableObject. Asset 메뉴에서 생성하여 사용합니다.
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    // 'name'은 Object에 이미 있으므로 'itemName'을 사용합니다.
    public string itemName = "New Item"; // 아이템 이름 (데이터 식별자)
    public string description = "Item Description"; // 아이템 설명
    public Sprite icon = null; // 아이템 아이콘

    public Item GetCopy()
    {
        Item copy = CreateInstance<Item>();
        copy.itemName = itemName;
        copy.description = description;
        copy.icon = icon;
        return copy;
    }
}
