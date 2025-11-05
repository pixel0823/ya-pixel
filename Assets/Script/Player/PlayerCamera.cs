using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float smoothing = 0.2f;
    [SerializeField] Vector2 minCameraBoundary;
    [SerializeField] Vector2 maxCameraBoundary;

    private Transform player;

    void Start()
    {
        // 로컬 플레이어를 찾아서 할당
        FindLocalPlayer();
    }

    void FindLocalPlayer()
    {
        // 씬의 모든 PlayerMovement를 가진 오브젝트 검색
        PlayerMovement[] players = FindObjectsOfType<PlayerMovement>();

        foreach (PlayerMovement playerObj in players)
        {
            PhotonView photonView = playerObj.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
            {
                player = playerObj.transform;
                break;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("Local player not found!");
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindLocalPlayer();
            return;
        }

        Vector3 targetPos = new Vector3(player.position.x, player.position.y, transform.position.z);

        targetPos.x = Mathf.Clamp(targetPos.x, minCameraBoundary.x, maxCameraBoundary.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minCameraBoundary.y, maxCameraBoundary.y);

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothing);
    }

    void OnDrawGizmos()
    {
        // 빨간색으로 boundary 박스 그리기
        Gizmos.color = Color.red;

        // 박스의 네 모서리 좌표 계산
        Vector3 bottomLeft = new Vector3(minCameraBoundary.x, minCameraBoundary.y, transform.position.z);
        Vector3 bottomRight = new Vector3(maxCameraBoundary.x, minCameraBoundary.y, transform.position.z);
        Vector3 topLeft = new Vector3(minCameraBoundary.x, maxCameraBoundary.y, transform.position.z);
        Vector3 topRight = new Vector3(maxCameraBoundary.x, maxCameraBoundary.y, transform.position.z);

        // 박스의 네 변 그리기
        Gizmos.DrawLine(bottomLeft, bottomRight);  // 아래
        Gizmos.DrawLine(bottomRight, topRight);    // 오른쪽
        Gizmos.DrawLine(topRight, topLeft);        // 위
        Gizmos.DrawLine(topLeft, bottomLeft);      // 왼쪽
    }
}
