using UnityEngine;
using YAPixel;

[CreateAssetMenu(fileName = "ObjectDatabase", menuName = "World/Object Database")]
public class ObjectDatabase : BaseDatabase<Object>
{
    // 이제 모든 로직은 BaseDatabase에 있습니다.
}
