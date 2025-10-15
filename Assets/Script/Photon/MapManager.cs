using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

/// <summary>
/// [수정됨] 게임 씬이 로드될 때, Photon 방의 Custom Properties에 저장된 시드 값을 읽어 맵을 생성합니다.
/// </summary>
public class MapManager : MonoBehaviour
{
    [Header("맵 생성 설정")]
    [Tooltip("전체 맵의 가로(X) 크기입니다.")]
    public int mapWidth = 100;
    [Tooltip("전체 맵의 세로(Z) 크기입니다.")]
    public int mapHeight = 100;

    [Header("기본 맵 설정")]
    [Tooltip("중앙에 배치될 기본 맵 프리팹입니다.")]
    public GameObject baseMapPrefab;
    [Tooltip("기본 맵의 가로(X) 크기입니다.")]
    public int baseMapWidth = 20;
    [Tooltip("기본 맵의 세로(Z) 크기입니다.")]
    public int baseMapHeight = 20;

    [Header("절차적 지형 설정")]
    [Tooltip("지형 생성에 사용될 타일 프리팹 리스트입니다.")]
    public List<TileInfo> terrainTiles;
    [Tooltip("Perlin Noise의 스케일 값입니다.")]
    public float noiseScale = 0.1f;

    // 시드 값은 이제 Photon Room Properties에서 직접 읽어옵니다.
    private int seed;
    private System.Random pseudoRandom;

    [System.Serializable]
    public class TileInfo
    {
        public string name;
        public GameObject tilePrefab;
        [Range(0f, 1f)]
        public float threshold;
    }

    void Start()
    {
        // 이 스크립트는 "Map1" 같은 게임 씬에 존재해야 합니다.
        // 씬이 로드되면, 모든 클라이언트에서 이 Start 함수가 실행됩니다.

        // 먼저, 인스펙터 설정이 올바르게 되었는지 확인합니다.
        if (terrainTiles == null || terrainTiles.Count == 0)
        {
            Debug.LogError("[MapManager] 'Terrain Tiles' 리스트가 비어있습니다. 맵을 생성할 수 없습니다.");
            return;
        }

        // (개선) 임계값(threshold)을 기준으로 타일 리스트를 오름차순 정렬하여 안정성 확보
        terrainTiles.Sort((a, b) => a.threshold.CompareTo(b.threshold));

        // Photon 방의 Custom Properties에서 "mapSeed" 값을 가져옵니다.
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("mapSeed", out object mapSeedValue))
        {
            this.seed = (int)mapSeedValue;
            Debug.Log($"[MapManager] 방 속성에서 맵 시드({this.seed})를 찾았습니다. 맵 생성을 시작합니다.");
            GenerateMap();
        }
        else
        {
            Debug.LogError("[MapManager] 방 속성에서 'mapSeed'를 찾을 수 없습니다! CreateRoom 스크립트에서 시드가 정상적으로 설정되었는지 확인하세요.");
        }
    }

    /// <summary>
    /// 동기화된 시드 값을 사용하여 맵 생성을 시작하는 메인 메서드입니다.
    /// </summary>
    public void GenerateMap()
    {
        ClearMap();
        InitializeRandom(this.seed);
        InstantiateBaseMap();
        GenerateSurroundingTerrain();
        Debug.Log($"[MapManager] 맵 생성 완료. (시드: {this.seed})");
    }

    private void InitializeRandom(int syncedSeed)
    {
        pseudoRandom = new System.Random(syncedSeed);
    }

    private void InstantiateBaseMap()
    {
        if (baseMapPrefab != null)
        {
            Vector3 centerPosition = new Vector3(mapWidth / 2f, 0, mapHeight / 2f);
            Instantiate(baseMapPrefab, centerPosition, Quaternion.identity, this.transform);
        }
    }

    private void GenerateSurroundingTerrain()
    {
        float offsetX = pseudoRandom.Next(-10000, 10000);
        float offsetZ = pseudoRandom.Next(-10000, 10000);

        int baseStartX = (mapWidth - baseMapWidth) / 2;
        int baseEndX = baseStartX + baseMapWidth;
        int baseStartZ = (mapHeight - baseMapHeight) / 2;
        int baseEndZ = baseStartZ + baseMapHeight;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                if (x >= baseStartX && x < baseEndX && z >= baseStartZ && z < baseEndZ)
                {
                    continue;
                }

                float sampleX = (x + offsetX) * noiseScale;
                float sampleZ = (z + offsetZ) * noiseScale;
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);

                GameObject tileToInstantiate = GetTileForPerlinValue(perlinValue);
                if (tileToInstantiate != null)
                {
                    Instantiate(tileToInstantiate, new Vector3(x, 0, z), Quaternion.identity, this.transform);
                }
            }
        }
    }

    private GameObject GetTileForPerlinValue(float perlinValue)
    {
        foreach (var tileInfo in terrainTiles)
        {
            if (perlinValue <= tileInfo.threshold)
            {
                return tileInfo.tilePrefab;
            }
        }
        if (terrainTiles.Count > 0)
        {
            return terrainTiles[terrainTiles.Count - 1].tilePrefab;
        }
        return null;
    }

    public void ClearMap()
    {
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}