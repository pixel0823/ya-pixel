using UnityEngine;
using Photon.Pun;

/// <summary>
/// 필드에 떨어져 있는 아이템을 나타냅니다. IInteractable을 구현하여 플레이어와 상호작용합니다.
/// </summary>
[RequireComponent(typeof(PhotonView))] // 네트워크 동기화를 위해 PhotonView가 필수
public class WorldItem : MonoBehaviour, IInteractable, IPunInstantiateMagicCallback
{
    [Tooltip("이 아이템의 데이터. Item 컴포넌트를 참조합니다.")]
    public Item itemData;
    [Tooltip("아이템 개수")]
    public int count = 1;

    /// <summary>
    /// 아이템 데이터와 개수를 기반으로 월드 아이템을 초기화합니다.
    /// </summary>
    /// <param name="data">설정할 아이템의 데이터</param>
    /// <param name="amount">설정할 아이템의 개수</param>
    public void Initialize(Item data, int amount)
    {
        this.itemData = data;
        this.count = amount;

        if (this.itemData != null)
        {
            gameObject.name = this.itemData.itemName + " (World)";
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = this.itemData.icon;
            }
        }
        else
        {
            Debug.LogError("월드 아이템 초기화에 사용된 Item 데이터가 null입니다.");
            // 데이터가 없으면 아이템을 즉시 파괴하여 오류 상태로 남지 않도록 함
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    #region IInteractable 구현
    public string GetInteractText()
    {
        // itemData가 할당되지 않았을 경우를 대비하여 널 체크를 합니다.
        string itemName = itemData != null ? itemData.itemName : "알 수 없는 아이템";
        return $"'F' 키를 눌러 {itemName} {(count > 1 ? $"{count}개" : "")} 줍기";
    }

    public void Interact(GameObject interactor)
    {
        // interactor(플레이어)에서 Inventory 컴포넌트를 가져옵니다.
        var inventory = interactor.GetComponent<Inventory>();
        if (inventory == null)
        {
            // 인벤토리가 없는 경우, 상호작용을 중단합니다.
            Debug.LogError("상호작용한 오브젝트에 인벤토리가 없습니다.");
            return;
        }

        // [수정됨] 인벤토리에 아이템을 추가하고, 성공 여부를 bool로 받습니다.
        if (inventory.Add(itemData, count))
        {
            // 아이템 추가에 성공하면, 네트워크를 통해 이 오브젝트를 파괴합니다.
            string itemName = itemData != null ? itemData.itemName : "알 수 없는 아이템";
            Debug.Log($"{interactor.name}이(가) {itemName} 아이템을 {count}개 주웠습니다.");

            if (PhotonNetwork.InRoom)
            {
                // 포톤 네트워크에 연결된 경우, 네트워크상의 모든 플레이어에게서 오브젝트를 파괴합니다.
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // 오프라인 상태인 경우, 로컬에서만 오브젝트를 파괴합니다.
                Destroy(gameObject);
            }
        }
        else
        {
            // 인벤토리가 가득 찼을 경우, 플레이어에게 알립니다.
            Debug.Log("인벤토리가 가득 찼습니다.");
            // 여기에 사용자에게 피드백을 주는 UI 로직을 추가할 수 있습니다. (예: "인벤토리가 꽉 찼습니다!" 메시지 표시)
        }
    }
    #endregion

    #region Photon 콜백
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // PhotonNetwork.Instantiate의 instantiationData에서 아이템 인덱스와 개수를 읽어옵니다.
        object[] instantiationData = info.photonView.InstantiationData;
        int itemIndex = (int)instantiationData[0];
        int amount = (int)instantiationData[1];

        // [중요] Resources 폴더 안의 경로가 정확해야 합니다.
        ItemDatabase database = Resources.Load<ItemDatabase>("Items/GlobalItemDatabase");
        if (database != null)
        {   
            Item data = database.GetItem(itemIndex);
            // 공용 초기화 메서드를 호출하여 아이템 정보를 설정합니다.
            Initialize(data, amount);
        }
        else
        {
            Debug.LogError("'Items/GlobalItemDatabase' 경로에서 ItemDatabase를 찾을 수 없습니다. Resources 폴더와 경로를 확인해주세요.");
        }
    }
    #endregion
}
