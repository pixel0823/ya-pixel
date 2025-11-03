using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 조합 슬롯 - 아이템을 드롭받고 표시
/// </summary>
public class CombSlot : MonoBehaviour, IPointerClickHandler
{
    private Image slotImage;
    private Sprite defaultSprite;
    private TextMeshProUGUI amountText;

    [HideInInspector] public Item item; // 현재 슬롯에 있는 아이템
    [HideInInspector] public bool isResultSlot; // 결과 슬롯인지 여부

    private CombManager combManager;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            defaultSprite = slotImage.sprite;
        }

        amountText = GetComponentInChildren<TextMeshProUGUI>();
        if (amountText != null)
        {
            amountText.raycastTarget = false;
        }

        combManager = GetComponentInParent<CombManager>();
    }

    public void UpdateSlotUI()
    {
        if (slotImage == null) return;

        if (item != null)
        {
            slotImage.sprite = item.icon;
            slotImage.color = Color.white;

            if (amountText != null)
            {
                if (item.amount > 1)
                {
                    amountText.text = item.amount.ToString();
                    amountText.gameObject.SetActive(true);
                }
                else
                {
                    amountText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            slotImage.sprite = defaultSprite;
            slotImage.color = Color.white;
            if (amountText != null)
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearSlot()
    {
        item = null;
        UpdateSlotUI();
    }

    public void SetItem(Item newItem)
    {
        item = newItem;
        UpdateSlotUI();
    }

    // 우클릭으로 슬롯 비우기
    public void OnPointerClick(PointerEventData eventData)
    {
        // 결과 슬롯은 클릭 불가
        if (isResultSlot) return;

        if (eventData.button == PointerEventData.InputButton.Right && item != null)
        {
            ClearSlot();
            if (combManager != null)
            {
                combManager.CheckRecipe();
            }
        }
    }
}
