using UnityEngine;
using Photon.Pun;

/// <summary>
/// 필드에 떨어져 있는 아이템을 나타냅니다. IInteractable을 구현하여 플레이어와 상호작용합니다.
/// </summary>
[RequireComponent(typeof(PhotonView))] // 네트워크 동기화를 위해 PhotonView가 필수
public class WorldItem : MonoBehaviour, IInteractable
{
    [Tooltip("이 아이템의 이름. UI나 인벤토리에서 사용됩니다.")]
    public string itemName = "기본 아이템";

    public string GetInteractText()
    {
        return $"'E' 키를 눌러 {itemName} 줍기";
    }

    public void Interact(GameObject interactor)
    {
        // 여기에 interactor(플레이어)의 인벤토리에 아이템을 추가하는 로직을 넣을 수 있습니다.
        // 예: interactor.GetComponent<Inventory>().AddItem(itemName);
        Debug.Log($"{interactor.name}이(가) {itemName} 아이템을 주웠습니다.");

        // 네트워크 환경에 따라 오브젝트를 파괴합니다.
        if (PhotonNetwork.InRoom)
        {
            // 방에 접속해 있다면, 네트워크상의 모든 클라이언트에게서 이 아이템을 파괴합니다.
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // 오프라인 테스트 중이라면, 로컬에서만 파괴합니다.
            Destroy(gameObject);
        }
    }
}
