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
    [Tooltip("생성될 바이옴 리스트입니다.")]
    public List<Biome> biomes;
    [Tooltip("바이옴 구분을 위한 Perlin Noise 스케일 값입니다. (작을수록 바이옴이 커짐)")]
    public float biomeNoiseScale = 0.02f;
    [Tooltip("바이옴 내 세부 지형을 위한 Perlin Noise 스케일 값입니다.")]
    public float terrainNoiseScale = 0.1f;

    private int seed;
    private System.Random pseudoRandom;

    [System.Serializable]
    public class TileInfo
    {
        public string name;
        public TileBase tileAsset;
        [Range(0f, 1f)]
        public float threshold;
    }

    // [신규] 바이옴 클래스 정의
    [System.Serializable]
    public class Biome
    {
        public string name;
        [Tooltip("이 바이옴을 결정하는 임계값입니다.")]
        [Range(0f, 1f)]
        public float threshold;
        [Tooltip("이 바이옴에서 사용될 타일 리스트입니다.")]
        public List<TileInfo> terrainTiles;
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

        // [수정] 바이옴 및 각 바이옴의 타일 리스트를 임계값 기준으로 정렬
        biomes.Sort((a, b) => a.threshold.CompareTo(b.threshold));
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
        Debug.Log($"[MapManager] 맵 생성 완료. (시드: {this.seed})");
    }

    private void InitializeRandom(int syncedSeed)
    {
        pseudoRandom = new System.Random(syncedSeed);
    }

    private void StampBaseMap()
    {
        if (baseMapPrefab == null) return;
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

    // [수정] 2중 Perlin Noise를 사용하도록 재작성
    private void GenerateSurroundingTerrain()
    {
        // 바이옴과 지형을 위한 별도의 노이즈 오프셋 생성
        float biomeOffsetX = pseudoRandom.Next(-10000, 10000);
        float biomeOffsetY = pseudoRandom.Next(-10000, 10000);
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

                // 1. 바이옴 결정
                float biomeSampleX = (x + biomeOffsetX) * biomeNoiseScale;
                float biomeSampleY = (y + biomeOffsetY) * biomeNoiseScale;
                float biomeValue = Mathf.PerlinNoise(biomeSampleX, biomeSampleY);
                Biome currentBiome = GetBiomeForValue(biomeValue);

                if (currentBiome == null) continue;

                // 2. 바이옴 내에서 세부 타일 결정
                float terrainSampleX = (x + terrainOffsetX) * terrainNoiseScale;
                float terrainSampleY = (y + terrainOffsetY) * terrainNoiseScale;
                float terrainValue = Mathf.PerlinNoise(terrainSampleX, terrainSampleY);
                TileBase tileToSet = GetTileForValue(terrainValue, currentBiome.terrainTiles);

                if (tileToSet != null)
                {
                    targetTilemap.SetTile(tilePosition, tileToSet);
                }
            }
        }
    }

    // [신규] 값에 따라 바이옴을 선택하는 함수
    private Biome GetBiomeForValue(float value)
    {
        foreach (var biome in biomes)
        {
            if (value <= biome.threshold)
            {
                return biome;
            }
        }
        if (biomes.Count > 0)
        {
            return biomes[biomes.Count - 1];
        }
        return null;
    }

    // [수정] 이름 변경 및 특정 타일 리스트를 받도록 수정
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
    }
}