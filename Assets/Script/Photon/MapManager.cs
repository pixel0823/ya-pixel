using UnityEngine;
using Photon.Pun;

/// <summary>
/// GameScene이 로드된 후 맵 생성을 주도적으로 시작하는 역할을 합니다.
/// 이 스크립트는 GameScene에 있는 게임 오브젝트에 추가되어야 하며, PhotonView 컴포넌트도 필요합니다.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class MapManager : MonoBehaviour
{
    private PhotonView photonView;

    private void Awake()
    {
        // RPC 호출을 위해 자신의 PhotonView 컴포넌트를 가져옵니다.
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        // 모든 클라이언트가 직접 방의 커스텀 프로퍼티에서 맵 시드를 읽어 맵 생성을 시작합니다.
        // 이 방식은 불필요한 RPC 호출을 없애고, PhotonView 초기화 관련 경쟁 상태 문제를 해결합니다.
        Debug.Log("MapManager: 방 정보에서 'mapSeed'를 확인하고 맵 생성을 시작합니다.");

        // 현재 방의 커스텀 프로퍼티에서 "mapSeed" 값을 가져옵니다.
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("mapSeed", out object seedValue))
        {
            // RPC를 사용하지 않고, 각 클라이언트가 로컬에서 직접 맵 생성 함수를 호출합니다.
            // 모든 클라이언트가 동일한 시드를 사용하므로 결과적으로 동일한 맵이 생성됩니다.
            GenerateMap((int)seedValue);
        }
        else
        {
            Debug.LogError("MapManager: 방 정보에서 'mapSeed'를 찾을 수 없습니다! 맵을 생성할 수 없습니다.");
        }
    }

    /// <summary>
    /// 모든 클라이언트에서 로컬로 호출되어 실제 맵과 플레이어를 생성하는 함수입니다.
    /// </summary>
    /// <param name="seed">방의 커스텀 프로퍼티에서 가져온 동기화된 맵 시드</param>
    private void GenerateMap(int seed)
    {
        Debug.Log($"--- MapManager: 맵 생성 함수 호출 (시드: {seed}) ---");

        // 맵 생성을 위해 시드를 사용합니다.
        // System.Random mapRandom = new System.Random(seed);

        Debug.Log("맵 레이아웃을 생성합니다...");
        // ... 여기에 시드를 기반으로 맵을 생성하는 코드를 작성합니다 ...
        Debug.Log("--- 맵 생성 완료 ---");

        // 플레이어 생성 로직은 PlayerManager가 담당합니다.
    }
}
