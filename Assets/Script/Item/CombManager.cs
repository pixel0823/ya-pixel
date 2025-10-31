using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 조합 시스템 관리
/// ItemCombPanel에 붙여서 사용
/// </summary>
public class CombManager : MonoBehaviour
{
    [Header("조합 슬롯 (자동 설정)")]
    public CombSlot slot1; // 재료 슬롯 1
    public CombSlot slot2; // 재료 슬롯 2
    public CombSlot slot3; // 결과 슬롯

    [Header("조합 버튼")]
    public Button craftButton;

    [Header("참조")]
    public RecipeDatabase recipeDatabase;
    public Inventory inventory;
    public InventoryUI inventoryUI;

    private Recipe currentRecipe;

    void Start()
    {
        // 슬롯 자동 찾기 - 직접 자식만 (프리펩 대응)
        List<CombSlot> foundSlots = new List<CombSlot>();

        foreach (Transform child in transform)
        {
            CombSlot combSlot = child.GetComponent<CombSlot>();
            if (combSlot == null)
            {
                // CombSlot이 없으면 자동 추가
                combSlot = child.gameObject.AddComponent<CombSlot>();
                Debug.Log($"CombSlot 자동 추가: {child.name}");
            }
            foundSlots.Add(combSlot);
        }

        if (foundSlots.Count >= 3)
        {
            slot1 = foundSlots[0];
            slot2 = foundSlots[1];
            slot3 = foundSlots[2];

            // 결과 슬롯 설정
            slot3.isResultSlot = true;

            Debug.Log($"조합 슬롯 자동 설정 완료: {slot1.name}, {slot2.name}, {slot3.name}");
        }
        else
        {
            Debug.LogError($"조합 슬롯이 부족합니다! (필요: 3개, 발견: {foundSlots.Count}개)");
            Debug.LogError($"ItemCombPanel의 직접 자식 오브젝트가 3개 있어야 합니다.");
        }

        // 조합 버튼 자동 찾기
        if (craftButton == null)
        {
            // 부모에서 CombBTN 찾기
            Transform parent = transform.parent;
            if (parent != null)
            {
                foreach (Transform sibling in parent)
                {
                    if (sibling.name == "CombBTN")
                    {
                        craftButton = sibling.GetComponent<Button>();
                        Debug.Log("조합 버튼 자동 찾기 완료: CombBTN");
                        break;
                    }
                }
            }
        }

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
            craftButton.interactable = false;
        }
        else
        {
            Debug.LogWarning("조합 버튼을 찾을 수 없습니다!");
        }

        // RecipeDatabase 자동 찾기
        if (recipeDatabase == null)
        {
            CraftingManager craftingManager = FindObjectOfType<CraftingManager>();
            if (craftingManager != null)
            {
                recipeDatabase = craftingManager.recipeDatabase;
                Debug.Log("RecipeDatabase 자동 찾기 완료");
            }
        }

        // InventoryUI 자동 찾기 (비활성화된 것도 포함)
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>(true);
            if (inventoryUI != null)
            {
                Debug.Log("InventoryUI 자동 찾기 완료");
            }
            else
            {
                Debug.LogError("InventoryUI를 찾을 수 없습니다! Scene에 InventoryUI 컴포넌트가 있는지 확인하세요.");
            }
        }

        // Inventory는 InventoryUI에서 가져오기 (같은 인스턴스 보장)
        if (inventory == null && inventoryUI != null)
        {
            inventory = inventoryUI.GetInventory();
            if (inventory != null)
            {
                Debug.Log($"[CombManager] InventoryUI에서 Inventory 가져오기 완료! 인스턴스 ID: {inventory.GetInstanceID()}");
                Debug.Log($"[CombManager] Inventory.items 개수: {inventory.items?.Count ?? 0}");

                // 아이템 내용 확인
                int itemCount = 0;
                if (inventory.items != null)
                {
                    foreach (var item in inventory.items)
                    {
                        if (item != null)
                        {
                            itemCount++;
                            Debug.Log($"[CombManager] Start에서 발견한 아이템: {item.itemName} x{item.amount}");
                        }
                    }
                }
                Debug.Log($"[CombManager] Start에서 null 아닌 아이템 개수: {itemCount}");
            }
            else
            {
                Debug.LogError("[CombManager] InventoryUI.GetInventory()가 null을 반환했습니다!");
            }
        }

        // 그래도 없으면 FindObjectOfType 시도
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>(true);
            if (inventory != null)
            {
                Debug.LogWarning($"Inventory 자동 찾기 완료 (InventoryUI와 다를 수 있음!) 인스턴스 ID: {inventory.GetInstanceID()}");
            }
            else
            {
                Debug.LogError("Inventory를 찾을 수 없습니다! Scene에 Inventory 컴포넌트가 있는지 확인하세요.");
            }
        }
    }

    void OnEnable()
    {
        Debug.Log("[CombManager] OnEnable 호출됨 - 조합창 활성화");

        // 조합창이 활성화될 때 다시 시도
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>(true);
            Debug.Log($"[CombManager] OnEnable에서 InventoryUI 찾기: {(inventoryUI != null ? "성공" : "실패")}");
        }

        // Inventory는 InventoryUI에서 가져오기 (같은 인스턴스 보장)
        if (inventoryUI != null)
        {
            inventory = inventoryUI.GetInventory();
            if (inventory != null)
            {
                Debug.Log($"[CombManager] OnEnable에서 Inventory 가져오기 완료! 인스턴스 ID: {inventory.GetInstanceID()}");
                Debug.Log($"[CombManager] OnEnable에서 Inventory.items 개수: {inventory.items?.Count ?? 0}");

                // 아이템 내용 확인
                int itemCount = 0;
                if (inventory.items != null)
                {
                    foreach (var item in inventory.items)
                    {
                        if (item != null)
                        {
                            itemCount++;
                            Debug.Log($"[CombManager] OnEnable에서 발견한 아이템: {item.itemName} x{item.amount}");
                        }
                    }
                }
                Debug.Log($"[CombManager] OnEnable에서 null 아닌 아이템 개수: {itemCount}");
            }
        }

        // 그래도 없으면 FindObjectOfType 시도
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>(true);
            Debug.LogWarning($"[CombManager] OnEnable에서 FindObjectOfType으로 Inventory 찾기: {(inventory != null ? "성공" : "실패")}");
        }
    }

    /// <summary>
    /// 인벤토리에서 조합 슬롯으로 드롭했을 때 호출
    /// </summary>
    public void OnDropToCombSlot(CombSlot targetSlot)
    {
        if (inventoryUI == null || !inventoryUI.IsDragging())
        {
            return;
        }

        // 드래그 중인 인벤토리 슬롯 가져오기
        InventorySlot draggedSlot = inventoryUI.GetDraggedSlot();

        if (draggedSlot == null || draggedSlot.item == null)
        {
            return;
        }

        // 조합 슬롯에 아이템 복사본 배치 (인벤토리에서는 제거 안 함)
        Item itemCopy = draggedSlot.item.GetCopy();
        itemCopy.amount = 1; // 조합에는 1개씩만 사용
        targetSlot.SetItem(itemCopy);

        // 레시피 확인
        CheckRecipe();
    }

    /// <summary>
    /// 현재 배치된 아이템으로 레시피 확인
    /// </summary>
    public void CheckRecipe()
    {
        currentRecipe = null;

        if (recipeDatabase == null)
        {
            Debug.LogError("RecipeDatabase가 없습니다!");
            UpdateCraftButton(false);
            return;
        }

        // 슬롯 1, 2에 아이템이 있는지 확인
        if (slot1 == null || slot2 == null || slot1.item == null || slot2.item == null)
        {
            Debug.Log("슬롯에 아이템이 부족합니다.");
            UpdateCraftButton(false);
            if (slot3 != null)
            {
                slot3.ClearSlot();
            }
            return;
        }

        Debug.Log($"조합 시도: {slot1.item.itemName} + {slot2.item.itemName}");

        // 배치된 아이템 목록
        List<Item> placedItems = new List<Item> { slot1.item, slot2.item };

        // 레시피 매칭
        foreach (Recipe recipe in recipeDatabase.allRecipes)
        {
            if (recipe == null || !recipe.IsValid())
            {
                continue;
            }

            // 재료가 2개인 레시피만 체크
            if (recipe.ingredients.Count != 2)
            {
                continue;
            }

            // 순서 상관없이 매칭 (이름 정규화: 공백 제거 + 소문자 변환)
            bool matched = false;

            string slot1Name = slot1.item.itemName.Trim().ToLower();
            string slot2Name = slot2.item.itemName.Trim().ToLower();
            string recipe0Name = recipe.ingredients[0].item.itemName.Trim().ToLower();
            string recipe1Name = recipe.ingredients[1].item.itemName.Trim().ToLower();

            // 경우 1: slot1=재료1, slot2=재료2
            if (slot1Name == recipe0Name && slot2Name == recipe1Name)
            {
                matched = true;
            }
            // 경우 2: slot1=재료2, slot2=재료1
            else if (slot1Name == recipe1Name && slot2Name == recipe0Name)
            {
                matched = true;
            }

            if (matched)
            {
                currentRecipe = recipe;
                Debug.Log($"레시피 찾음: {recipe.recipeName}");
                break;
            }
        }

        // 레시피를 찾았으면 결과 표시
        if (currentRecipe != null)
        {
            if (slot3 != null)
            {
                Item resultItem = currentRecipe.result.item.GetCopy();
                resultItem.amount = currentRecipe.result.resultAmount;
                slot3.SetItem(resultItem);
                Debug.Log($"결과 아이템 표시: {resultItem.itemName} x{resultItem.amount}");
            }

            // 인벤토리에 재료가 충분한지 확인
            if (HasEnoughIngredients())
            {
                Debug.Log("재료 충분! 버튼 활성화");
                UpdateCraftButton(true);
            }
            else
            {
                Debug.Log("재료 부족! 버튼 비활성화");
                UpdateCraftButton(false);
            }
        }
        else
        {
            Debug.Log("일치하는 레시피 없음");
            if (slot3 != null)
            {
                slot3.ClearSlot();
            }
            UpdateCraftButton(false);
        }
    }

    /// <summary>
    /// 인벤토리에 재료가 충분한지 확인
    /// </summary>
    private bool HasEnoughIngredients()
    {
        if (currentRecipe == null)
        {
            Debug.LogError("currentRecipe가 null입니다!");
            return false;
        }

        if (inventory == null)
        {
            Debug.LogError("inventory가 null입니다!");
            return false;
        }

        // 인벤토리 상태 디버깅
        Debug.Log($"=== 인벤토리 상태 확인 ===");
        Debug.Log($"Inventory 인스턴스: {inventory.GetInstanceID()}");
        Debug.Log($"Inventory.items가 null인가? {inventory.items == null}");
        Debug.Log($"Inventory.items 개수: {inventory.items?.Count ?? 0}");

        // 인벤토리 아이템 개수 확인
        Dictionary<string, int> inventoryItems = new Dictionary<string, int>();
        int itemCount = 0;
        foreach (Item item in inventory.items)
        {
            if (item != null)
            {
                itemCount++;
                // 이름 정규화: 공백 제거 + 소문자 변환
                string normalizedName = item.itemName.Trim().ToLower();

                Debug.Log($"인벤토리 아이템 발견: '{item.itemName}' (정규화: '{normalizedName}') x{item.amount}");

                if (inventoryItems.ContainsKey(normalizedName))
                {
                    inventoryItems[normalizedName] += item.amount;
                }
                else
                {
                    inventoryItems[normalizedName] = item.amount;
                }
            }
        }

        Debug.Log($"=== 인벤토리에서 null 아닌 아이템: {itemCount}개 ===");
        Debug.Log($"인벤토리 아이템 목록: {string.Join(", ", inventoryItems.Keys)}");

        // 레시피 재료가 충분한지 확인
        foreach (RecipeIngredient ingredient in currentRecipe.ingredients)
        {
            // 이름 정규화: 공백 제거 + 소문자 변환
            string normalizedIngredientName = ingredient.item.itemName.Trim().ToLower();

            Debug.Log($"필요한 재료: {ingredient.item.itemName} (정규화: {normalizedIngredientName}) x{ingredient.requiredAmount}");

            if (!inventoryItems.ContainsKey(normalizedIngredientName))
            {
                Debug.LogWarning($"인벤토리에 {ingredient.item.itemName}이(가) 없습니다! (정규화된 이름: '{normalizedIngredientName}')");
                Debug.LogWarning($"인벤토리에 있는 아이템들: {string.Join(", ", inventoryItems.Keys)}");
                return false;
            }

            int available = inventoryItems[normalizedIngredientName];
            Debug.Log($"인벤토리에 있는 {ingredient.item.itemName}: {available}개");

            if (available < ingredient.requiredAmount)
            {
                Debug.LogWarning($"{ingredient.item.itemName}이(가) 부족합니다! (필요: {ingredient.requiredAmount}, 보유: {available})");
                return false;
            }
        }

        Debug.Log("모든 재료 충분!");
        return true;
    }

    /// <summary>
    /// 조합 버튼 클릭 시 호출
    /// </summary>
    private void OnCraftButtonClicked()
    {
        if (currentRecipe == null)
        {
            Debug.Log("조합할 레시피가 없습니다.");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("[CombManager] inventory가 null입니다! 조합 불가능");
            return;
        }

        CraftingManager craftingManager = FindObjectOfType<CraftingManager>();
        if (craftingManager == null)
        {
            Debug.LogError("CraftingManager를 찾을 수 없습니다.");
            return;
        }

        // CraftingManager가 올바른 Inventory를 참조하도록 설정
        Debug.Log($"[CombManager] 조합 버튼 클릭! CombManager Inventory ID: {inventory.GetInstanceID()}");
        Debug.Log($"[CombManager] CraftingManager Inventory ID: {(craftingManager.inventory != null ? craftingManager.inventory.GetInstanceID().ToString() : "null")}");

        // CraftingManager의 inventory를 CombManager와 동일한 것으로 설정
        craftingManager.inventory = inventory;
        Debug.Log($"[CombManager] CraftingManager에 Inventory 설정 완료");

        // 조합 실행
        bool success = craftingManager.CraftItem(currentRecipe);

        if (success)
        {
            Debug.Log($"{currentRecipe.recipeName} 조합 성공!");
            ClearAllSlots();
        }
        else
        {
            Debug.Log("조합 실패");
        }
    }

    /// <summary>
    /// 모든 슬롯 비우기
    /// </summary>
    public void ClearAllSlots()
    {
        if (slot1 != null) slot1.ClearSlot();
        if (slot2 != null) slot2.ClearSlot();
        if (slot3 != null) slot3.ClearSlot();

        currentRecipe = null;
        UpdateCraftButton(false);
    }

    /// <summary>
    /// 조합 버튼 활성화 상태 업데이트
    /// </summary>
    private void UpdateCraftButton(bool interactable)
    {
        if (craftButton != null)
        {
            craftButton.interactable = interactable;
        }
    }
}
