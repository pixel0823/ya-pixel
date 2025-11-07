using UnityEngine;
using YAPixel;
using UnityEngine.U2D.Animation; // 2D Animation 패키지 사용

// 도구의 종류를 나타내는 열거형
public enum ToolType { None, Axe, Pickaxe, Shovel }

// 아이템 정보를 담는 ScriptableObject. Asset 메뉴에서 생성하여 사용합니다.
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IDatabaseItem
{
    public string Name => itemName;

    // 'name'은 Object에 이미 있으므로 'itemName'을 사용합니다.
    public string itemName = "New Item"; // 아이템 이름 (데이터 식별자)
    public string description = "Item Description"; // 아이템 설명
    public Sprite icon = null; // 아이템 아이콘

    public bool isTool = false; // 이 아이템이 도구인지 여부
    public ToolType toolType = ToolType.None; // 도구의 종류
    public int attackPower = 0; // 도구의 공격력

    [Tooltip("도구일 경우, 이 도구의 스프라이트 라이브러리 에셋을 할당하세요.")]
    public SpriteLibraryAsset toolSpriteLibrary; // 도구 전용 스프라이트 라이브러리

    [Tooltip("도구일 경우, SpriteResolver가 사용할 카테고리 이름을 지정하세요.")]
    public string toolCategory; // SpriteResolver 카테고리 이름

    public bool isStackable = true; // 아이템이 겹칠 수 있는지 여부
    public int maxStackSize = 99; // 최대 겹칠 수 있는 개수

    public int amount = 1; // 현재 아이템의 개수 (인벤토리 내에서 사용)

    public virtual Item GetCopy()
    {
        Item copy = CreateInstance<Item>();
        copy.itemName = itemName;
        copy.description = description;
        copy.icon = icon;
        copy.isTool = isTool;
        copy.toolType = toolType;
        copy.attackPower = attackPower;
        copy.toolSpriteLibrary = toolSpriteLibrary;
        copy.toolCategory = toolCategory;
        copy.isStackable = isStackable;
        copy.maxStackSize = maxStackSize;
        copy.amount = amount;
        return copy;
    }

    //스택의 개수를 설정하는 함수
    public void SetAmount(int newAmount)
    {
        amount = Mathf.Clamp(newAmount, 1, maxStackSize);
    }
}
