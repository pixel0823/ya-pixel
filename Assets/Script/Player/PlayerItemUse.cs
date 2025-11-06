using UnityEngine;
using Photon.Pun;

/// <summary>
/// 나중에 아이템 사용 관련 로직 구현 예정
/// </summary>
public class PlayerItemUse : MonoBehaviourPunCallbacks
{
    [Header("테스트 설정")]
    [Tooltip("게임 시작 시 자동으로 귀환석을 추가할지 여부")]
    public bool addReturnStoneOnStart = true;

    [Tooltip("귀환석 아이템 (ScriptableObject)")]
    public ReturnStone returnStoneItem;

    private Animator animator;
    private Inventory inventory; // 인벤토리 컴포넌트 참조
    private InventoryUI inventoryUI; // 인벤토리 UI 참조

    void Awake()
    {
        animator = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        // 씬에 있는 InventoryUI 오브젝트를 찾습니다.
        inventoryUI = FindObjectOfType<InventoryUI>();
    }

    void Start()
    {
        // 테스트용: 게임 시작 시 귀환석 자동 추가
        if (addReturnStoneOnStart && returnStoneItem != null)
        {
            AddReturnStone();
        }
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine && PhotonNetwork.InRoom)
        {
            return;
        }

        // 테스트용 치트키: P 키를 누르면 귀환석 추가
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddReturnStone();
        }
    }

    /// <summary>
    /// 테스트용: 인벤토리에 귀환석을 추가합니다.
    /// </summary>
    private void AddReturnStone()
    {
        if (inventory == null)
        {
            Debug.LogError("[PlayerItemUse] Inventory가 없습니다.");
            return;
        }

        if (returnStoneItem != null)
        {
            bool success = inventory.Add(returnStoneItem, 1);
            if (success)
            {
                Debug.Log("[PlayerItemUse] ✅ 귀환석이 인벤토리에 추가되었습니다!");
            }
            else
            {
                Debug.LogWarning("[PlayerItemUse] ❌ 귀환석 추가 실패. 인벤토리가 가득 찼습니다.");
            }
        }
        else
        {
            Debug.LogError("[PlayerItemUse] returnStoneItem이 할당되지 않았습니다! Inspector에서 귀환석을 드래그하세요.");
        }
    }
}
