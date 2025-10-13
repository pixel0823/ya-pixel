using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> allItems = new List<Item>();

    // 인덱스로 아이템을 찾는 함수
    public Item GetItem(int index)
    {
        if (index >= 0 && index < allItems.Count)
        {
            return allItems[index];
        }
        Debug.LogError($"잘못된 아이템 인덱스입니다: {index}");
        return null;
    }

    // 아이템으로 인덱스를 찾는 함수
    public int GetIndex(Item item)
    {
        if (item == null) return -1;
        for (int i = 0; i < allItems.Count; i++)
        {
            if (allItems[i] != null && allItems[i].itemName == item.itemName)
            {
                return i;
            }
        }

        Debug.LogWarning($"{item.itemName} 아이템이 데이터베이스에 없습니다.");
        return -1;
    }
}
