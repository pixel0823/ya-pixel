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

    // [수정] 단일 지형 설정 대신 바이옴 기반 설정으로 변경
    [Header("바이옴 및 지형 설정")]
    [Tooltip("생성될 바이옴 리스트입니다. 리스트 순서대로 그리드에 배치됩니다.")]
    public List<Biome> biomes;
    [Tooltip("바이옴 내 세부 지형을 위한 Perlin Noise 스케일 값입니다.")]
    public float terrainNoiseScale = 0.1f;
    [Space(10)]
    [Tooltip("바이옴 경계의 왜곡(울퉁불퉁함) 정도를 조절합니다.")]
    public float biomeWarpScale = 0.05f;
    [Tooltip("바이옴 경계의 왜곡 강도(최대 변위)를 조절합니다.")]
    public float biomeWarpIntensity = 20f;

    [Header("오브젝트 생성 설정")]
    [Tooltip("생성될 월드 오브젝트의 프리팹입니다. (WorldObject 스크립트 포함)")]
    public GameObject worldObjectPrefab;

    private int seed;
    private System.Random pseudoRandom;
    // [신규] 왜곡 노이즈를 위한 오프셋
    private float warpOffsetX, warpOffsetY;
    private Transform objectContainer;
    private ObjectDatabase objectDatabase;

    [System.Serializable]
    public class TileInfo
    {
        public string name;
        public TileBase tileAsset;
        [Range(0f, 1f)]
        public float threshold;
    }

    // [수정] 바이옴 클래스에서 threshold 제거
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
        // [수정] 바이옴 리스트 유효성 검사
        if (biomes == null || biomes.Count == 0)
        {
            Debug.LogError("[MapManager] 'Biomes' 리스트가 비어있습니다. 맵을 생성할 수 없습니다.");
            return;
        }

        // [신규] 오브젝트 데이터베이스 로드
        objectDatabase = Resources.Load<ObjectDatabase>("Objects/GlobalObjectDatabase");
        if (objectDatabase == null)
        {
            Debug.LogError("[MapManager] ObjectDatabase를 'Resources/Objects/GlobalObjectDatabase' 경로에서 찾을 수 없습니다.");
            return;
        }

        // [수정] 각 바이옴의 타일 리스트를 임계값 기준으로 정렬
        foreach (var biome in biomes)
        {
            biome.terrainTiles.Sort((a, b) => a.threshold.CompareTo(b.threshold));
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
        StampBaseMap();
        GenerateSurroundingTerrain();
        Debug.Log($"[MapManager] GenerateSurroundingTerrain 완료 후 targetTilemap.cellBounds: {targetTilemap.cellBounds}");
        SpawnObjects();
        Debug.Log($"[MapManager] 맵 생성 완료. (시드: {this.seed})");
    }

    private void InitializeRandom(int syncedSeed)
    {
        pseudoRandom = new System.Random(syncedSeed);
        // [신규] 왜곡 노이즈 오프셋 초기화
        warpOffsetX = pseudoRandom.Next(-30000, 30000);
        warpOffsetY = pseudoRandom.Next(-30000, 30000);
    }

    private void StampBaseMap()
    {
        if (baseMapPrefab == null)
        {
            Debug.LogWarning("[MapManager] baseMapPrefab이 할당되지 않았습니다. 베이스맵 스탬프를 건너뜁니다.");
            return;
        }
        Debug.Log($"[MapManager] baseMapPrefab.cellBounds: {baseMapPrefab.cellBounds}");
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
        Debug.Log($"[MapManager] StampBaseMap 완료 후 targetTilemap.cellBounds: {targetTilemap.cellBounds}");
    }

    // [수정] 경계 왜곡을 포함한 바이옴 결정 로직으로 변경
    private void GenerateSurroundingTerrain()
    {
        int tilesSetCount = 0;
        // 지형을 위한 노이즈 오프셋 생성
        float terrainOffsetX = pseudoRandom.Next(-20000, 20000);
        float terrainOffsetY = pseudoRandom.Next(-20000, 20000);

        int startX = -mapWidth / 2;
        int startY = -mapHeight / 2;

        for (int x = startX; x < startX + mapWidth; x++)
        {
            for (int y = startY; y < startY + mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (targetTilemap.HasTile(tilePosition))
                {
                    continue;
                }

                // 1. 좌표를 왜곡하여 바이옴 결정
                Vector2Int warpedPos = GetWarpedPosition(x, y);
                Biome currentBiome = GetBiomeForGridPosition(warpedPos.x, warpedPos.y);
                if (currentBiome == null) continue;

                // 2. 바이옴 내에서 세부 타일 결정
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
        Debug.Log($"[MapManager] GenerateSurroundingTerrain에서 설정된 타일 수: {tilesSetCount}");
    }

    // [신규] Perlin Noise를 사용하여 좌표를 왜곡하는 함수
    private Vector2Int GetWarpedPosition(int x, int y)
    {
        float warpSampleX = (x + warpOffsetX) * biomeWarpScale;
        float warpSampleY = (y + warpOffsetY) * biomeWarpScale;

        // Perlin Noise 결과값을 [-1, 1] 범위로 변환
        float warpNoiseX = (Mathf.PerlinNoise(warpSampleX, warpSampleY) * 2) - 1;
        float warpNoiseY = (Mathf.PerlinNoise(warpSampleY, warpSampleX) * 2) - 1; // X, Y를 반대로 넣어 비대칭적인 왜곡 생성

        int warpedX = x + Mathf.RoundToInt(warpNoiseX * biomeWarpIntensity);
        int warpedY = y + Mathf.RoundToInt(warpNoiseY * biomeWarpIntensity);

        return new Vector2Int(warpedX, warpedY);
    }

    // [수정] 이름 변경: GetBiomeForGridPosition
    private Biome GetBiomeForGridPosition(int worldX, int worldY)
    {
        int biomeCount = biomes.Count;
        if (biomeCount == 0) return null;

        // 그리드 차원 계산 (가급적 정사각형에 가깝게)
        int gridCols = Mathf.CeilToInt(Mathf.Sqrt(biomeCount));
        int gridRows = Mathf.CeilToInt((float)biomeCount / gridCols);

        // 각 그리드 셀의 크기
        float cellWidth = (float)mapWidth / gridCols;
        float cellHeight = (float)mapHeight / gridRows;

        // 월드 좌표를 [0, mapWidth/mapHeight] 범위로 정규화
        float normalizedX = worldX + (float)mapWidth / 2;
        float normalizedY = worldY + (float)mapHeight / 2;

        // 정규화된 좌표를 그리드 인덱스로 변환
        int col = Mathf.FloorToInt(normalizedX / cellWidth);
        int row = Mathf.FloorToInt(normalizedY / cellHeight);

        // 1D 인덱스로 변환
        int biomeIndex = row * gridCols + col;

        // 인덱스가 바이옴 리스트 범위 내에 있는지 확인
        if (biomeIndex >= 0 && biomeIndex < biomeCount)
        {
            return biomes[biomeIndex];
        }

        // 범위를 벗어나는 경우 가장 가까운 그리드 셀의 바이옴으로 대체
        int clampedRow = Mathf.Clamp(row, 0, gridRows - 1);
        int clampedCol = Mathf.Clamp(col, 0, gridCols - 1);
        int clampedIndex = clampedRow * gridCols + clampedCol;

        return biomes[Mathf.Min(clampedIndex, biomeCount - 1)];
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

        // 기존 오브젝트 삭제 (마스터 클라이언트에서만)
        foreach (Transform child in objectContainer)
        {
            PhotonNetwork.Destroy(child.gameObject);
        }

                Debug.Log("[MapManager] 오브젝트 생성을 시작합니다.");
        
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
                            }
                        }
                    }
                }
                Debug.Log("[MapManager] 오브젝트 생성 완료.");    }

    // [수정 없음] 특정 타일 리스트 내에서 값에 따라 타일을 선택하는 함수
    private TileBase GetTileForValue(float value, List<TileInfo> tiles)
    {
        foreach (var tileInfo in tiles)
        {
            if (value <= tileInfo.threshold)
            {
                return tileInfo.tileAsset;
            }
        }
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

    // [수정] 외부용 GetBiomeAt 함수도 왜곡 로직을 사용하도록 변경
    public Biome GetBiomeAt(Vector3Int worldPosition)
    {
        Vector2Int warpedPos = GetWarpedPosition(worldPosition.x, worldPosition.y);
        return GetBiomeForGridPosition(warpedPos.x, warpedPos.y);
    }
}