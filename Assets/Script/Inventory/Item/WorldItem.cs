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

    #region IInteractable 구현
    public string GetInteractText()
    {
        // itemData가 할당되지 않았을 경우를 대비하여 널 체크를 합니다.
        string itemName = itemData != null ? itemData.itemName : "알 수 없는 아이템";
        return $"'F' 키를 눌러 {itemName} 줍기";
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

        // 인벤토리에 아이템을 추가합니다.
        if (inventory.Add(itemData) != -1)
        {
            // 아이템 추가에 성공하면, 네트워크를 통해 이 오브젝트를 파괴합니다.
            string itemName = itemData != null ? itemData.itemName : "알 수 없는 아이템";
            Debug.Log($"{interactor.name}이(가) {itemName} 아이템을 주웠습니다.");

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
        // PhotonNetwork.Instantiate의 instantiationData에서 아이템 인덱스를 읽어옵니다.
        object[] instantiationData = info.photonView.InstantiationData;
        int itemIndex = (int)instantiationData[0];

        // Resources 폴더에서 ItemDatabase를 로드합니다.
        ItemDatabase database = Resources.Load<ItemDatabase>("Items/GlobalItemDatabase");
        if (database != null)
        {
            // 인덱스를 사용하여 아이템 데이터를 가져와 설정합니다.
            itemData = database.GetItem(itemIndex);

            // 아이템 이름과 아이콘 업데이트 (선택적이지만, 씬에서 바로 확인하기 좋음)
            if (itemData != null)
            {
                gameObject.name = itemData.itemName + " (World)";
                var spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = itemData.icon;
                }
            }
        }
        else
        {
            Debug.LogError("GlobalItemDatabase를 Resources 폴더에서 찾을 수 없습니다.");
        }
    }
    #endregion
}
