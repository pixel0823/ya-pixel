using System.Collections.Generic;
using UnityEngine;

// 레시피 재료 정보
[System.Serializable]
public class RecipeIngredient
{
    public Item item;           // 필요한 아이템
    public int requiredAmount;  // 필요한 개수

    public RecipeIngredient(Item item, int amount)
    {
        this.item = item;
        this.requiredAmount = amount;
    }
}

// 레시피 결과 정보
[System.Serializable]
public class RecipeResult
{
    public Item item;           // 생성될 아이템
    public int resultAmount;    // 생성 개수

    public RecipeResult(Item item, int amount)
    {
        this.item = item;
        this.resultAmount = amount;
    }
}

// 레시피 정의
[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("레시피 정보")]
    public string recipeName = "New Recipe";        // 레시피 이름
    [TextArea(3, 5)]
    public string description = "";                  // 레시피 설명

    [Header("재료")]
    public List<RecipeIngredient> ingredients = new List<RecipeIngredient>();  // 필요한 재료들

    [Header("결과")]
    public RecipeResult result;  // 조합 결과

    [Header("조합 조건")]
    public bool requiresCraftingTable = false;  // 작업대가 필요한지
    public string requiredTool = "";            // 필요한 도구 (비어있으면 도구 불필요)

    // 재료가 충분한지 확인하는 함수
    public bool CanCraft(Dictionary<Item, int> availableItems)
    {
        foreach (RecipeIngredient ingredient in ingredients)
        {
            // 이름 정규화를 통한 비교 (공백 제거 + 소문자 변환)
            string normalizedIngredientName = ingredient.item.itemName.Trim().ToLower();
            bool found = false;
            int availableAmount = 0;

            foreach (var kvp in availableItems)
            {
                string normalizedAvailableName = kvp.Key.itemName.Trim().ToLower();
                if (normalizedAvailableName == normalizedIngredientName)
                {
                    found = true;
                    availableAmount = kvp.Value;
                    break;
                }
            }

            if (!found)
            {
                return false;
            }

            if (availableAmount < ingredient.requiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    // 레시피의 유효성 검사
    public bool IsValid()
    {
        if (ingredients.Count == 0)
        {
            Debug.LogWarning($"레시피 '{recipeName}'에 재료가 없습니다.");
            return false;
        }

        if (result == null || result.item == null)
        {
            Debug.LogWarning($"레시피 '{recipeName}'에 결과물이 없습니다.");
            return false;
        }

        foreach (RecipeIngredient ingredient in ingredients)
        {
            if (ingredient.item == null)
            {
                Debug.LogWarning($"레시피 '{recipeName}'에 null 아이템이 있습니다.");
                return false;
            }
        }

        return true;
    }
}

