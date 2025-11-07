using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// [수정됨] 바이옴(Biome) 개념을 도입하여 맵을 생성합니다.
/// 2중 Perlin Noise를 사용하여 큰 바이옴을 먼저 결정하고, 각 바이옴 내에서 세부 지형을 생성합니다.
/// </summary>
public class MapManager : MonoBehaviour
{
    [Header("맵 생성 설정")]
    [Tooltip("전체 맵의 가로(X) 크기입니다.")]
    public int mapWidth = 1000;
    [Tooltip("전체 맵의 세로(Y) 크기입니다.")]
    public int mapHeight = 1000;

    [Header("타일맵 설정")]
    [Tooltip("맵을 그릴 대상 Tilemap입니다.")]
    public Tilemap targetTilemap;
    [Tooltip("중앙에 배치될 베이스맵 프리팹입니다. (Tilemap 형태)")]
    public Tilemap baseMapPrefab;
    [Tooltip("베이스맵의 위치 오프셋입니다.")]
    public Vector2Int baseMapOffset;

    [Header("바이옴 및 지형 설정")]
    [Tooltip("생성될 바이옴 리스트입니다. 생성 시 순서가 무작위로 섞입니다.")]
    public List<Biome> biomes;
    [Tooltip("바이옴 내 세부 지형을 위한 Perlin Noise 스케일 값입니다.")]
    public float terrainNoiseScale = 0.1f;
    [Space(10)]
    [Tooltip("바이옴 경계의 왜곡(울퉁불퉁함) 정도를 조절합니다.")]
    public float biomeWarpScale = 0.05f;
    [Tooltip("바이옴 경계의 왜곡 강도(최대 변위)를 조절합니다.")]
    public float biomeWarpIntensity = 20f;

    [Header("오브젝트 생성 설정")]
    [Tooltip("생성될 월드 오브젝트의 프리팹입니다. (WorldObject 스크rip트 포함)")]
    public GameObject worldObjectPrefab;

    private int seed;
    private System.Random pseudoRandom;
    private float warpOffsetX, warpOffsetY;
    private Transform objectContainer;
    private ObjectDatabase objectDatabase;

    // [신규] 맵 생성 시 순서가 섞인 바이옴 리스트
    private List<Biome> shuffledBiomes;

    [System.Serializable]
    public class TileInfo
    {
        public string name;
        public TileBase tileAsset;
        [Range(0f, 1f)]
        public float threshold;
    }

    [System.Serializable]
    public class Biome
    {
        public string name;
        [Tooltip("이 바이옴에서 사용될 타일 리스트입니다.")]
        public List<TileInfo> terrainTiles;
        [Tooltip("이 바이옴에서 생성될 오브젝트 리스트입니다.")]
        public List<ObjectInfo> spawnableObjects;
    }

    [System.Serializable]
    public class ObjectInfo
    {
        public Object objectData;
        [Range(0f, 1f)]
        [Tooltip("타일 하나당 생성될 확률입니다.")]
        public float density;
        [Tooltip("이 오브젝트가 생성될 수 있는 타일 리스트입니다. 비어있으면 바이옴 내 모든 타일에 생성 가능합니다.")]
        public List<TileBase> canSpawnOn;
    }


    void Start()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("[MapManager] 'Target Tilemap'이 할당되지 않았습니다.");
            return;
        }
        if (biomes == null || biomes.Count == 0)
        {
            Debug.LogError("[MapManager] 'Biomes' 리스트가 비어있습니다. 맵을 생성할 수 없습니다.");
            return;
        }

        objectDatabase = Resources.Load<ObjectDatabase>("Objects/GlobalObjectDatabase");
        if (objectDatabase == null)
        {
            Debug.LogError("[MapManager] ObjectDatabase를 'Resources/Objects/GlobalObjectDatabase' 경로에서 찾을 수 없습니다.");
            return;
        }

        // [수정] 각 바이옴의 타일 리스트를 임계값 기준으로 '내림차순' 정렬합니다.
        // 이렇게 하면 GetTileForValue에서 높은 임계값의 타일부터 확인할 수 있습니다.
        foreach (var biome in biomes)
        {
            biome.terrainTiles.Sort((a, b) => b.threshold.CompareTo(a.threshold));
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("mapSeed", out object mapSeedValue))
        {
            this.seed = (int)mapSeedValue;
            Debug.Log($"[MapManager] 방 속성에서 맵 시드({this.seed})를 찾았습니다. 맵 생성을 시작합니다.");
            GenerateMap();
        }
        else
        {
            Debug.LogError("[MapManager] 방 속성에서 'mapSeed'를 찾을 수 없습니다!");
        }
    }

    public void GenerateMap()
    {
        ClearMap();
        InitializeRandom(this.seed);
        // [신규] 바이옴 순서를 섞습니다.
        ShuffleBiomes();
        StampBaseMap();
        GenerateSurroundingTerrain();
        SpawnObjects();
        Debug.Log($"[MapManager] 맵 생성 완료. (시드: {this.seed})");
    }

    private void InitializeRandom(int syncedSeed)
    {
        pseudoRandom = new System.Random(syncedSeed);
        warpOffsetX = pseudoRandom.Next(-30000, 30000);
        warpOffsetY = pseudoRandom.Next(-30000, 30000);
    }

    // [신규] 바이옴 리스트의 순서를 무작위로 섞는 함수
    private void ShuffleBiomes()
    {
        shuffledBiomes = new List<Biome>(biomes);
        // Fisher-Yates shuffle 알고리즘을 사용하여 리스트를 섞습니다.
        int n = shuffledBiomes.Count;
        while (n > 1)
        {
            n--;
            int k = pseudoRandom.Next(n + 1);
            Biome value = shuffledBiomes[k];
            shuffledBiomes[k] = shuffledBiomes[n];
            shuffledBiomes[n] = value;
        }
        Debug.Log("[MapManager] 바이옴 순서를 섞었습니다.");
    }

    private void StampBaseMap()
    {
        if (baseMapPrefab == null)
        {
            Debug.LogWarning("[MapManager] baseMapPrefab이 할당되지 않았습니다. 베이스맵 스탬프를 건너뜁니다.");
            return;
        }
        BoundsInt bounds = baseMapPrefab.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int prefabTilePos = new Vector3Int(x, y, 0);
                TileBase tile = baseMapPrefab.GetTile(prefabTilePos);
                if (tile != null)
                {
                    Vector3Int targetPos = new Vector3Int(x + baseMapOffset.x, y + baseMapOffset.y, 0);
                    targetTilemap.SetTile(targetPos, tile);
                }
            }
        }
        Debug.Log("[MapManager] 베이스맵 스탬프 완료.");
    }

    private void GenerateSurroundingTerrain()
    {
        int tilesSetCount = 0;
        float terrainOffsetX = pseudoRandom.Next(-20000, 20000);
        float terrainOffsetY = pseudoRandom.Next(-20000, 20000);

        int startX = -mapWidth / 2;
        int startY = -mapHeight / 2;

        for (int x = startX; x < startX + mapWidth; x++)
        {
            for (int y = startY; y < startY + mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                // 베이스맵 등 이미 타일이 있는 곳은 건너뜁니다.
                if (targetTilemap.HasTile(tilePosition))
                {
                    continue;
                }

                Vector2Int warpedPos = GetWarpedPosition(x, y);
                Biome currentBiome = GetBiomeForGridPosition(warpedPos.x, warpedPos.y);
                if (currentBiome == null) continue;

                float terrainSampleX = (x + terrainOffsetX) * terrainNoiseScale;
                float terrainSampleY = (y + terrainOffsetY) * terrainNoiseScale;
                float terrainValue = Mathf.PerlinNoise(terrainSampleX, terrainSampleY);
                TileBase tileToSet = GetTileForValue(terrainValue, currentBiome.terrainTiles);

                if (tileToSet != null)
                {
                    targetTilemap.SetTile(tilePosition, tileToSet);
                    tilesSetCount++;
                }
            }
        }
        Debug.Log($"[MapManager] 주변 지형 생성 완료. 설정된 타일 수: {tilesSetCount}");
    }

    private Vector2Int GetWarpedPosition(int x, int y)
    {
        float warpSampleX = (x + warpOffsetX) * biomeWarpScale;
        float warpSampleY = (y + warpOffsetY) * biomeWarpScale;

        float warpNoiseX = (Mathf.PerlinNoise(warpSampleX, warpSampleY) * 2) - 1;
        float warpNoiseY = (Mathf.PerlinNoise(warpSampleY, warpSampleX) * 2) - 1;

        int warpedX = x + Mathf.RoundToInt(warpNoiseX * biomeWarpIntensity);
        int warpedY = y + Mathf.RoundToInt(warpNoiseY * biomeWarpIntensity);

        return new Vector2Int(warpedX, warpedY);
    }

    private Biome GetBiomeForGridPosition(int worldX, int worldY)
    {
        // [수정] 원본 바이옴 리스트 대신 섞인 바이옴 리스트를 사용합니다.
        int biomeCount = shuffledBiomes.Count;
        if (biomeCount == 0) return null;

        int gridCols = Mathf.CeilToInt(Mathf.Sqrt(biomeCount));
        int gridRows = Mathf.CeilToInt((float)biomeCount / gridCols);

        float cellWidth = (float)mapWidth / gridCols;
        float cellHeight = (float)mapHeight / gridRows;

        float normalizedX = worldX + (float)mapWidth / 2;
        float normalizedY = worldY + (float)mapHeight / 2;

        int col = Mathf.FloorToInt(normalizedX / cellWidth);
        int row = Mathf.FloorToInt(normalizedY / cellHeight);

        int biomeIndex = row * gridCols + col;

        if (biomeIndex >= 0 && biomeIndex < biomeCount)
        {
            return shuffledBiomes[biomeIndex];
        }

        int clampedRow = Mathf.Clamp(row, 0, gridRows - 1);
        int clampedCol = Mathf.Clamp(col, 0, gridCols - 1);
        int clampedIndex = clampedRow * gridCols + clampedCol;

        return shuffledBiomes[Mathf.Min(clampedIndex, biomeCount - 1)];
    }

    private void SpawnObjects()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[MapManager] 마스터 클라이언트가 아니므로 오브젝트 생성을 건너뜁니다.");
            return;
        }

        if (worldObjectPrefab == null)
        {
            Debug.LogError("[MapManager] 'World Object Prefab'이 할당되지 않았습니다. 오브젝트를 생성할 수 없습니다.");
            return;
        }

        if (objectContainer == null)
        {
            objectContainer = new GameObject("ObjectContainer").transform;
            objectContainer.SetParent(this.transform);
        }

        foreach (Transform child in objectContainer)
        {
            PhotonNetwork.Destroy(child.gameObject);
        }

        Debug.Log("[MapManager] 오브젝트 생성을 시작합니다.");
        
        // [신규] 오브젝트가 생성된 위치를 기록하여 중복 생성을 방지합니다.
        HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
        
        int startX = -mapWidth / 2;
        int startY = -mapHeight / 2;

        for (int x = startX; x < startX + mapWidth; x++)
        {
            for (int y = startY; y < startY + mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (!targetTilemap.HasTile(tilePosition))
                {
                    continue;
                }

                TileBase currentTile = targetTilemap.GetTile(tilePosition);
                Biome currentBiome = GetBiomeAt(tilePosition);

                if (currentBiome == null || currentBiome.spawnableObjects == null)
                {
                    continue;
                }

                foreach (var objectInfo in currentBiome.spawnableObjects)
                {
                    if (objectInfo.objectData == null) continue;

                    if (objectInfo.canSpawnOn.Count > 0 && !objectInfo.canSpawnOn.Contains(currentTile))
                    {
                        continue;
                    }

                    if (pseudoRandom.NextDouble() < objectInfo.density)
                    {
                        // [수정] 이 타일에 이미 오브젝트가 생성되었는지 확인합니다.
                        if (occupiedPositions.Contains(tilePosition))
                        {
                            continue; // 이미 오브젝트가 있으면 건너뜁니다.
                        }

                        int objectIndex = objectDatabase.GetIndex(objectInfo.objectData);
                        if (objectIndex == -1)
                        {
                            Debug.LogWarning($"[MapManager] 데이터베이스에서 오브젝트 '{objectInfo.objectData.name}'를 찾을 수 없습니다. 생성을 건너뜁니다.");
                            continue;
                        }

                        Vector3 spawnPos = targetTilemap.GetCellCenterWorld(tilePosition);
                        object[] instantiationData = { objectIndex };
                        
                        GameObject newObj = PhotonNetwork.Instantiate(worldObjectPrefab.name, spawnPos, Quaternion.identity, 0, instantiationData);
                        newObj.transform.SetParent(objectContainer);

                        // [수정] 생성된 위치를 기록하고, 이 타일에는 더 이상 다른 오브젝트를 생성하지 않도록 루프를 빠져나갑니다.
                        occupiedPositions.Add(tilePosition);
                        break; 
                    }
                }
            }
        }
        Debug.Log("[MapManager] 오브젝트 생성 완료.");
    }

    // [수정] 타일 선택 로직 변경
    private TileBase GetTileForValue(float value, List<TileInfo> tiles)
    {
        // 리스트가 임계값(threshold)의 '내림차순'으로 정렬되어 있다고 가정합니다.
        // 노이즈 값이 임계값보다 크거나 같은 첫 번째 타일을 반환합니다.
        foreach (var tileInfo in tiles)
        {
            if (value >= tileInfo.threshold)
            {
                return tileInfo.tileAsset;
            }
        }
        // 모든 임계값보다 낮은 경우, 가장 낮은 임계값의 타일(리스트의 마지막)을 반환합니다.
        if (tiles.Count > 0)
        {
            return tiles[tiles.Count - 1].tileAsset;
        }
        return null;
    }

    public void ClearMap()
    {
        if (targetTilemap != null)
        {
            targetTilemap.ClearAllTiles();
        }

        if (PhotonNetwork.IsMasterClient && objectContainer != null)
        {
            foreach (Transform child in objectContainer)
            {
                PhotonNetwork.Destroy(child.gameObject);
            }
        }
    }

    public Biome GetBiomeAt(Vector3Int worldPosition)
    {
        Vector2Int warpedPos = GetWarpedPosition(worldPosition.x, worldPosition.y);
        return GetBiomeForGridPosition(warpedPos.x, warpedPos.y);
    }
}