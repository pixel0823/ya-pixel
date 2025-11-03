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
}
