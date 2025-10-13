using UnityEngine;

/// <summary>
/// 테스트 목적으로 특정 키를 누르면 아이템을 드랍하는 스크립트입니다.
/// </summary>
public class ItemDropTester : MonoBehaviour
{
    [Header("테스트 설정")]
    [Tooltip("드랍할 아이템들의 ScriptableObject 애셋 배열")]
    public Item[] itemsToDrop;

    [Tooltip("아이템을 드랍할 주체(플레이어)의 인벤토리")]
    public Inventory targetInventory;

    [Tooltip("테스트에 사용할 키")]
    public KeyCode testKey = KeyCode.T;

    void Update()
    {
        // 지정된 테스트 키가 눌렸는지 확인
        if (Input.GetKeyDown(testKey))
        {
            // 타겟 인벤토리가 할당되지 않았다면, 씬에서 찾아봅니다.
            if (targetInventory == null)
            {
                targetInventory = FindObjectOfType<Inventory>();
            }

            // 할당된 아이템과 인벤토리가 있는지 확인
            if (itemsToDrop == null || itemsToDrop.Length == 0)
            {
                Debug.LogError("드랍할 아이템이 지정되지 않았습니다!");
                return;
            }
            if (targetInventory == null)
            {
                Debug.LogError("타겟 인벤토리를 찾을 수 없습니다! 씬에 플레이어가 생성되었는지 확인해주세요.");
                return;
            }

            Debug.Log($"테스트: 아이템 {itemsToDrop.Length}개를 강제로 드랍합니다...");

            // DropItem 함수는 인벤토리에 아이템이 있는지 먼저 확인합니다.
            // 따라서 확실한 테스트를 위해, 먼저 인벤토리에 아이템을 추가한 후 바로 드랍합니다.
            foreach (var itemToDrop in itemsToDrop)
            {
                if (itemToDrop != null)
                {
                    int addedIndex = targetInventory.Add(itemToDrop);
                    if (addedIndex != -1)
                    {
                        targetInventory.DropItem(addedIndex);
                    }
                }
            }
        }
    }
}