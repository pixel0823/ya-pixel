using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Inventory/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    public List<Recipe> allRecipes = new List<Recipe>();

    // 인덱스로 레시피를 찾는 함수
    public Recipe GetRecipe(int index)
    {
        if (index >= 0 && index < allRecipes.Count)
        {
            return allRecipes[index];
        }
        Debug.LogError($"잘못된 레시피 인덱스입니다: {index}");
        return null;
    }

    // 레시피 이름으로 찾기
    public Recipe GetRecipeByName(string recipeName)
    {
        foreach (Recipe recipe in allRecipes)
        {
            if (recipe != null && recipe.recipeName == recipeName)
            {
                return recipe;
            }
        }
        Debug.LogWarning($"'{recipeName}' 레시피를 찾을 수 없습니다.");
        return null;
    }

    // 결과 아이템으로 레시피 찾기 (여러 개일 수 있음)
    public List<Recipe> GetRecipesByResult(Item resultItem)
    {
        List<Recipe> recipes = new List<Recipe>();

        foreach (Recipe recipe in allRecipes)
        {
            if (recipe != null && recipe.result != null && recipe.result.item == resultItem)
            {
                recipes.Add(recipe);
            }
        }

        return recipes;
    }

    // 특정 재료를 사용하는 레시피 찾기
    public List<Recipe> GetRecipesByIngredient(Item ingredient)
    {
        List<Recipe> recipes = new List<Recipe>();

        foreach (Recipe recipe in allRecipes)
        {
            if (recipe != null)
            {
                foreach (RecipeIngredient recipeIngredient in recipe.ingredients)
                {
                    if (recipeIngredient.item == ingredient)
                    {
                        recipes.Add(recipe);
                        break;
                    }
                }
            }
        }

        return recipes;
    }

    // 현재 가진 아이템으로 만들 수 있는 레시피 찾기
    public List<Recipe> GetCraftableRecipes(Dictionary<Item, int> availableItems)
    {
        List<Recipe> craftableRecipes = new List<Recipe>();

        foreach (Recipe recipe in allRecipes)
        {
            if (recipe != null && recipe.IsValid() && recipe.CanCraft(availableItems))
            {
                craftableRecipes.Add(recipe);
            }
        }

        return craftableRecipes;
    }

    // 데이터베이스 유효성 검사
    public void ValidateDatabase()
    {
        int validCount = 0;
        int invalidCount = 0;

        foreach (Recipe recipe in allRecipes)
        {
            if (recipe != null)
            {
                if (recipe.IsValid())
                {
                    validCount++;
                }
                else
                {
                    invalidCount++;
                }
            }
            else
            {
                Debug.LogWarning("레시피 데이터베이스에 null 레시피가 있습니다.");
                invalidCount++;
            }
        }

        Debug.Log($"레시피 데이터베이스 검증 완료: 유효 {validCount}개, 무효 {invalidCount}개");
    }
}
