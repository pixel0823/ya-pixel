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

    [Header("UI (드래그 기능에 필요)")]
    [Tooltip("UI의 최상위 Canvas Transform. 드래그 아이콘을 표시할 때 사용됩니다.")]
    public Transform rootCanvas;

    [Header("아이템 드랍 설정")]
    [Tooltip("플레이어로부터 아이템이 드랍될 거리")]
    public float dropOffset = 1.0f;

    // 인벤토리 아이템 리스트.
    public List<Item> items;

    // --- 드래그 앤 드롭 상태 관리 변수 ---
    private GameObject dragIcon;      // 드래그 시 따라다니는 아이콘
    private InventorySlot originalSlot; // 드래그를 시작한 슬롯
    private bool dropSuccessful;      // 드롭 성공 여부

    void Awake()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase가 Inventory 컴포넌트에 할당되지 않았습니다!", this);
        }
        if (rootCanvas == null)
        {
            rootCanvas = FindObjectOfType<Canvas>()?.transform;
            if (rootCanvas == null)
                Debug.LogError("Root Canvas가 Inventory 컴포넌트에 할당되지 않았습니다! 드래그 기능이 작동하지 않습니다.", this);
        }

        items = new List<Item>(space);
        for (int i = 0; i < space; i++)
        {
            items.Add(null);
        }
    }

    // 아이템을 인벤토리의 첫 번째 빈 슬롯에 추가하고, 성공 시 해당 인덱스를, 실패 시 -1을 반환합니다.
    public int Add(Item item)
    {
        int emptyIndex = items.IndexOf(null);

        if (emptyIndex != -1)
        {
            items[emptyIndex] = item.GetCopy();
            onItemChangedCallback?.Invoke();
            return emptyIndex;
        }
        else
        {
            Debug.Log("인벤토리에 공간이 부족합니다.");
            return -1;
        }
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

    // 아이템을 인벤토리에서 제거하고 월드에 생성(드랍)합니다. (인덱스 기반으로 변경)
    public void DropItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= space || items[slotIndex] == null) return;

        Item itemToDrop = items[slotIndex];

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase가 할당되지 않아 아이템을 드랍할 수 없습니다.");
            return;
        }

        int itemIndexInDB = itemDatabase.GetIndex(itemToDrop);
        if (itemIndexInDB == -1)
        {
            Debug.LogError($"'{itemToDrop.itemName}'을(를) 데이터베이스에서 찾을 수 없어 네트워크 드랍에 실패했습니다.");
            return;
        }

        Remove(slotIndex);

        // --- 드랍 위치 계산 로직 ---
        Vector3 spawnPosition = transform.position;
        if (itemToDrop.icon != null)
        {
            // 아이템 스프라이트의 절반 높이만큼 y 위치를 올려서, 아이템의 바닥이 플레이어 발에 오도록 합니다.
            float verticalOffset = itemToDrop.icon.bounds.extents.y;
            spawnPosition += new Vector3(0, verticalOffset, 0);
        }
        // --- 드랍 위치 계산 로직 끝 ---

        object[] instantiationData = new object[] { itemIndexInDB };

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate("WorldItem", spawnPosition, Quaternion.identity, 0, instantiationData);
        }
        else
        {
            GameObject worldItemPrefab = Resources.Load<GameObject>("WorldItem");
            if (worldItemPrefab != null)
            {
                GameObject worldItemObject = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);
                WorldItem worldItem = worldItemObject.GetComponent<WorldItem>();
                if (worldItem != null)
                {
                    worldItem.itemData = itemToDrop;
                    worldItemObject.name = itemToDrop.itemName + " (World)";
                    var spriteRenderer = worldItemObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null) spriteRenderer.sprite = itemToDrop.icon;
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
            Item temp = items[index1];
            items[index1] = items[index2];
            items[index2] = temp;

            onItemChangedCallback?.Invoke();
        }
    }

    // --- 드래그 앤 드롭 로직 (InventorySlot에서 호출) ---

    public bool IsDragging()
    {
        return originalSlot != null;
    }

    public void OnBeginDrag(InventorySlot slot)
    {
        if (slot.item == null || rootCanvas == null) return;

        originalSlot = slot;
        dropSuccessful = false;

        dragIcon = new GameObject("Drag Icon");
        var rt = dragIcon.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        var img = dragIcon.AddComponent<Image>();
        img.sprite = originalSlot.item.icon;
        img.raycastTarget = false;

        dragIcon.transform.SetParent(rootCanvas);
        dragIcon.transform.SetAsLastSibling();

        originalSlot.SetDragState(true); // 드래그 시작 슬롯을 투명하게 만듭니다.
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }
    }

    public void OnDrop(InventorySlot dropSlot)
    {
        if (originalSlot == null || originalSlot == dropSlot) return;

        SwapItems(originalSlot.slotIndex, dropSlot.slotIndex);
        dropSuccessful = true;
    }

    public void OnEndDrag()
    {
        if (originalSlot == null) return;

        if (!dropSuccessful)
        {
            DropItem(originalSlot.slotIndex);
        }
        else
        {
            // 성공적으로 스왑되었으므로, 원래 아이콘을 다시 켤 필요가 없습니다.
            // onItemChangedCallback에 의해 UI가 업데이트되기 때문입니다.
        }

        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
        dragIcon = null;

        // 드래그가 실패했거나 취소되었을 때만 원래 슬롯의 모습을 복구합니다.
        if (!dropSuccessful)
        {
            originalSlot.SetDragState(false);
        }

        originalSlot = null;
    }
}