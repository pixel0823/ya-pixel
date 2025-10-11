using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 각 플레이어의 인벤토리를 관리하는 스크립트. UI와 데이터 연동의 핵심.
public class Inventory : MonoBehaviour
{
    [Header("아이템 데이터베이스")]
    [Tooltip("게임의 모든 아이템이 등록된 ItemDatabase 애셋.")]
    public ItemDatabase itemDatabase;

    // 아이템이 변경되었을 때 UI 업데이트를 위해 호출될 델리게이트
    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    [Header("인벤토리 설정")]
    [Tooltip("인벤토리 최대 공간")]
    public int space = 20;

    // 인벤토리 아이템 리스트. 고정 크기이며 빈 슬롯은 null로 표현됩니다.
    public List<Item> items;

    void Awake()
    {
        // 데이터베이스 할당 여부 확인
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase가 Inventory 컴포넌트에 할당되지 않았습니다!", this);
        }

        // 인벤토리 리스트를 고정 크기로 초기화하고 null로 채웁니다.
        items = new List<Item>(space);
        for (int i = 0; i < space; i++)
        {
            items.Add(null);
        }
    }

    // 아이템을 인벤토리의 첫 번째 빈 슬롯에 추가합니다.
    public bool Add(Item item)
    {
        // 첫 번째 빈 슬롯(null)을 찾습니다.
        int emptyIndex = items.IndexOf(null);

        if (emptyIndex != -1)
        {
            // 빈 슬롯에 아이템을 추가합니다.
            items[emptyIndex] = item;
            // UI 업데이트 콜백 호출
            onItemChangedCallback?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("인벤토리에 공간이 부족합니다.");
            return false;
        }
    }

    // 인벤토리에서 특정 아이템을 찾아 제거(null로 설정)합니다.
    public void Remove(Item item)
    {
        int index = items.IndexOf(item);
        if (index != -1)
        {
            // 해당 인덱스의 아이템을 null로 설정하여 슬롯을 비웁니다.
            items[index] = null;
            // UI 업데이트 콜백 호출
            onItemChangedCallback?.Invoke();
        }
    }

    // 아이템을 인벤토리에서 제거하고 월드에 생성(드랍)합니다.
    public void DropItem(Item item)
    {
        if (item == null || !items.Contains(item)) return;

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase가 할당되지 않아 아이템을 드랍할 수 없습니다.");
            return;
        }

        // 데이터베이스에서 아이템 인덱스 찾기 (네트워크용)
        int itemIndex = itemDatabase.GetIndex(item);
        if (itemIndex == -1)
        {
            Debug.LogError($"'{item.itemName}'을(를) 데이터베이스에서 찾을 수 없어 네트워크 드랍에 실패했습니다.");
            return;
        }

        // 인벤토리에서 아이템 제거
        Remove(item);

        // --- 아이템 드랍 로직 (온라인/오프라인) ---
        object[] instantiationData = new object[] { itemIndex };

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate("WorldItem", transform.position + transform.forward, Quaternion.identity, 0, instantiationData);
        }
        else
        {
            GameObject worldItemPrefab = Resources.Load<GameObject>("WorldItem");
            if (worldItemPrefab != null)
            {
                GameObject worldItemObject = Instantiate(worldItemPrefab, transform.position + transform.forward, Quaternion.identity);
                WorldItem worldItem = worldItemObject.GetComponent<WorldItem>();
                if (worldItem != null)
                {
                    worldItem.itemData = item;
                    worldItemObject.name = item.itemName + " (World)";
                    var spriteRenderer = worldItemObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null) spriteRenderer.sprite = item.icon;
                }
                else
                {
                    Debug.LogError("'WorldItem' 프리팹에 WorldItem 컴포넌트가 없습니다.");
                    Destroy(worldItemObject);
                }
            }
            else
            {
                Debug.LogError("'WorldItem' 프리팹을 Resources 폴더에서 찾을 수 없습니다.");
            }
        }
    }

    // 두 인덱스에 해당하는 슬롯의 아이템을 서로 바꿉니다.
    public void SwapItems(int index1, int index2)
    {
        // 인덱스가 유효한 범위 내에 있는지 확인합니다.
        if (index1 >= 0 && index1 < space && index2 >= 0 && index2 < space)
        {
            // 두 슬롯의 아이템을 교환합니다. (어느 한쪽 또는 양쪽 모두 null일 수 있음)
            Item temp = items[index1];
            items[index1] = items[index2];
            items[index2] = temp;

            // UI 업데이트 콜백 호출
            onItemChangedCallback?.Invoke();
        }
    }
}
