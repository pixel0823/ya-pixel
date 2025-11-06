using UnityEngine;
using YAPixel;

// 오브젝트 정보를 담는 ScriptableObject. Asset 메뉴에서 생성하여 사용합니다.
[CreateAssetMenu(fileName = "New Object", menuName = "World/Object")]
public class Object : ScriptableObject, IDatabaseItem
{
    public string Name => objectName;

    [Header("오브젝트 기본 정보")]
    // 'name'은 Object에 이미 있으므로 'objectName'을 사용합니다.
    public string objectName = "New Object"; // 오브젝트 이름 (데이터 식별자)
    public string description = "Object Description"; // 오브젝트 설명
    public Sprite icon = null; // 오브젝트 아이콘
    public int maxHealth = 100; // 오브젝트의 체력


    [Header("오브젝트 파괴시 드랍 아이템")]
    public Item itemToDrop; // 오브젝트가 파괴될 때 드랍할 아이템
    public int minDropAmount = 1; // 최소 드랍 개수
    public int maxDropAmount = 5; // 최대 드랍 개수

    [Header("필요 도구")]
    public ToolType requiredToolType; // 오브젝트를 채집하는 데 필요한 도구 종류
    public int toolDurabilityCost = 1; // 도구 사용 시 감소하는 내구도

    public Object GetCopy()
    {
        Object copy = CreateInstance<Object>();
        copy.objectName = objectName;
        copy.description = description;
        copy.icon = icon;
        copy.maxHealth = maxHealth;
        copy.itemToDrop = itemToDrop;
        copy.minDropAmount = minDropAmount;
        copy.maxDropAmount = maxDropAmount;
        copy.requiredToolType = requiredToolType;
        copy.toolDurabilityCost = toolDurabilityCost;
        return copy;
    }
}