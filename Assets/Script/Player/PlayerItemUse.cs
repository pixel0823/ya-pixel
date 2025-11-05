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
                            Debug.Log($"PlayerItemUse: SpriteLibraryAsset을 '{selectedItem.toolSpriteLibrary.name}' (으)로 변경했습니다.");
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
                                    Debug.Log($"PlayerItemUse: Resolver 카테고리를 '{selectedItem.toolCategory}', 레이블을 '{currentLabel}' (으)로 설정 시도.");

                                    string newCategory = toolAnimationResolver.GetCategory();
                                    if (newCategory == selectedItem.toolCategory)
                                    {
                                        Debug.Log("PlayerItemUse: 카테고리 변경 성공!");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"PlayerItemUse: 카테고리 변경 실패! 원인: '{selectedItem.toolCategory}' 카테고리 안에 '{currentLabel}' 레이블이 없는 것 같습니다. SpriteLibraryAsset 편집기에서 레이블 이름을 직접 확인해주세요.");
                                    }
                                }
                            }
                            // else: currentLabel이 비어있다면, 아직 애니메이터가 준비되지 않은 것이므로 이번 프레임은 건너뜁니다.
                        }
                        else
                        {
                            Debug.LogWarning("PlayerItemUse: Item의 toolCategory가 비어있거나 설정되지 않았습니다. Inspector를 확인해주세요!");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("PlayerItemUse: toolAnimationResolver 오브젝트에 SpriteLibrary 컴포넌트가 없습니다!");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerItemUse: Inspector에서 toolAnimationResolver가 할당되지 않았습니다!");
        }
    }

    /// <summary>
    /// 현재 선택된 슬롯의 아이템 정보를 가져옵니다.
    /// </summary>
    Item GetSelectedItem()
    {
        if (inventory == null || selectedSlot < 0 || selectedSlot >= inventory.items.Count)
        {
            return null;
        }
        return inventory.items[selectedSlot];
    }
}