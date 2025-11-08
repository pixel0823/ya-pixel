using UnityEngine;
using Photon.Pun;
using YAPixel;
using YAPixel.World;
using System.Collections;

/// <summary>
/// 월드에 배치된 상호작용 가능한 오브젝트 (예: 나무, 돌)
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class WorldObject : BaseWorldEntity<Object, ObjectDatabase>
{
    [Tooltip("오브젝트의 현재 체력")]
    private int currentHealth;

    [Tooltip("상호작용 애니메이션 시간(데미지 주는 간격)")]
    public float interactAnimationTime = 1.0f;

    // BaseWorldEntity에서 상속받은 entityData를 Object 타입으로 쉽게 접근할 수 있도록 프로퍼티를 추가합니다.
    public Object objectData
    {
        get { return entityData; }
        set { entityData = value; }
    }

    protected override string DatabasePath => "Objects/GlobalObjectDatabase";

    public void Interact(GameObject interactor)
    {
        var playerPhotonView = interactor.GetComponent<PhotonView>();
        if (playerPhotonView != null && playerPhotonView.IsMine)
        {
            // 플레이어의 아이템 사용 컴포넌트를 가져옵니다.
            var playerItemUse = interactor.GetComponent<PlayerItemUse>();
            if (playerItemUse == null) return;

            Item currentItem = playerItemUse.GetSelectedItem();
            // PlayerItemUse에서 데미지 값을 가져옵니다.
            int damage = playerItemUse.GetToolDamage(currentItem, objectData.requiredToolType);

            // 도구 내구도 감소 로직 (필요 시 추가)
            if (currentItem != null && currentItem.isTool && objectData.requiredToolType != ToolType.None)
            {
                // currentItem.durability -= objectData.toolDurabilityCost;
            }

            Debug.Log($"[WorldObject] Interact by me (ViewID: {playerPhotonView.ViewID}). Sending damage request to MasterClient with damage: {damage}");
            // 마스터 클라이언트에게 이 오브젝트에 데미지를 입혀달라고 요청
            this.photonView.RPC("RequestDamageFromServer", RpcTarget.MasterClient, damage);
        }
    }

    #region Photon RPC
    /// <summary>
    /// [마스터 클라이언트에서만 실행됨] 오브젝트에 데미지를 입히고, 파괴되었는지 확인합니다.
    /// </summary>
    [PunRPC]
    void RequestDamageFromServer(int damage)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentHealth -= damage;
        Debug.Log($"[Master] Object '{objectData.objectName}' damaged. Current health: {currentHealth}/{objectData.maxHealth}");

        // 체력이 0 이하면 오브젝트를 파괴하고 아이템을 드랍합니다.
        if (currentHealth <= 0)
        {
            Debug.Log($"[Master] Object '{objectData.objectName}' destroyed. Dropping items.");
            DropItems();
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            // 체력 변경사항을 모든 클라이언트에게 동기화합니다.
            this.photonView.RPC("SyncHealth", RpcTarget.All, currentHealth);
        }
    }

    /// <summary>
    /// [모든 클라이언트에서 실행됨] 오브젝트의 체력을 동기화합니다.
    /// </summary>
    [PunRPC]
    void SyncHealth(int health)
    {
        this.currentHealth = health;
    }
    #endregion

    #region 초기화
    /// <summary>
    /// 오브젝트 데이터 기반으로 월드 오브젝트의 상태를 설정합니다.
    /// 이 메서드는 BaseWorldEntity의 OnPhotonInstantiate에서 호출됩니다.
    /// </summary>
    public override void Initialize(Object data, object[] instantiationData)
    {
        this.objectData = data;
        this.currentHealth = data.maxHealth;

        if (this.objectData != null)
        {
            gameObject.name = $"{this.objectData.objectName} (World)";
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Note: Object에 icon이 없으므로, 필요하다면 추가해야 합니다.
                spriteRenderer.sprite = this.objectData.icon;
            }
        }
        else
        {
            Debug.LogError("월드 오브젝트 초기화에 사용된 Object 데이터가 null입니다.");
            if (PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(gameObject);
        }
    }
    #endregion

    /// <summary>
    /// [마스터 클라이언트에서만 실행됨] 오브젝트가 파괴될 때 아이템을 드랍합니다.
    /// </summary>
    private void DropItems()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (objectData.itemToDrop == null) return;

        // 아이템 데이터베이스에서 드랍할 아이템의 인덱스를 찾습니다.
        // 참고: 아이템 드랍 로직은 WorldObject에만 있으므로 ItemDatabase를 직접 로드합니다.
        ItemDatabase itemDatabase = Resources.Load<ItemDatabase>("Items/GlobalItemDatabase");
        if (itemDatabase == null)
        {
            Debug.LogError("[Master] ItemDatabase not found. Cannot drop items.");
            return;
        }

        int itemIndex = itemDatabase.GetIndex(objectData.itemToDrop);
        if (itemIndex == -1)
        {
            Debug.LogError($"[Master] Item to drop '{objectData.itemToDrop.itemName}' not found in database.");
            return;
        }

        // 랜덤한 개수의 아이템을 드랍합니다.
        int dropAmount = Random.Range(objectData.minDropAmount, objectData.maxDropAmount + 1);
        if (dropAmount > 0)
        {
            Debug.Log($"[Master] Dropping {dropAmount} of '{objectData.itemToDrop.itemName}' at {transform.position}.");
            object[] instantiationData = { itemIndex, dropAmount };
            PhotonNetwork.Instantiate("WorldItem", new Vector3(transform.position.x, transform.position.y, -1f), Quaternion.identity, 0, instantiationData);
        }
    }
}
