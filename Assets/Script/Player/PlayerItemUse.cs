using UnityEngine;
using Photon.Pun;

/// <summary>
/// 나중에 아이템 사용 관련 로직 구현 예정
/// </summary>
public class ItemController : MonoBehaviourPunCallbacks
{
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

    void Update()
    {
        if (photonView != null && !photonView.IsMine && PhotonNetwork.InRoom)
        {
            return;
        }


    }
}
