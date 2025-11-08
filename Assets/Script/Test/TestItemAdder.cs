using UnityEngine;

/// <summary>
/// 테스트용 아이템 추가 스크립트
/// </summary>
public class TestItemAdder : MonoBehaviour
{
    [Header("테스트 아이템 설정")]
    [Tooltip("게임 시작 시 추가할 아이템들")]
    public Item[] itemsToAdd;

    [Tooltip("게임 시작 시 자동으로 아이템 추가")]
    public bool addItemsOnStart = true;

    private Inventory playerInventory;

    void Start()
    {
        if (addItemsOnStart)
        {
            Invoke("AddTestItems", 1f); // 1초 후에 추가 (플레이어 생성 대기)
        }
    }

    void Update()
    {
        // P키: 테스트 아이템 추가
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddTestItems();
        }
    }

    /// <summary>
    /// 테스트 아이템을 플레이어 인벤토리에 추가합니다.
    /// </summary>
    private void AddTestItems()
    {
        // 플레이어 찾기
        if (playerInventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
            }
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("[TestItemAdder] 플레이어 인벤토리를 찾을 수 없습니다.");
            return;
        }

        // 아이템 추가
        if (itemsToAdd != null && itemsToAdd.Length > 0)
        {
            foreach (Item item in itemsToAdd)
            {
                if (item != null)
                {
                    bool success = playerInventory.Add(item, 1);
                    if (success)
                    {
                        Debug.Log($"[TestItemAdder] ✅ '{item.itemName}' 추가 성공!");
                    }
                    else
                    {
                        Debug.LogWarning($"[TestItemAdder] ❌ '{item.itemName}' 추가 실패 (인벤토리 가득참)");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[TestItemAdder] 추가할 아이템이 없습니다.");
        }
    }
}
