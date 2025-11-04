using UnityEngine;
using Photon.Pun;
using YAPixel;
using YAPixel.World;

/// <summary>
/// 필드에 떨어져 있는 아이템을 나타냅니다. IInteractable을 구현하여 플레이어와 상호작용합니다.
/// [수정됨] 아이템 줍기 로직을 마스터 클라이언트 권한으로 변경하여 소유권 문제를 해결합니다.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class WorldItem : BaseWorldEntity<Item, ItemDatabase>, IInteractable
{
    [Tooltip("아이템 개수")]
    public int count = 1;

    // BaseWorldEntity에서 상속받은 entityData를 Item 타입으로 쉽게 접근할 수 있도록 프로퍼티를 추가합니다.
    public Item itemData
    {
        get { return entityData; }
        set { entityData = value; }
    }

    protected override string DatabasePath => "Items/GlobalItemDatabase";

    #region IInteractable 구현
    public string GetInteractText()
    {
        string itemName = itemData != null ? itemData.itemName : "알 수 없는 아이템";
        return $"'F' 키를 눌러 {itemName} {(count > 1 ? $"{count}개" : "")} 줍기";
    }

    /// <summary>
    /// 플레이어가 상호작용 시, 직접 아이템을 줍는 대신 마스터 클라이언트에게 픽업 요청을 보냅니다.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        var playerPhotonView = interactor.GetComponent<PhotonView>();
        if (playerPhotonView != null && playerPhotonView.IsMine)
        {
            Debug.Log($"[WorldItem] Interact by me (ViewID: {playerPhotonView.ViewID}). Sending pickup request to MasterClient.");
            // 마스터 클라이언트에게 이 아이템을 주워달라고 요청하는 RPC를 보냅니다.
            // 매개변수: 아이템을 주울 플레이어의 ViewID
            this.photonView.RPC("RequestPickupFromServer", RpcTarget.MasterClient, playerPhotonView.ViewID);
        }
    }
    #endregion

    #region Photon RPC
    /// <summary>
    /// [마스터 클라이언트에서만 실행됨] 플레이어의 아이템 줍기 요청을 처리합니다.
    /// </summary>
    [PunRPC]
    void RequestPickupFromServer(int requestingPlayerViewID)
    {
        // 마스터 클라이언트가 아니면 아무것도 하지 않음
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"[Master] Received pickup request for player ViewID: {requestingPlayerViewID}");

        // 요청을 보낸 플레이어를 찾습니다.
        PhotonView requestingPlayerView = PhotonView.Find(requestingPlayerViewID);
        if (requestingPlayerView == null)
        {
            Debug.LogError($"[Master] Pickup failed: Player with ViewID {requestingPlayerViewID} not found.");
            return;
        }

        // 아이템을 인벤토리에 추가하라고 해당 클라이언트에게 RPC로 명령합니다.
        // 이제 database 필드는 BaseWorldEntity에서 초기화됩니다.
        if (database == null)
        {
            Debug.LogError("[Master] ItemDatabase not loaded. Cannot give item.");
            return;
        }

        int itemID = database.GetIndex(this.itemData);
        if (itemID != -1)
        {
            Debug.Log($"[Master] Found item '{this.itemData.itemName}' (ID: {itemID}). Sending AddItemRPC to player {requestingPlayerView.Owner.UserId}.");
            // 1. 해당 플레이어에게 아이템을 추가하라는 명령을 보냅니다.
            requestingPlayerView.RPC("AddItemRPC", requestingPlayerView.Owner, itemID, this.count);

            // 2. 아이템 추가 명령을 보낸 후, 월드에 있는 아이템을 모든 플레이어에게서 파괴합니다.
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            Debug.LogError($"[Master] Pickup failed: Item {this.itemData.itemName} not found in DB.");
        }
    }
    #endregion

    #region 초기화
    /// <summary>
    /// 아이템 데이터와 개수를 기반으로 월드 아이템의 외형과 정보를 설정합니다.
    /// 이 메서드는 BaseWorldEntity의 OnPhotonInstantiate에서 호출됩니다.
    /// </summary>
    public override void Initialize(Item data, object[] instantiationData)
    {
        this.itemData = data;
        this.count = (int)instantiationData[1]; // instantiationData에서 개수(amount)를 가져옵니다.

        if (this.itemData != null)
        {
            gameObject.name = $"{this.itemData.itemName} ({this.count}) (World)";
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = this.itemData.icon;
            }
        }
        else
        {
            Debug.LogError("월드 아이템 초기화에 사용된 Item 데이터가 null입니다.");
            if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
        }
    }
    #endregion
}
