using System.Collections.Generic;
using UnityEngine;

namespace YAPixel
{
    /// <summary>
    /// 데이터베이스 역할을 하는 ScriptableObject의 기본 클래스입니다.
    /// Get/Set과 같은 공통 기능을 제네릭으로 구현합니다.
    /// </summary>
    /// <typeparam name="T">IDatabaseItem을 구현하는 데이터 타입 (예: Item, Object)</typeparam>
    public abstract class BaseDatabase<T> : ScriptableObject where T : ScriptableObject, IDatabaseItem
    {
        [SerializeField]
        private List<T> allItems = new List<T>();

        /// <summary>
        /// 데이터베이스의 모든 항목 리스트를 가져옵니다.
        /// </summary>
        public List<T> AllItems => allItems;

        /// <summary>
        /// 인덱스를 사용하여 데이터베이스에서 항목을 찾습니다.
        /// </summary>
        /// <param name="index">찾을 항목의 인덱스</param>
        /// <returns>찾은 항목. 없으면 null을 반환합니다.</returns>
        public T GetItem(int index)
        {
            if (index >= 0 && index < allItems.Count)
            {
                return allItems[index];
            }
            Debug.LogError($"잘못된 인덱스입니다: {index}");
            return null;
        }

        /// <summary>
        /// 항목 객체를 사용하여 데이터베이스에서 인덱스를 찾습니다.
        /// </summary>
        /// <param name="item">찾을 항목</param>
        /// <returns>찾은 인덱스. 없으면 -1을 반환합니다.</returns>
        public int GetIndex(T item)
        {
            if (item == null) return -1;
            for (int i = 0; i < allItems.Count; i++)
            {
                // 각 항목의 고유 이름(Name 속성)을 비교합니다.
                if (allItems[i] != null && allItems[i].Name == item.Name)
                {
                    return i;
                }
            }

            Debug.LogWarning($"{item.Name} 항목이 데이터베이스에 없습니다.");
            return -1;
        }
    }
}
