using UnityEngine;
using Photon.Pun;
using YAPixel.World;
using YAPixel;

/// <summary>
/// 테스트 목적으로 월드 오브젝트 생성을 담당하는 스크립트입니다.
/// </summary>
public class ItemSpawnTester : MonoBehaviour
{
    [Tooltip("오브젝트 데이터베이스 (Resources/Objects/GlobalObjectDatabase)")]
    public ObjectDatabase objectDatabase;

    [Tooltip("데이터베이스에서 생성할 오브젝트의 인덱스")]
    public int objectIndexToSpawn = 0;

    void Update()
    {
        // 'T' 키를 누르면 오브젝트 생성 시도
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnObject();
        }
    }

    /// <summary>
    /// 마스터 클라이언트에서 월드 오브젝트를 생성합니다.
    /// </summary>
    public void SpawnObject()
    {
        if (objectDatabase == null)
        {
            Debug.LogError("ObjectDatabase가 할당되지 않았습니다! Inspector에서 설정해주세요.");
            return;
        }

        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        {
            Debug.LogError("Photon 룸에 연결되어 있지 않아 네트워크 오브젝트를 생성할 수 없습니다.");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("오브젝트 생성은 마스터 클라이언트만 가능합니다.");
            return;
        }

        Object objectToSpawn = objectDatabase.GetItem(objectIndexToSpawn);
        if (objectToSpawn == null)
        {
            Debug.LogError($"데이터베이스에서 인덱스 {objectIndexToSpawn}에 해당하는 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // WorldObject의 OnPhotonInstantiate에 전달될 데이터입니다.
        // 첫 번째 요소는 데이터베이스에서의 오브젝트 인덱스입니다.
        object[] instantiationData = { objectIndexToSpawn };

        // "WorldObject" 프리팹이 "Resources" 폴더에 있다고 가정합니다.
        // 실제 경로가 다를 경우 이 부분을 수정해야 합니다.
        string prefabName = "WorldObject";

        Debug.Log($"'{objectToSpawn.objectName}' 오브젝트를 월드에 생성합니다. (위치: {transform.position})");
        PhotonNetwork.Instantiate(prefabName, transform.position, Quaternion.identity, 0, instantiationData);
    }
}