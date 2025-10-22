using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("레시피 데이터베이스")]
    public RecipeDatabase recipeDatabase;

    [Header("인벤토리 참조")]
    public Inventory inventory;  // 인벤토리 참조

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (recipeDatabase == null)
        {
            Debug.LogError("RecipeDatabase가 할당되지 않았습니다!");
        }

        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
            if (inventory == null)
            {
                Debug.LogWarning("Inventory를 찾을 수 없습니다. 수동으로 할당해주세요.");
            }
        }
    }

    // 레시피로 아이템 조합
    public bool CraftItem(Recipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogError("레시피가 null입니다.");
            return false;
        }

        if (!recipe.IsValid())
        {
            Debug.LogError($"레시피 '{recipe.recipeName}'가 유효하지 않습니다.");
            return false;
        }

        if (inventory == null)
        {
            Debug.LogError("Inventory가 설정되지 않았습니다.");
            return false;
        }

        // 재료 확인
        Dictionary<Item, int> availableItems = GetAvailableItems();
        if (!recipe.CanCraft(availableItems))
        {
            Debug.Log($"재료가 부족하여 '{recipe.recipeName}'를 조합할 수 없습니다.");
            return false;
        }

        // 재료 소모
        foreach (RecipeIngredient ingredient in recipe.ingredients)
        {
            if (!ConsumeItem(ingredient.item, ingredient.requiredAmount))
            {
                Debug.LogError($"재료 소모 중 오류가 발생했습니다: {ingredient.item.itemName}");
                return false;
            }
        }

        // 결과 아이템 생성 및 인벤토리에 추가
        Item resultItem = recipe.result.item.GetCopy();
        if (inventory.Add(resultItem, recipe.result.resultAmount))
        {
            Debug.Log($"'{recipe.recipeName}' 조합 성공! {recipe.result.item.itemName} x{recipe.result.resultAmount} 획득");
            return true;
        }
        else
        {
            Debug.LogError("인벤토리가 가득 차서 아이템을 추가할 수 없습니다.");
            // TODO: 재료 복구 로직 추가 가능
            return false;
        }
    }

    // 인벤토리에서 사용 가능한 아이템 목록 가져오기
    private Dictionary<Item, int> GetAvailableItems()
    {
        Dictionary<Item, int> availableItems = new Dictionary<Item, int>();

        if (inventory == null || inventory.items == null)
        {
            return availableItems;
        }

        foreach (Item item in inventory.items)
        {
            if (item != null)
            {
                // 아이템 이름으로 구분하여 합산
                Item existingKey = null;
                foreach (Item key in availableItems.Keys)
                {
                    if (key.itemName == item.itemName)
                    {
                        existingKey = key;
                        break;
                    }
                }

                if (existingKey != null)
                {
                    availableItems[existingKey] += item.amount;
                }
                else
                {
                    availableItems[item] = item.amount;
                }
            }
        }

        return availableItems;
    }

    // 인벤토리에서 아이템 소모
    private bool ConsumeItem(Item itemTemplate, int amount)
    {
        if (inventory == null || inventory.items == null)
        {
            return false;
        }

        int remainingAmount = amount;

        // 모든 슬롯을 확인하며 아이템 소모
        for (int i = 0; i < inventory.items.Count; i++)
        {
            Item item = inventory.items[i];
            if (item != null && item.itemName == itemTemplate.itemName)
            {
                if (item.amount >= remainingAmount)
                {
                    // 이 슬롯에서 필요한 만큼 모두 소모 가능
                    item.amount -= remainingAmount;
                    if (item.amount == 0)
                    {
                        inventory.items[i] = null;
                    }
                    inventory.onItemChangedCallback?.Invoke();
                    return true;
                }
                else
                {
                    // 이 슬롯의 아이템을 모두 소모하고 다음 슬롯으로
                    remainingAmount -= item.amount;
                    item.amount = 0;
                    inventory.items[i] = null;
                }
            }
        }

        // 소모 완료 후 콜백 호출
        if (remainingAmount == 0)
        {
            inventory.onItemChangedCallback?.Invoke();
        }

        return remainingAmount == 0;
    }

    // 조합 가능한 레시피 목록 가져오기
    public List<Recipe> GetCraftableRecipes()
    {
        if (recipeDatabase == null)
        {
            return new List<Recipe>();
        }

        Dictionary<Item, int> availableItems = GetAvailableItems();
        return recipeDatabase.GetCraftableRecipes(availableItems);
    }

    // 특정 결과물을 만드는 레시피 찾기
    public List<Recipe> GetRecipesForResult(Item resultItem)
    {
        if (recipeDatabase == null)
        {
            return new List<Recipe>();
        }

        return recipeDatabase.GetRecipesByResult(resultItem);
    }

    // 특정 재료를 사용하는 레시피 찾기
    public List<Recipe> GetRecipesUsingIngredient(Item ingredient)
    {
        if (recipeDatabase == null)
        {
            return new List<Recipe>();
        }

        return recipeDatabase.GetRecipesByIngredient(ingredient);
    }
}
