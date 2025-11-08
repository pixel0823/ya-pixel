using UnityEngine;
using YAPixel;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : BaseDatabase<Item>
{
    // 이제 모든 로직은 BaseDatabase에 있습니다.
}
