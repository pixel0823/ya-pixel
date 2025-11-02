using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상자 UI를 관리하는 클래스
/// ChestListPanel: 상자 아이템 슬롯 20개
/// ItemListPanel: 플레이어 인벤토리 슬롯 20개
/// </summary>
public class ChestUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject chestPanel;           // ChestPanel (루트)
    public GameObject chestUI;              // ChestUI (서브 패널)
    public GameObject chestListPanel;       // 상자 아이템 슬롯 부모 (Grid Layout)
    public GameObject itemListPanel;        // 인벤토리 슬롯 부모 (Grid Layout)

    [Header("References")]
    private Inventory playerInventory;      // 플레이어 인벤토리
    private LootBox currentLootBox;         // 현재 열린 상자

    private InventorySlot[] chestSlots;     // 상자 슬롯 배열 (20개)
    private InventorySlot[] inventorySlots; // 인벤토리 슬롯 배열 (20개)
    private List<Item> chestItems;          // 상자에 있는 아이템 목록 (최대 20개)
    private bool isInitialized = false;     // 초기화 여부

    void Awake()
    {
        // 초기화는 실제 사용할 때만 (OpenChest 시)
        chestItems = new List<Item>();

        // ChestPanel은 시작 시 비활성화
        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }

        Debug.Log("[ChestUI] Awake 완료");
    }

    /// <summary>
    /// ChestUI 초기화 (OpenChest에서만 호출 - 게임 시작 시 아님!)
    /// </summary>
    private void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("[ChestUI] 이미 초기화됨, 건너뜀");
            return;
        }

        Debug.Log("[ChestUI] 초기화 시작... (상자 열 때만 실행)");

        // 상자 아이템 리스트 초기화
        if (chestItems == null)
        {
            chestItems = new List<Item>();
        }

        // 패널에서 슬롯들 찾기 (상자 열 때만 실행되므로 안전)
        if (chestListPanel != null)
        {
            chestSlots = chestListPanel.GetComponentsInChildren<InventorySlot>(true);
            Debug.Log($"[ChestUI] 상자 슬롯 {chestSlots.Length}개 로드됨");
        }
        else
        {
            Debug.LogError("[ChestUI] ChestListPanel이 할당되지 않았습니다!", this);
            chestSlots = new InventorySlot[0];
        }

        if (itemListPanel != null)
        {
            inventorySlots = itemListPanel.GetComponentsInChildren<InventorySlot>(true);
            Debug.Log($"[ChestUI] 인벤토리 슬롯 {inventorySlots.Length}개 로드됨");
        }
        else
        {
            Debug.LogError("[ChestUI] ItemListPanel이 할당되지 않았습니다!", this);
            inventorySlots = new InventorySlot[0];
        }

        isInitialized = true;
        Debug.Log("[ChestUI] 초기화 완료!");
    }

    void Update()
    {
        // ESC 키로 상자 UI 닫기
        if (IsChestOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChest();
        }
    }

    /// <summary>
    /// 상자 UI를 엽니다
    /// </summary>
    public void OpenChest(LootBox lootBox)
    {
        // 초기화 확인 (Awake가 실행되지 않았을 경우 대비)
        Initialize();

        if (lootBox == null)
        {
            Debug.LogError("[ChestUI] LootBox가 null입니다!");
            return;
        }

        if (lootBox.lootTable == null)
        {
            Debug.LogError("[ChestUI] LootBox에 LootTable이 할당되지 않았습니다!", lootBox);
            return;
        }

        // 플레이어 인벤토리 찾기
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory == null)
            {
                Debug.LogError("[ChestUI] 플레이어 Inventory를 찾을 수 없습니다!");
                return;
            }
        }

        currentLootBox = lootBox;

        // 상자 아이템 가져오기 (캐싱됨 - 한 번만 생성)
        // LootBox의 cachedItems를 직접 참조하지 않고 매번 새로 복사
        List<ItemDrop> cachedLootItems = lootBox.GetLootItems();
        chestItems.Clear();

        // cachedItems를 Item 리스트로 변환
        foreach (var drop in cachedLootItems)
        {
            if (drop != null && drop.item != null)
            {
                Item itemCopy = drop.item.GetCopy();
                itemCopy.amount = drop.amount;
                chestItems.Add(itemCopy);
            }
            else
            {
                // null 아이템도 인덱스 유지를 위해 추가
                chestItems.Add(null);
            }
        }

        // UI 업데이트
        UpdateChestSlots();
        UpdateInventorySlots();

        // 상자 슬롯에 클릭 이벤트 등록
        AssignChestSlotClickHandlers();

        // UI 패널 열기 (부모와 자식 모두 활성화)
        if (chestPanel != null)
        {
            chestPanel.SetActive(true);
        }

        if (chestUI != null)
        {
            chestUI.SetActive(true);
        }

        Debug.Log($"[ChestUI] 상자 UI 열림: {chestItems.Count}개 아이템 생성");
    }

    /// <summary>
    /// 상자 UI를 닫습니다
    /// </summary>
    public void CloseChest()
    {
        // UI 패널 닫기 (자식부터 닫고 부모 닫기)
        if (chestUI != null)
        {
            chestUI.SetActive(false);
        }

        if (chestPanel != null)
        {
            chestPanel.SetActive(false);
        }

        // 데이터 정리 (currentLootBox는 유지 - 재오픈 시 캐싱 사용)
        // chestItems는 비우지만 LootBox의 cachedItems는 유지됨
        chestItems.Clear();
        currentLootBox = null;

        Debug.Log("[ChestUI] 상자 UI 닫힘");
    }

    /// <summary>
    /// 상자 슬롯 UI 업데이트
    /// </summary>
    private void UpdateChestSlots()
    {
        for (int i = 0; i < chestSlots.Length; i++)
        {
            if (i < chestItems.Count && chestItems[i] != null)
            {
                chestSlots[i].item = chestItems[i];
            }
            else
            {
                chestSlots[i].item = null;
            }
            chestSlots[i].UpdateSlotUI();
        }
    }

    /// <summary>
    /// 인벤토리 슬롯 UI 업데이트
    /// </summary>
    private void UpdateInventorySlots()
    {
        if (playerInventory == null) return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < playerInventory.items.Count)
            {
                inventorySlots[i].item = playerInventory.items[i];
            }
            else
            {
                inventorySlots[i].item = null;
            }
            inventorySlots[i].UpdateSlotUI();
        }
    }

    /// <summary>
    /// 상자 슬롯에 클릭 이벤트 등록
    /// ChestSlotHandler 컴포넌트를 추가하여 클릭 이벤트 처리
    /// </summary>
    private void AssignChestSlotClickHandlers()
    {
        for (int i = 0; i < chestSlots.Length; i++)
        {
            // ChestSlotHandler 추가 또는 가져오기
            ChestSlotHandler handler = chestSlots[i].GetComponent<ChestSlotHandler>();
            if (handler == null)
            {
                handler = chestSlots[i].gameObject.AddComponent<ChestSlotHandler>();
            }

            // ChestUI와 슬롯 인덱스 설정
            handler.Initialize(this, i);
        }
    }

    /// <summary>
    /// 상자 슬롯 클릭 시 호출 (ChestSlotHandler에서 호출)
    /// </summary>
    public void OnChestSlotClicked(int slotIndex)
    {
        if (slotIndex >= chestItems.Count || chestItems[slotIndex] == null)
        {
            Debug.Log("빈 슬롯입니다.");
            return;
        }

        Item itemToTake = chestItems[slotIndex];

        // 플레이어 인벤토리에 추가 시도
        bool added = playerInventory.Add(itemToTake, itemToTake.amount);

        if (added)
        {
            // 상자에서 아이템 제거 (ChestUI)
            chestItems[slotIndex] = null;

            // LootBox의 cachedItems에서도 제거 (영구 제거)
            if (currentLootBox != null)
            {
                currentLootBox.RemoveItemAt(slotIndex);
            }

            // UI 업데이트
            UpdateChestSlots();
            UpdateInventorySlots();

            Debug.Log($"{itemToTake.itemName} x{itemToTake.amount} 획득!");

            // 상자가 완전히 비었는지 확인
            CheckIfChestIsEmpty();
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
        }
    }

    /// <summary>
    /// 상자가 완전히 비었는지 확인하고 처리
    /// </summary>
    private void CheckIfChestIsEmpty()
    {
        bool isEmpty = true;
        foreach (var item in chestItems)
        {
            if (item != null)
            {
                isEmpty = false;
                break;
            }
        }

        if (isEmpty && currentLootBox != null)
        {
            Debug.Log("[ChestUI] 상자가 비었습니다!");
            currentLootBox.MarkAsOpened();
        }
    }

    /// <summary>
    /// 모든 아이템을 한 번에 가져가기
    /// </summary>
    public void TakeAllItems()
    {
        for (int i = chestItems.Count - 1; i >= 0; i--)
        {
            if (chestItems[i] != null)
            {
                Item itemToTake = chestItems[i];
                bool added = playerInventory.Add(itemToTake, itemToTake.amount);

                if (added)
                {
                    // ChestUI에서 제거
                    chestItems[i] = null;

                    // LootBox의 cachedItems에서도 제거
                    if (currentLootBox != null)
                    {
                        currentLootBox.RemoveItemAt(i);
                    }
                }
            }
        }

        UpdateChestSlots();
        UpdateInventorySlots();

        // 상자가 비었는지 확인
        CheckIfChestIsEmpty();
    }

    /// <summary>
    /// 상자가 열려있는지 확인
    /// </summary>
    public bool IsChestOpen()
    {
        // ChestPanel과 ChestUI 둘 다 활성화 상태여야 열린 것
        bool panelActive = chestPanel != null && chestPanel.activeSelf;
        bool uiActive = chestUI != null && chestUI.activeSelf;
        return panelActive && uiActive;
    }

    /// <summary>
    /// 현재 열려있는 LootBox 반환
    /// </summary>
    public LootBox GetCurrentLootBox()
    {
        return currentLootBox;
    }
}
