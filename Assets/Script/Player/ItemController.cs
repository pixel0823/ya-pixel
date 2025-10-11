using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어의 입력을 받아 Inventory를 제어합니다. (아이템 드랍 등)
/// </summary>
public class ItemController : MonoBehaviourPunCallbacks
{
    private Animator animator;
    private Inventory inventory; // 인벤토리 컴포넌트 참조

    void Awake()
    {
        animator = GetComponent<Animator>();
        inventory = GetComponent<Inventory>(); // 동일한 게임오브젝트에 있는 Inventory 컴포넌트를 가져옵니다.
    }

    void Update()
    {
        // 자신의 캐릭터가 아니면 조작할 수 없습니다.
        if (photonView != null && !photonView.IsMine && PhotonNetwork.InRoom)
        {
            return;
        }

        // 'Q' 키를 누르면 아이템을 드랍합니다.
        // (현재는 테스트를 위해 첫 번째 아이템을 드랍합니다. 나중에 선택된 아이템을 드랍하도록 변경해야 합니다.)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (inventory != null && inventory.items.Count > 0)
            {
                // 인벤토리의 첫 번째 아이템을 드랍합니다.
                Debug.Log($"Attempting to drop: {inventory.items[0].itemName}");
                inventory.DropItem(inventory.items[0]);
            }
            else
            {
                Debug.Log("Inventory is empty or could not be found.");
            }
        }
    }
}
