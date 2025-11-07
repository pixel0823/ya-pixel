using UnityEngine;
using Photon.Pun;
using UnityEngine.U2D.Animation; // SpriteResolver와 SpriteLibraryAsset을 사용하기 위해 추가

/// <summary>
/// 아이템 사용 및 장착된 아이템 표시 로직을 담당합니다.
/// </summary>
public class PlayerItemUse : MonoBehaviourPunCallbacks
{
    private Animator animator;
    private Inventory inventory;
    private InventoryUI inventoryUI;

    [Header("오브젝트 레퍼런스")]
    [Tooltip("도구가 아닐 때 아이템 아이콘을 표시할 SpriteRenderer")]
    public SpriteRenderer heldItemRenderer;
    [Tooltip("도구 애니메이션에 사용될 SpriteResolver. 도구를 렌더링하는 자식 오브젝트에 있어야 합니다.")]
    public SpriteResolver toolAnimationResolver;

    private int selectedSlot = -1;

    void Awake()
    {
        animator = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        inventoryUI = FindObjectOfType<InventoryUI>();

        // 초기 상태 설정
        if (heldItemRenderer != null) heldItemRenderer.sprite = null;
        if (toolAnimationResolver != null) toolAnimationResolver.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (photonView != null && !photonView.IsMine && PhotonNetwork.InRoom)
        {
            return;
        }

        // 인벤토리 UI에서 선택된 슬롯을 가져옵니다.
        if (inventoryUI != null)
        {
            selectedSlot = inventoryUI.selectedSlot;
        }

        // 매 프레임 장착된 아이템 표시를 업데이트하여 애니메이션과 동기화합니다.
        UpdateEquippedItem();

        // 우클릭으로 아이템 사용 (귀환석 등)
        HandleItemUse();
    }

    /// <summary>
    /// 우클릭으로 들고 있는 아이템을 사용합니다.
    /// </summary>
    void HandleItemUse()
    {
        // 우클릭 감지
        if (Input.GetMouseButtonDown(1)) // 1 = 우클릭
        {
            Item selectedItem = GetSelectedItem();

            if (selectedItem == null)
            {
                return; // 아이템을 들고 있지 않음
            }

            Debug.Log($"[PlayerItemUse] 우클릭! 현재 아이템: {selectedItem.itemName} (타입: {selectedItem.GetType().Name})");

            // ReturnStone인지 확인
            if (selectedItem is ReturnStone returnStone)
            {
                Debug.Log($"[PlayerItemUse] ✅ ReturnStone 감지!");

                // 귀환석 사용
                bool success = returnStone.Use(gameObject);

                if (success)
                {
                    Debug.Log($"[PlayerItemUse] 귀환석 사용 성공!");

                    // 소모품이면 인벤토리에서 제거
                    if (returnStone.isConsumable && inventory != null && selectedSlot >= 0)
                    {
                        Item slotItem = inventory.items[selectedSlot];
                        if (slotItem != null)
                        {
                            slotItem.amount--;
                            if (slotItem.amount <= 0)
                            {
                                inventory.Remove(selectedSlot);
                            }
                            inventory.onItemChangedCallback?.Invoke();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlayerItemUse] 귀환석 사용 실패!");
                }
            }
            else
            {
                Debug.Log($"[PlayerItemUse] {selectedItem.itemName}은(는) 우클릭으로 사용할 수 없는 아이템입니다.");
            }
        }
    }

    /// <summary>
    /// 선택된 아이템에 따라 손에 들린 아이템 표시를 업데이트합니다.
    /// </summary>
    void UpdateEquippedItem()
    {
        Item selectedItem = GetSelectedItem();

        // 도구와 비-도구 렌더러 처리
        bool isTool = selectedItem != null && selectedItem.isTool;

        if (heldItemRenderer != null)
        {
            heldItemRenderer.sprite = isTool ? null : selectedItem?.icon;
        }

        if (toolAnimationResolver != null)
        {
            toolAnimationResolver.gameObject.SetActive(isTool);
            if (isTool) // isTool이 true일 때만 내부 로직 실행
            {
                if (selectedItem.toolSpriteLibrary != null)
                {
                    SpriteLibrary spriteLibrary = toolAnimationResolver.spriteLibrary;
                    if (spriteLibrary != null)
                    {
                        // 1. 스프라이트 라이브러리 할당
                        if (spriteLibrary.spriteLibraryAsset != selectedItem.toolSpriteLibrary)
                        {
                            spriteLibrary.spriteLibraryAsset = selectedItem.toolSpriteLibrary;

                        }

                        // 2. 카테고리 설정
                        if (!string.IsNullOrEmpty(selectedItem.toolCategory))
                        {
                            string currentLabel = toolAnimationResolver.GetLabel();

                            // currentLabel이 유효한 경우에만 카테고리 변경 시도
                            if (!string.IsNullOrEmpty(currentLabel))
                            {
                                // 현재 카테고리와 설정하려는 카테고리가 다를 때만 변경
                                if (toolAnimationResolver.GetCategory() != selectedItem.toolCategory)
                                {
                                    toolAnimationResolver.SetCategoryAndLabel(selectedItem.toolCategory, currentLabel);


                                    string newCategory = toolAnimationResolver.GetCategory();
                                    if (newCategory == selectedItem.toolCategory)
                                    {

                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            // else: currentLabel이 비어있다면, 아직 애니메이터가 준비되지 않은 것이므로 이번 프레임은 건너뜁니다.
                        }
                        else
                        {

                        }
                    }
                    else
                    {

                    }
                }
            }
        }

    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템 정보를 가져옵니다.
    /// </summary>
    public Item GetSelectedItem()
    {
        if (inventory == null || selectedSlot < 0 || selectedSlot >= inventory.items.Count)
        {
            return null;
        }
        return inventory.items[selectedSlot];
    }
}