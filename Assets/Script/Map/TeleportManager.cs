using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;

/// <summary>
/// 플레이어를 특정 Biome 내의 랜덤한 위치로 순간이동시킵니다.
/// MapManager의 Biome 시스템과 연동하여 안전한 위치를 찾습니다.
/// </summary>
public class TeleportManager : MonoBehaviour
{
    [Header("맵 참조")]
    [Tooltip("MapManager 참조 (자동으로 찾습니다)")]
    public MapManager mapManager;

    [Tooltip("순간이동할 타겟 Tilemap")]
    public Tilemap targetTilemap;

    [Header("순간이동 설정")]
    [Tooltip("랜덤 위치 검색 최대 시도 횟수")]
    public int maxAttempts = 100;

    [Tooltip("Biome 중심에서 얼마나 떨어진 곳에 스폰할지 (0~1, 0.5 = 중심 근처)")]
    [Range(0f, 1f)]
    public float spawnCenterBias = 0.3f;

    void Start()
    {
        // MapManager 자동 찾기 (비활성화된 오브젝트도 포함)
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>(true);
            if (mapManager == null)
            {
                Debug.LogError("[TeleportManager] MapManager를 찾을 수 없습니다.");
            }
        }

        // Tilemap 자동 찾기
        if (targetTilemap == null && mapManager != null)
        {
            targetTilemap = mapManager.targetTilemap;
        }

        // Biome 정보 출력
        PrintBiomeInfo();
    }

    /// <summary>
    /// 디버그용: 모든 Biome 정보를 출력합니다.
    /// </summary>
    private void PrintBiomeInfo()
    {
        if (mapManager == null || mapManager.biomes == null)
        {
            Debug.LogWarning("[TeleportManager] MapManager 또는 Biome 정보가 없습니다.");
            return;
        }

        Debug.Log($"[TeleportManager] ========== Biome 정보 ==========");
        Debug.Log($"[TeleportManager] 전체 맵 크기: {mapManager.mapWidth} x {mapManager.mapHeight}");
        Debug.Log($"[TeleportManager] Biome 개수: {mapManager.biomes.Count}");

        for (int i = 0; i < mapManager.biomes.Count; i++)
        {
            BiomeBounds bounds = CalculateBiomeBounds(i);
            Debug.Log($"[TeleportManager] Biome [{i}] \"{mapManager.biomes[i].name}\" - 범위: ({bounds.minX}, {bounds.minY}) ~ ({bounds.maxX}, {bounds.maxY})");
        }

        Debug.Log($"[TeleportManager] ===================================");
    }

    /// <summary>
    /// 플레이어를 선택한 Biome 내의 랜덤한 위치로 순간이동시킵니다.
    /// </summary>
    /// <param name="player">순간이동할 플레이어</param>
    /// <param name="biomeIndex">Biome 인덱스 (MapManager.biomes 리스트의 인덱스)</param>
    public void TeleportToRandomBiomePosition(GameObject player, int biomeIndex)
    {
        if (mapManager == null || targetTilemap == null)
        {
            Debug.LogError("[TeleportManager] MapManager 또는 Tilemap이 설정되지 않았습니다.");
            return;
        }

        if (biomeIndex < 0 || biomeIndex >= mapManager.biomes.Count)
        {
            Debug.LogError($"[TeleportManager] 잘못된 Biome 인덱스: {biomeIndex}");
            return;
        }

        string biomeName = mapManager.biomes[biomeIndex].name;
        Debug.Log($"[TeleportManager] 🚀 순간이동 시작 - Biome [{biomeIndex}] \"{biomeName}\"");

        // Biome 영역 계산
        BiomeBounds bounds = CalculateBiomeBounds(biomeIndex);
        Debug.Log($"[TeleportManager] 계산된 Biome 범위: ({bounds.minX}, {bounds.minY}) ~ ({bounds.maxX}, {bounds.maxY})");

        // Biome 내에서 타일이 있는 랜덤 위치 찾기
        Vector3 randomPosition = FindRandomTilePositionInBiome(bounds);

        if (randomPosition != Vector3.zero)
        {
            // Photon 네트워크 플레이어인 경우
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                player.transform.position = randomPosition;
                Debug.Log($"[TeleportManager] ✅ {player.name}이(가) \"{biomeName}\"로 순간이동했습니다. 위치: {randomPosition}");
            }
            else if (pv == null)
            {
                // Photon이 없는 경우 (싱글플레이)
                player.transform.position = randomPosition;
                Debug.Log($"[TeleportManager] ✅ {player.name}이(가) \"{biomeName}\"로 순간이동했습니다. 위치: {randomPosition}");
            }
        }
        else
        {
            Debug.LogError($"[TeleportManager] ❌ \"{biomeName}\"에서 적절한 순간이동 위치를 찾지 못했습니다.");
        }
    }

    /// <summary>
    /// 플레이어를 선택한 Biome 이름으로 해당 Biome 내의 랜덤한 위치로 순간이동시킵니다.
    /// </summary>
    /// <param name="player">순간이동할 플레이어</param>
    /// <param name="biomeName">목표 Biome의 이름</param>
    public void TeleportToBiomeByName(GameObject player, string biomeName)
    {
        if (mapManager == null || mapManager.biomes == null)
        {
            Debug.LogError("[TeleportManager] MapManager 또는 Biome 목록이 없습니다.");
            return;
        }

        // 이름으로 Biome 인덱스 찾기
        int biomeIndex = mapManager.biomes.FindIndex(b => b.name == biomeName);

        if (biomeIndex != -1)
        {
            // 찾은 인덱스로 기존 순간이동 함수 호출
            TeleportToRandomBiomePosition(player, biomeIndex);
        }
        else
        {
            Debug.LogError($"[TeleportManager] '{biomeName}'이라는 이름을 가진 Biome을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// Biome의 월드 좌표 범위를 계산합니다.
    /// MapManager의 GetBiomeForGridPosition 로직을 참고합니다.
    /// </summary>
    private BiomeBounds CalculateBiomeBounds(int biomeIndex)
    {
        int biomeCount = mapManager.biomes.Count;

        // 그리드 차원 계산 (MapManager와 동일한 로직)
        int gridCols = Mathf.CeilToInt(Mathf.Sqrt(biomeCount));
        int gridRows = Mathf.CeilToInt((float)biomeCount / gridCols);

        // 각 그리드 셀의 크기
        float cellWidth = (float)mapManager.mapWidth / gridCols;
        float cellHeight = (float)mapManager.mapHeight / gridRows;

        // biomeIndex를 row, col로 변환
        int row = biomeIndex / gridCols;
        int col = biomeIndex % gridCols;

        // Biome의 월드 좌표 범위 계산
        float minX = col * cellWidth - (mapManager.mapWidth / 2f);
        float maxX = (col + 1) * cellWidth - (mapManager.mapWidth / 2f);
        float minY = row * cellHeight - (mapManager.mapHeight / 2f);
        float maxY = (row + 1) * cellHeight - (mapManager.mapHeight / 2f);

        return new BiomeBounds
        {
            minX = Mathf.RoundToInt(minX),
            maxX = Mathf.RoundToInt(maxX),
            minY = Mathf.RoundToInt(minY),
            maxY = Mathf.RoundToInt(maxY)
        };
    }

    /// <summary>
    /// Biome 범위 내에서 타일이 있는 랜덤 위치를 찾습니다.
    /// </summary>
    private Vector3 FindRandomTilePositionInBiome(BiomeBounds bounds)
    {
        // Biome 중심 계산
        int centerX = (bounds.minX + bounds.maxX) / 2;
        int centerY = (bounds.minY + bounds.maxY) / 2;

        // 검색 범위 계산 (중심 편향 적용)
        int rangeX = Mathf.RoundToInt((bounds.maxX - bounds.minX) * (1f - spawnCenterBias) / 2f);
        int rangeY = Mathf.RoundToInt((bounds.maxY - bounds.minY) * (1f - spawnCenterBias) / 2f);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // 중심 근처에서 랜덤 좌표 생성
            int randomX = centerX + Random.Range(-rangeX, rangeX);
            int randomY = centerY + Random.Range(-rangeY, rangeY);

            // 범위를 벗어나지 않도록 클램프
            randomX = Mathf.Clamp(randomX, bounds.minX, bounds.maxX - 1);
            randomY = Mathf.Clamp(randomY, bounds.minY, bounds.maxY - 1);

            Vector3Int tilePosition = new Vector3Int(randomX, randomY, 0);

            // 해당 위치에 타일이 있는지 확인
            if (targetTilemap.HasTile(tilePosition))
            {
                // 타일의 월드 중심 좌표 반환
                return targetTilemap.GetCellCenterWorld(tilePosition);
            }
        }

        // 적절한 위치를 못 찾은 경우 Biome 중심 좌표 반환
        Debug.LogWarning($"[TeleportManager] {maxAttempts}번 시도 후 적절한 위치를 못 찾았습니다. Biome 중심으로 이동합니다.");
        Vector3Int fallbackPosition = new Vector3Int(centerX, centerY, 0);

        if (targetTilemap.HasTile(fallbackPosition))
        {
            return targetTilemap.GetCellCenterWorld(fallbackPosition);
        }

        return Vector3.zero; // 실패
    }

    /// <summary>
    /// Biome 범위를 저장하는 구조체
    /// </summary>
    private struct BiomeBounds
    {
        public int minX;
        public int maxX;
        public int minY;
        public int maxY;
    }

    // 디버그용: Biome 영역을 시각화 (Scene 뷰에서만 보임)
    void OnDrawGizmos()
    {
        if (mapManager == null || mapManager.biomes == null) return;

        for (int i = 0; i < mapManager.biomes.Count; i++)
        {
            BiomeBounds bounds = CalculateBiomeBounds(i);

            Vector3 center = new Vector3(
                (bounds.minX + bounds.maxX) / 2f,
                (bounds.minY + bounds.maxY) / 2f,
                0f
            );

            Vector3 size = new Vector3(
                bounds.maxX - bounds.minX,
                bounds.maxY - bounds.minY,
                0f
            );

            Gizmos.color = new Color(Random.value, Random.value, Random.value, 0.3f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}