using UnityEngine;
using Photon.Pun;
using UnityEngine.U2D.Animation; // SpriteResolver와 SpriteLibraryAsset을 사용하기 위해 추가
using System.Collections;

/// <summary>
/// 아이템 사용 및 장착된 아이템 표시, 도구 사용(공격/채집) 로직을 담당합니다.
/// </summary>
public class PlayerItemUse : MonoBehaviourPunCallbacks
{
    private Animator animator;
    private Inventory inventory;
    private InventoryUI inventoryUI;
    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    [Header("오브젝트 레퍼런스")]
    [Tooltip("도구가 아닐 때 아이템 아이콘을 표시할 SpriteRenderer")]
    public SpriteRenderer heldItemRenderer;
    [Tooltip("도구 애니메이션에 사용될 SpriteResolver. 도구를 렌더링하는 자식 오브젝트에 있어야 합니다.")]
    public SpriteResolver toolAnimationResolver;

    [Header("도구 설정")]
    [Tooltip("도구의 히트 판정 반경")]
    public float toolHitRadius = 0.5f;
    [Tooltip("공격(채집) 애니메이션의 길이(초)")]
    public float attackAnimationTime = 0.5f;
    [Tooltip("올바른 도구 사용 시 데미지")]
    public int toolDamageCorrect = 5;
    [Tooltip("잘못된 도구 사용 시 데미지")]
    public int toolDamageIncorrect = 1;
    [Tooltip("맨손 공격 시 데미지")]
    public int toolDamageBareHand = 1;

    private int selectedSlot = -1;
    private bool _isAttackReady = true; // 공격 가능 상태를 나타내는 플래그

    /// <summary>
    /// 현재 아이템과 요구 도구 타입에 따라 적절한 데미지 값을 반환합니다.
    /// </summary>
    public int GetToolDamage(Item currentItem, ToolType requiredToolType)
    {
        // 규칙:
        // - currentItem == null 또는 비도구: 1 데미지
        // - 도구이고 requiredToolType과 일치: 도구의 attackPower 전부
        // - 도구이고 requiredToolType과 불일치(다른 도구): 도구 attackPower의 절반(최소 1)
        if (currentItem == null || !currentItem.isTool)
        {
            return 1;
        }

        // 도구인 경우
        if (requiredToolType != ToolType.None)
        {
            if (currentItem.toolType == requiredToolType)
            {
                return Mathf.Max(1, currentItem.attackPower);
            }
            else
            {
                return Mathf.Max(1, Mathf.FloorToInt(currentItem.attackPower / 2f));
            }
        }

        // requiredToolType가 None인 경우(대상에 특정 도구가 요구되지 않음)에는 도구의 전체 공격력을 적용
        return Mathf.Max(1, currentItem.attackPower);
    }

    /// <summary>
    /// 몬스터에 대한 데미지 계산: 도구이면 attackPower의 절반, 아니면 1
    /// </summary>
    public float GetDamageToMonster(Item currentItem)
    {
        if (currentItem != null && currentItem.isTool)
        {
            return Mathf.Max(1f, currentItem.attackPower * 0.5f);
        }
        return 1f;
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        inventoryUI = FindObjectOfType<InventoryUI>();

        // 초기 상태 설정
        if (heldItemRenderer != null) heldItemRenderer.sprite = null;
        if (toolAnimationResolver != null) toolAnimationResolver.gameObject.SetActive(false);
        // 플레이어 컴포넌트 참조
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();
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

        // 좌클릭으로 도구 사용
        HandleToolUse();

        // 우클릭으로 아이템 사용 (귀환석 등)
        HandleItemUse();
    }

    /// <summary>
    /// 좌클릭으로 들고 있는 도구를 사용합니다. (공격/채집)
    /// </summary>
    void HandleToolUse()
    {
        Item selectedItem = GetSelectedItem();

        // 무기(도구)가 있든 없든 공격 동작을 허용합니다 (맨손 공격 포함)
        if (Input.GetMouseButton(0) && _isAttackReady)
        {
            StartCoroutine(PerformAttack());
        }
    }

    /// <summary>
    /// 공격 애니메이션, 히트 판정, 재공격 딜레이를 처리하는 코루틴입니다.
    /// </summary>
    private IEnumerator PerformAttack()
    {
        _isAttackReady = false; // 공격 중에는 다시 공격할 수 없도록 설정

        animator.SetTrigger("Attack");
        Debug.Log("[PlayerItemUse] Attack 애니메이션 트리거 실행.");

        // 애니메이션 시간만큼 대기
        yield return new WaitForSeconds(attackAnimationTime);

        // 히트 판정 실행
        PerformHitDetection();

        // 공격이 끝났으므로 다시 공격 가능 상태로 변경
        _isAttackReady = true;
    }

    /// <summary>
    /// toolAnimationResolver 위치에서 원형으로 충돌을 감지하여 WorldObject에 데미지를 줍니다.
    /// </summary>
    private void PerformHitDetection()
    {
        // 공격 기준 위치: 툴 애니메이터 위치가 활성화되어 있으면 그 위치 사용, 아니면 플레이어의 바라보는 방향 기준으로 앞을 사용
        Vector2 attackOrigin;
        if (toolAnimationResolver != null && toolAnimationResolver.gameObject.activeInHierarchy)
        {
            attackOrigin = toolAnimationResolver.transform.position;
        }
        else
        {
            Vector2 dir = Vector2.down; // 기본값
            if (playerMovement != null)
            {
                dir = new Vector2(playerMovement.LastMoveX, playerMovement.LastMoveY);
                if (dir == Vector2.zero) dir = Vector2.down;
            }
            attackOrigin = (Vector2)transform.position + dir.normalized * 0.3f;
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackOrigin, toolHitRadius);
        Debug.Log($"[PlayerItemUse] 히트 판정! {hitColliders.Length}개의 콜라이더 감지. origin={attackOrigin}");

        Item selectedItem = GetSelectedItem();
        bool anyHit = false;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider == null) continue;

            // 몬스터 처리
            MonsterAI monster = hitCollider.GetComponent<MonsterAI>();
            if (monster != null && monster.photonView != null)
            {
                float damageToDeal = GetDamageToMonster(selectedItem);

                // 마스터 클라이언트에서 실제 체력 처리하도록 요청
                monster.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damageToDeal);
                Debug.Log($"[PlayerItemUse] 몬스터 {monster.name}에게 {damageToDeal} 데미지 요청");
                anyHit = true;
                continue; // 한 콜라이더에서 몬스터와 오브젝트가 동시에 있는 경우를 피함
            }

            // 월드 오브젝트(채광 등)
            HarvestableObject harvest = hitCollider.GetComponent<HarvestableObject>();
            if (harvest != null)
            {
                int damageToDeal = 1;
                if (selectedItem != null && selectedItem.isTool)
                {
                    // 도구로 채광하면 도구 공격력 전부 적용
                    damageToDeal = selectedItem.attackPower;
                }
                else
                {
                    damageToDeal = 1; // 맨손은 1
                }

                harvest.TakeDamage(damageToDeal);
                Debug.Log($"[PlayerItemUse] 월드 오브젝트 '{harvest.name}'에 {damageToDeal} 데미지 적용");
                anyHit = true;
                continue;
            }

            // 일반 WorldObject 인터랙션
            WorldObject worldObject = hitCollider.GetComponent<WorldObject>();
            if (worldObject != null)
            {
                Debug.Log($"[PlayerItemUse] WorldObject '{worldObject.name}' 발견! Interact 호출.");
                worldObject.Interact(this.gameObject);
                anyHit = true;
            }
        }

        // 공격 성공시 배고픔 감소 (StatusManager를 통해 처리)
        if (anyHit && StatusManager.Instance != null)
        {
            StatusManager.Instance.OnAttack();
        }
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

    // Gizmos를 사용하여 히트 판정 범위를 시각적으로 표시합니다.
    private void OnDrawGizmosSelected()
    {
        if (toolAnimationResolver == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(toolAnimationResolver.transform.position, toolHitRadius);
    }
}