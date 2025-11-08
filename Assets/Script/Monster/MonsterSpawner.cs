using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject monsterPrefab; // 몬스터 프리팹

    [SerializeField]
    private int numberOfMonsters = 5; // 생성할 몬스터 수

    private BoxCollider2D spawnArea;

    void Start()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트의 BoxCollider2D 컴포넌트를 가져옵니다.
        spawnArea = GetComponent<BoxCollider2D>();
        if (spawnArea == null)
        {
            Debug.LogError("스폰 지역을 정의하기 위한 BoxCollider2D 컴포넌트가 필요합니다.");
            return;
        }
    }

    public void SpawnMonsters()
    {
        if (spawnArea == null)
        {
            Debug.LogError("BoxCollider2D가 설정되지 않아 스폰할 수 없습니다.");
            return;
        }
        
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < numberOfMonsters; i++)
        {
            // 스폰 지역 내에서 랜덤한 위치를 계산합니다.
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 spawnPosition = new Vector2(randomX, randomY);

            // 몬스터를 생성합니다.
            Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
