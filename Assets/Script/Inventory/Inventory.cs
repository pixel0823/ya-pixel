using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // PhotonNetwork.Instantiate를 사용하기 위해 추가

// 각 플레이어의 로컬 인벤토리를 관리하는 스크립트.
public class Inventory : MonoBehaviour
{
    // 아이템이 변경되었을 때 호출될 델리게이트(대리자)
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public List<Item> items = new List<Item>(); // 플레이어의 개인 인벤토리 아이템 리스트
    public int space = 20; // 인벤토리 최대 공간

    // 아이템을 인벤토리에 추가하는 로컬 함수
    public bool Add(Item item)
    {
        if (items.Count >= space)
        {
            Debug.Log("Not enough room in inventory.");
            return false;
        }
        items.Add(item);

        // 아이템 변경 콜백 호출
        onItemChangedCallback?.Invoke();

        return true;
    }

    // 아이템을 인벤토리에서 제거하는 로컬 함수
    public void Remove(Item item)
    {
        items.Remove(item);

        // 아이템 변경 콜백 호출
        onItemChangedCallback?.Invoke();
    }

    // 아이템을 인벤토리에서 제거하고 월드에 생성(드랍)하는 함수
    public void DropItem(Item item)
    {
        if (items.Contains(item))
        {
            // Remove 함수에서 콜백이 호출되므로 여기서는 호출할 필요가 없습니다.
            Remove(item);

            // Photon을 사용하여 네트워크상의 모든 플레이어에게 아이템 오브젝트를 생성합니다.
            PhotonNetwork.Instantiate("Items/" + item.itemName,
                                      transform.position + transform.forward, // 플레이어의 약간 앞쪽에 생성
                                      Quaternion.identity);
        }
    }
}
