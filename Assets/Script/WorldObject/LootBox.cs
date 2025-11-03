using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LootBox : MonoBehaviourPun, IInteractable
{
    [Header("Loot Settings")]
    public LootTable lootTable;  // Inspector에서 할당할 LootTable
    public string boxName = "상자";  // 상자 이름

    [Header("State")]
    public bool isOpened = false;  // 열린 상태

    [Header("Visual (Optional)")]
    public Animator animator;  // 열리는 애니메이션 (선택)
    public GameObject closedVisual;  // 닫힌 상태 비주얼
    public GameObject openedVisual;  // 열린 상태 비주얼

    private ChestUI chestUI;  // 상자 UI 참조
    private List<ItemDrop> cachedItems;  // 한 번 생성된 아이템 캐싱

    private void Start()
    {
        UpdateVisuals();

        // ChestUI 찾기 (비활성화된 오브젝트도 포함)
        chestUI = FindObjectOfType<ChestUI>(true);
        if (chestUI == null)
        {
            Debug.LogWarning("ChestUI를 찾을 수 없습니다! 상자 UI가 작동하지 않습니다.");
        }
    }

    #region IInteractable Implementation

    public string GetInteractText()
    {
        // UI가 이미 열려있으면 닫기 안내
        if (chestUI != null && chestUI.IsChestOpen() && chestUI.GetCurrentLootBox() == this)
        {
            return $"'F' 키를 눌러 {boxName} 닫기";
        }
        return $"'F' 키를 눌러 {boxName} 열기";
    }

    public void Interact(GameObject interactor)
    {
        if (lootTable == null)
        {
            Debug.LogError("LootTable이 할당되지 않았습니다!");
            return;
        }

        if (chestUI == null)
        {
            Debug.LogError("ChestUI를 찾을 수 없습니다!");
            return;
        }

        // 상자 UI가 이미 열려있으면 닫기
        if (chestUI.IsChestOpen() && chestUI.GetCurrentLootBox() == this)
        {
            chestUI.CloseChest();
            return;
        }

        // 상자 UI 열기 (아이템은 GetLootItems()에서 캐싱 처리)
        chestUI.OpenChest(this);

        // 애니메이션 재생 (있다면, 처음 열 때만)
        if (animator != null && cachedItems == null)
        {
            animator.SetTrigger("Open");
        }
    }

    #endregion

    /// <summary>
    /// 상자의 아이템 목록을 가져옵니다 (한 번만 생성, 이후 캐싱)
    /// </summary>
    public List<ItemDrop> GetLootItems()
    {
        // 이미 생성된 아이템이 있으면 캐시 반환
        if (cachedItems != null)
        {
            Debug.Log($"[LootBox] 캐싱된 아이템 {cachedItems.Count}개 반환");
            return cachedItems;
        }

        // 처음 열 때만 랜덤 아이템 생성
        if (lootTable != null)
        {
            cachedItems = lootTable.GenerateRandomItems();
            Debug.Log($"[LootBox] 새로운 아이템 {cachedItems.Count}개 생성");
            return cachedItems;
        }

        Debug.LogError("[LootBox] LootTable이 없습니다!");
        cachedItems = new List<ItemDrop>();
        return cachedItems;
    }

    /// <summary>
    /// 특정 인덱스의 아이템을 null로 만듭니다 (인덱스 유지)
    /// </summary>
    public void RemoveItemAt(int index)
    {
        if (cachedItems != null && index >= 0 && index < cachedItems.Count)
        {
            cachedItems[index] = null;
            Debug.Log($"[LootBox] 슬롯 {index} 아이템 제거됨");

            // 남은 아이템 개수 확인
            int remainingCount = 0;
            foreach (var item in cachedItems)
            {
                if (item != null) remainingCount++;
            }
            Debug.Log($"[LootBox] 남은 아이템 개수: {remainingCount}");
        }
    }

    /// <summary>
    /// 모든 아이템을 제거합니다
    /// </summary>
    public void ClearAllItems()
    {
        if (cachedItems != null)
        {
            cachedItems.Clear();
            Debug.Log("[LootBox] 모든 아이템 제거됨");
        }
    }

    /// <summary>
    /// 상자가 완전히 비었는지 확인
    /// </summary>
    public bool IsEmpty()
    {
        if (cachedItems == null || cachedItems.Count == 0)
            return true;

        // 모든 아이템이 null인지 확인
        foreach (var item in cachedItems)
        {
            if (item != null)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 상자를 비움 상태로 표시 (네트워크 동기화)
    /// </summary>
    public void MarkAsOpened()
    {
        if (PhotonNetwork.IsConnected && photonView != null)
        {
            photonView.RPC("MarkAsOpenedRPC", RpcTarget.AllBuffered);
        }
        else
        {
            isOpened = true;
            UpdateVisuals();
        }
    }

    [PunRPC]
    private void MarkAsOpenedRPC()
    {
        isOpened = true;
        UpdateVisuals();
    }

    /// <summary>
    /// 상자의 시각적 상태 업데이트
    /// </summary>
    private void UpdateVisuals()
    {
        if (closedVisual != null)
        {
            closedVisual.SetActive(!isOpened);
        }

        if (openedVisual != null)
        {
            openedVisual.SetActive(isOpened);
        }
    }

    /// <summary>
    /// Photon 직렬화 (네트워크 동기화)
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송
            stream.SendNext(isOpened);
        }
        else
        {
            // 데이터 수신
            isOpened = (bool)stream.ReceiveNext();
            UpdateVisuals();
        }
    }
}
