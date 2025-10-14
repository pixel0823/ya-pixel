using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for Image component
using UnityEngine.EventSystems; // Required for PointerEventData
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

    // 인벤토리 아이템 리스트.
    public List<Item> items;

    void Awake()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase가 Inventory 컴포넌트에 할당되지 않았습니다!", this);
        }

        items = new List<Item>(space);
        for (int i = 0; i < space; i++)
        {
            items.Add(null);
        }
    }


    // 아이템 추가 로직 (스태킹, 슬롯 분할 등 전체 기능 수정)
    // itemTemplate: 원본 아이템 정보, amount: 추가할 개수
    // 성공 시 true, 인벤토리가 가득 차 실패 시 false 반환
    public bool Add(Item itemTemplate, int amount)
    {
        if (itemTemplate == null || amount <= 0) return false;

        int amountToAdd = amount;

        // 1. 스택 가능한 아이템의 경우, 기존 스택에 합치기 시도
        if (itemTemplate.isStackable)
        {
            for (int i = 0; i < space; i++)
            {
                if (items[i] != null && items[i].itemName == itemTemplate.itemName && items[i].amount < items[i].maxStackSize)
                {
                    int spaceLeftInStack = items[i].maxStackSize - items[i].amount;
                    int amountToMove = Mathf.Min(amountToAdd, spaceLeftInStack);

                    items[i].amount += amountToMove;
                    amountToAdd -= amountToMove;

                    if (amountToAdd <= 0)
                    {
                        onItemChangedCallback?.Invoke();
                        return true; // 모든 아이템 추가 완료
                    }
                }
            }
        }

        // 2. 남은 아이템을 새 슬롯에 추가 (스택 불가능 아이템은 바로 여기로)
        while (amountToAdd > 0)
        {
            int emptySlotIndex = -1;
            for (int i = 0; i < space; i++)
            {
                if (items[i] == null)
                {
                    emptySlotIndex = i;
                    break;
                }
            }

            if (emptySlotIndex != -1)
            {
                items[emptySlotIndex] = itemTemplate.GetCopy();
                int amountForNewStack = Mathf.Min(amountToAdd, itemTemplate.maxStackSize);
                items[emptySlotIndex].amount = amountForNewStack;
                amountToAdd -= amountForNewStack;
            }
            else
            {
                // 빈 슬롯이 더 이상 없음
                Debug.Log("인벤토리가 가득 찼습니다. 남은 아이템 " + amountToAdd + "개는 추가할 수 없습니다.");
                onItemChangedCallback?.Invoke();
                return false; // 추가 실패
            }
        }

        onItemChangedCallback?.Invoke();
        return true; // 모든 아이템 추가 성공
    }

    // 호환성을 위한 오버로드. Item 객체에 담긴 amount만큼 추가 시도
    public bool Add(Item item)
    {
        if (item == null) return false;
        return Add(item, item.amount);
    }

    // 인벤토리에서 특정 인덱스의 아이템을 제거(null로 설정)합니다.
    public void Remove(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < space)
        {
            items[slotIndex] = null;
            onItemChangedCallback?.Invoke();
        }
    }

    // 아이템 드랍 (단일/전체 드랍 기능 구현)
    public void DropItem(int slotIndex, bool dropAll = false)
    {
        if (slotIndex < 0 || slotIndex >= space || items[slotIndex] == null) return;

        Item itemInSlot = items[slotIndex];
        Item itemToDrop = itemInSlot.GetCopy(); // 복사본 생성

        // 드랍할 수량 결정 및 인벤토리 잔여 수량 조절
        if (!itemInSlot.isStackable || dropAll)
        {
            itemToDrop.amount = itemInSlot.amount; // 전체 수량 드랍
            Remove(slotIndex); // 인벤토리에서 아이템 완전히 제거
        }
        else // 1개만 드랍
        {
            itemToDrop.amount = 1;
            itemInSlot.amount--;
            if (itemInSlot.amount <= 0)
            {
                Remove(slotIndex); // 수량이 0이 되면 제거
            }
        }

        onItemChangedCallback?.Invoke();

        // --- 네트워크 드랍 처리 ---
        int itemIndexInDB = itemDatabase.GetIndex(itemToDrop);
        if (itemIndexInDB == -1)
        {
            Debug.LogError($"'{itemToDrop.itemName}'을(를) 데이터베이스에서 찾을 수 없어 네트워크 드랍에 실패했습니다.");
            return;
        }

        // 아이템을 플레이어의 현재 위치(발밑)에 바로 생성합니다.
        Vector3 spawnPosition = transform.position;

        // 월드 아이템 생성 시, 아이템 DB 인덱스와 '개수'를 함께 전달
        object[] instantiationData = new object[] { itemIndexInDB, itemToDrop.amount };

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate("WorldItem", spawnPosition, Quaternion.identity, 0, instantiationData);
        }
        else
        {
            // 오프라인 상태일 경우, 로컬에서만 아이템을 생성합니다.
            GameObject worldItemPrefab = Resources.Load<GameObject>("WorldItem");
            if (worldItemPrefab != null)
            {
                GameObject worldItemObject = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);
                WorldItem worldItem = worldItemObject.GetComponent<WorldItem>();
                if (worldItem != null)
                {
                    // WorldItem.cs에 추가된 공용 초기화 메서드를 호출하여 아이템 정보를 설정합니다.
                    worldItem.Initialize(itemDatabase.GetItem(itemIndexInDB), itemToDrop.amount);
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
        if (index1 >= 0 && index1 < space && index2 >= 0 && index2 < space)
        {
            // 같은 아이템이고 스택 가능하면 합치기
            if (items[index1] != null && items[index2] != null && items[index1].itemName == items[index2].itemName && items[index1].isStackable)
            {
                int spaceLeftInStack = items[index2].maxStackSize - items[index2].amount;
                int amountToMove = Mathf.Min(items[index1].amount, spaceLeftInStack);

                items[index2].amount += amountToMove;
                items[index1].amount -= amountToMove;

                if (items[index1].amount <= 0)
                {
                    items[index1] = null;
                }
                onItemChangedCallback?.Invoke();
            }
            else // 다른 아이템이면 그냥 스왑
            {
                Item temp = items[index1];
                items[index1] = items[index2];
                items[index2] = temp;
                onItemChangedCallback?.Invoke();
            }
        }
    }


}