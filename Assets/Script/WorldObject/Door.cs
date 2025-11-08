using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// 문과의 상호작용을 처리합니다. IInteractable을 구현합니다.
/// - 외부 건물의 문: roomEntrance에 연결된 위치로 플레이어를 텔레포시킵니다.
///   이 때 플레이어의 원래 위치를 저장합니다.
/// - 방 내부의 입구 문(isRoomEntrance=true): F를 누르면 저장된 원래 위치로 복귀합니다.
/// 여러 플레이어 동시 사용을 위해 플레이어별 복귀 정보를 내부 맵에 저장합니다.
/// Photon 네트워크 사용 시 로컬 플레이어(PV.IsMine)만 텔레포를 수행합니다.
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    [Header("Door 설정")]
    [Tooltip("이 문이 방 내부 입구인지 여부입니다. (true이면 복귀 동작을 수행합니다)")]
    public bool isRoomEntrance = false;

    [Tooltip("이 문으로 들어갈 때 도착할 Transform (방 입구 위치). 외부 건물의 문에 설정하세요.")]
    public Transform roomEntrance;

    [Tooltip("(선택) 방에서 복귀할 위치를 강제로 지정하려면 사용하세요. 비워두면 입장 시 저장된 원래 위치로 복귀합니다.")]
    public Transform forcedReturnPoint;

    [Tooltip("상호작용 텍스트에 표시할 대상 이름(예: 방 이름 또는 건물 이름)")]
    public string destination = "어딘가";

    // 플레이어 InstanceID -> 복귀 정보 매핑
    private static Dictionary<int, ReturnInfo> returnMap = new Dictionary<int, ReturnInfo>();

    private class ReturnInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Transform parent;
    }

    public string GetInteractText()
    {
        return $"'F' 키를 눌러 {destination}(으)로 이동";
    }

    public void Interact(GameObject interactor)
    {
        if (interactor == null)
        {
            Debug.LogWarning("[Door] 상호작용 주체가 없습니다.");
            return;
        }

        PhotonView pv = interactor.GetComponent<PhotonView>();

        // 네트워크 플레이어의 경우 로컬 플레이어만 자신의 위치를 직접 변경
        bool canTeleport = (pv == null) || pv.IsMine;

        // 네트워크 환경에서는 InstanceID가 클라이언트마다 달라질 수 있으므로
        // PhotonView가 있으면 ActorNumber를 사용해 전역적으로 식별합니다.
        int key;
        if (pv != null)
        {
            // OwnerActorNr는 플레이어의 고유한 actor number를 반환합니다.
            key = pv.OwnerActorNr != 0 ? pv.OwnerActorNr : interactor.GetInstanceID();
        }
        else
        {
            key = interactor.GetInstanceID();
        }

        if (!isRoomEntrance)
        {
            // 외부 건물 문: 방 입구로 이동
            if (roomEntrance == null)
            {
                Debug.LogWarning($"[Door] {name}의 roomEntrance가 설정되어 있지 않습니다.");
                return;
            }

            // 현재 위치 저장
            var info = new ReturnInfo
            {
                position = interactor.transform.position,
                rotation = interactor.transform.rotation,
                parent = interactor.transform.parent
            };

            // 덮어쓰기 허용: 같은 플레이어가 여러 번 입장할 수 있음
            returnMap[key] = info;

            if (canTeleport)
            {
                interactor.transform.position = roomEntrance.position;
                interactor.transform.rotation = roomEntrance.rotation;
            }

            Debug.Log($"[Door] {interactor.name} 이(가) 문 '{destination}'으로 입장했습니다. 복귀 지점 저장됨.");
        }
        else
        {
            // 방 내부 입구: 저장된 복귀 위치로 돌아가기
            if (!returnMap.TryGetValue(key, out ReturnInfo info))
            {
                Debug.LogWarning($"[Door] {interactor.name} 의 복귀 위치가 없습니다. (아직 입장하지 않았거나 저장이 누락됨)");
                return;
            }

            // 우선 forcedReturnPoint가 있으면 그 위치로, 아니면 저장된 위치로 복귀
            if (canTeleport)
            {
                if (forcedReturnPoint != null)
                {
                    interactor.transform.position = forcedReturnPoint.position;
                    interactor.transform.rotation = forcedReturnPoint.rotation;
                }
                else
                {
                    interactor.transform.position = info.position;
                    interactor.transform.rotation = info.rotation;
                    interactor.transform.SetParent(info.parent);
                }
            }

            // 복귀 정보 제거
            returnMap.Remove(key);

            Debug.Log($"[Door] {interactor.name} 이(가) 방 '{destination}'에서 복귀했습니다.");
        }
    }
}
