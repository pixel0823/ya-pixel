using UnityEngine;

// 아이템 정보를 담는 MonoBehaviour. 프리펩에 부착하여 사용합니다.
public class Item : MonoBehaviour
{
    // 'name'은 MonoBehaviour에 이미 있으므로 'itemName'을 사용합니다.
    public string itemName = "New Item"; // 아이템 이름 (네트워크 식별자)
    public string description = "Item Description"; // 아이템 설명
    public Sprite icon = null; // 아이템 아이콘
}
