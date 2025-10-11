using UnityEngine;
using Photon.Pun;

/// <summary>
/// 플레이어의 아이템 '드랍' 기능을 관리합니다. (줍는 기능은 PlayerInteraction으로 이전)
/// </summary>
public class ItemController : MonoBehaviourPunCallbacks
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool canControl = false;
        if (PhotonNetwork.InRoom)
        {
            if (photonView.IsMine) canControl = true;
        }
        else
        {
            canControl = true;
        }

        if (!canControl) return;

        // --- 아이템 드랍 테스트 ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) DropItem("PotionItem");
        if (Input.GetKeyDown(KeyCode.Alpha2)) DropItem("SwordItem");
        // --- 테스트 끝 ---
    }

    private void DropItem(string itemPrefabName)
    {
        // 드랍 위치를 플레이어의 발밑보다 약간 위로 설정
        float yOffset = 0.5f; // Y축 오프셋 값, 필요에 따라 조절하세요.
        Vector3 dropPosition = transform.position + new Vector3(0, yOffset, 0);

        GameObject itemPrefab = Resources.Load<GameObject>(itemPrefabName);

        if (itemPrefab == null)
        {
            Debug.LogError($"{itemPrefabName} 프리팹을 Resources 폴더에서 찾을 수 없습니다.");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate(itemPrefabName, dropPosition, Quaternion.identity);
        }
        else
        {
            Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        }
        Debug.Log($"{itemPrefabName} 아이템을 드랍했습니다.");
    }
}
