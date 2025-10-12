using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어의 입력을 받아 Inventory 및 InventoryUI와 상호작용합니다.
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
