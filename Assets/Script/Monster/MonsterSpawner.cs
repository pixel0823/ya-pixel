using UnityEngine;
using Photon.Pun;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab; // 유니티 에디터에서 할당할 몬스터 프리팹

    // 몬스터를 생성하는 메서드
    public void SpawnMonster()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // PhotonNetwork.Instantiate를 사용하여 네트워크 상에 몬스터 프리팹을 생성합니다.
            // monsterPrefab 변수에 직접 할당하는 경우, Resources 폴더에 없어도 됩니다.
            PhotonNetwork.Instantiate(monsterPrefab.name, transform.position, Quaternion.identity);
            Debug.Log("Monster spawned via PhotonNetwork.Instantiate!");
        }
        else
        {
            Debug.LogWarning("Photon is not connected or ready. Cannot spawn monster.");
        }
    }

    // 테스트를 위해 특정 키를 누르면 몬스터를 생성하도록 할 수 있습니다.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) // 'M' 키를 누르면 몬스터 생성
        {
            SpawnMonster();
        }
    }
}