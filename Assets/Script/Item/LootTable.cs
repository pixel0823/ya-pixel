using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<LootItem> possibleItems = new List<LootItem>();
    public int minItemsToGive = 1;  // 최소 지급 아이템 개수
    public int maxItemsToGive = 3;  // 최대 지급 아이템 개수

    /// <summary>
    /// 가중치 기반 랜덤으로 아이템들을 선택합니다
    /// </summary>
    public List<ItemDrop> GenerateRandomItems()
    {
        List<ItemDrop> selectedItems = new List<ItemDrop>();

        if (possibleItems.Count == 0)
        {
            Debug.LogWarning("LootTable에 아이템이 없습니다!");
            return selectedItems;
        }

        // 지급할 아이템 개수 결정
        int itemsToGive = Random.Range(minItemsToGive, maxItemsToGive + 1);

        for (int i = 0; i < itemsToGive; i++)
        {
            LootItem selected = SelectRandomItem();
            if (selected != null && selected.item != null)
            {
                int amount = Random.Range(selected.minAmount, selected.maxAmount + 1);
                selectedItems.Add(new ItemDrop(selected.item, amount));
            }
        }

        return selectedItems;
    }

    /// <summary>
    /// 드롭 확률(가중치)을 기반으로 하나의 아이템을 랜덤 선택
    /// </summary>
    private LootItem SelectRandomItem()
    {
        // 총 가중치 계산
        float totalWeight = 0f;
        foreach (var lootItem in possibleItems)
        {
            totalWeight += lootItem.dropRate;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("총 드롭 확률이 0입니다!");
            return null;
        }

        // 랜덤 값 생성
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        // 가중치 기반 선택
        foreach (var lootItem in possibleItems)
        {
            currentWeight += lootItem.dropRate;
            if (randomValue <= currentWeight)
            {
                return lootItem;
            }
        }

        // 만약 여기까지 왔다면 마지막 아이템 반환
        return possibleItems[possibleItems.Count - 1];
    }
}

/// <summary>
/// LootTable에 등록할 아이템 정보
/// </summary>
[System.Serializable]
public class LootItem
{
    public Item item;           // 드롭될 아이템
    public float dropRate = 50f; // 드롭 확률 (가중치) - 높을수록 자주 나옴
    public int minAmount = 1;   // 최소 개수
    public int maxAmount = 1;   // 최대 개수
}

/// <summary>
/// 선택된 아이템과 개수
/// </summary>
[System.Serializable]
public class ItemDrop
{
    public Item item;
    public int amount;

    public ItemDrop(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}
