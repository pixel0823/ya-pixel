using UnityEngine;
using Photon.Pun;

/// <summary>
/// 필드에 떨어져 있는 아이템을 나타냅니다. IInteractable을 구현하여 플레이어와 상호작용합니다.
/// [수정됨] 아이템 줍기 로직을 마스터 클라이언트 권한으로 변경하여 소유권 문제를 해결합니다.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class WorldItem : MonoBehaviourPun, IInteractable, IPunInstantiateMagicCallback
{
    [Tooltip("이 아이템의 데이터. Item 컴포넌트를 참조합니다.")]
    public Item itemData;
    [Tooltip("아이템 개수")]
    public int count = 1;

    private ItemDatabase itemDatabase; // 아이템 DB 참조

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
        // itemDatabase는 OnPhotonInstantiate에서 로드되어 있어야 합니다.
        if (itemDatabase == null)
        {
            Debug.LogError("[Master] ItemDatabase not loaded. Cannot give item.");
            return;
        }

        int itemID = itemDatabase.GetIndex(this.itemData);
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

        #region Photon 콜백 및 초기화
        /// <summary>
        /// PhotonNetwork.Instantiate를 통해 생성될 때 호출됩니다.
        /// 아이템 데이터와 개수를 네트워크로부터 받아 초기화합니다.
        /// </summary>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // DB를 로드합니다.
        itemDatabase = Resources.Load<ItemDatabase>("Items/GlobalItemDatabase");
        if (itemDatabase == null)
        {
            Debug.LogError("'Items/GlobalItemDatabase' 경로에서 ItemDatabase를 찾을 수 없습니다.");
            if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
            return;
        }

        // 네트워크 생성 시 받은 데이터(아이템 DB 인덱스, 개수)를 읽어옵니다.
        object[] instantiationData = info.photonView.InstantiationData;
        int itemIndex = (int)instantiationData[0];
        int amount = (int)instantiationData[1];

        Item data = itemDatabase.GetItem(itemIndex);
        Initialize(data, amount);
    }

    /// <summary>
    /// 아이템 데이터와 개수를 기반으로 월드 아이템의 외형과 정보를 설정합니다.
    /// </summary>
    public void Initialize(Item data, int amount)
    {
        this.itemData = data;
        this.count = amount;

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
