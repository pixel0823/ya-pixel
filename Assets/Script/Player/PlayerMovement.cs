using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    //private PhotonView photonView;
    private Animator animator; // 애니메이터 컴포넌트
    public float moveSpeed = 5f;
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    // 애니메이션 동기화를 위한 변수
    private float networkMoveX = 0f;
    private float networkMoveY = 0f;
    private float networkLastMoveX = 0f;
    private float networkLastMoveY = -1f; // 기본값은 정면(아래)을 보도록 설정
    private bool networkIsWalking = false;

    void Awake()
    {
        animator = GetComponent<Animator>(); // 애니메이터 컴포넌트 가져오기
        if (photonView == null)
        {
            Debug.LogError("PlayerMovement is missing a PhotonView component. Please add one in the Inspector.");
        }
        if (animator == null)
        {
            Debug.LogError("PlayerMovement is missing an Animator component. Please add one in the Inspector.");
        }
    }

    void Update()
    {
        if (photonView != null && photonView.IsMine)
        {
            // 로컬 플레이어의 입력 및 이동 처리
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            bool isWalking = (moveX != 0 || moveY != 0);

            Vector3 move = new Vector3(moveX, moveY, 0).normalized;
            transform.position += move * moveSpeed * Time.deltaTime;

            // 애니메이터 파라미터 설정
            animator.SetBool("IsWalking", isWalking);
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveY", moveY);

            // 걷고 있을 때만 마지막 방향을 업데이트
            if (isWalking)
            {
                animator.SetFloat("LastMoveX", moveX);
                animator.SetFloat("LastMoveY", moveY);
            }
        }
        else
        {
            // 원격 플레이어의 애니메이션 파라미터 적용
            animator.SetBool("IsWalking", networkIsWalking);
            animator.SetFloat("MoveX", networkMoveX);
            animator.SetFloat("MoveY", networkMoveY);
            animator.SetFloat("LastMoveX", networkLastMoveX);
            animator.SetFloat("LastMoveY", networkLastMoveY);
        }
    }

    void LateUpdate()
    {
        if (photonView != null && !photonView.IsMine)
        {
            // 원격 플레이어의 위치를 부드럽게 보간
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 이 플레이어는 우리 소유이므로, 다른 플레이어에게 데이터를 전송합니다
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(animator.GetBool("IsWalking"));
            stream.SendNext(animator.GetFloat("MoveX"));
            stream.SendNext(animator.GetFloat("MoveY"));
            stream.SendNext(animator.GetFloat("LastMoveX"));
            stream.SendNext(animator.GetFloat("LastMoveY"));
        }
        else
        {
            // 네트워크 플레이어이므로, 데이터를 수신합니다
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkIsWalking = (bool)stream.ReceiveNext();
            networkMoveX = (float)stream.ReceiveNext();
            networkMoveY = (float)stream.ReceiveNext();
            networkLastMoveX = (float)stream.ReceiveNext();
            networkLastMoveY = (float)stream.ReceiveNext();
        }
    }
}