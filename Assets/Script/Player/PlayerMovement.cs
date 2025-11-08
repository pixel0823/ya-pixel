using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PlayerStats))]
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
    private bool networkIsMining = false;

    // 외부에서 마지막 이동 방향을 읽을 수 있도록 public 프로퍼티 추가
    public float LastMoveX => networkLastMoveX;
    public float LastMoveY => networkLastMoveY;

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
        // photonView.IsMine이 true일 때만 키보드 입력을 받아서 직접 캐릭터를 움직입니다.
        // 이렇게 하면 다른 사람의 캐릭터가 내 키보드 입력에 반응하지 않습니다.
        if (photonView.IsMine)
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
                networkLastMoveX = moveX;
                networkLastMoveY = moveY;
            }

            // StatusManager에 걷기 상태 전달
            if (StatusManager.Instance != null)
            {
                StatusManager.Instance.UpdateWalkingState(isWalking);
            }

            // 마우스 좌클릭으로 공격
            if (Input.GetMouseButtonDown(0))
            {
                photonView.RPC("RPC_SetMining", RpcTarget.All, true);
                Attack();

                // StatusManager에 채광/공격 알림
                if (StatusManager.Instance != null)
                {
                    StatusManager.Instance.OnMining();
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                photonView.RPC("RPC_SetMining", RpcTarget.All, false);
            }
        }
        else
        {
            // 원격 플레이어(다른 사람 캐릭터)의 위치를 부드럽게 보간하여 움직임을 표현합니다.
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10);
            // 원격 플레이어의 애니메이션 파라미터 적용
            animator.SetBool("IsWalking", networkIsWalking);
            animator.SetFloat("MoveX", networkMoveX);
            animator.SetFloat("MoveY", networkMoveY);
            animator.SetFloat("LastMoveX", networkLastMoveX);
            animator.SetFloat("LastMoveY", networkLastMoveY);
            animator.SetBool("IsMining", networkIsMining);
        }
    }

    private void Attack()
    {
        // 공격 방향 결정
        Vector2 attackDirection = new Vector2(networkLastMoveX, networkLastMoveY).normalized;
        if (attackDirection == Vector2.zero) attackDirection = Vector2.down; // 기본값

        // 공격 위치 계산
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * 0.3f; // 사거리

        // 해당 위치에 있는 모든 몬스터를 감지
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPosition, new Vector2(0.5f, 0.5f), 0);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                MonsterAI monster = hit.GetComponent<MonsterAI>();
                if (monster != null && monster.photonView != null)
                {
                    // Master Client를 통해 데미지 처리 요청
                    monster.photonView.RPC("TakeDamage", RpcTarget.MasterClient, GetComponent<PlayerStats>().attackDamage);
                    Debug.Log($"{hit.name}에게 {GetComponent<PlayerStats>().attackDamage}의 데미지를 입혔습니다.");
                }
            }
            else if (hit.GetComponent<WorldObject>() != null)
            {
                WorldObject harvestable = hit.GetComponent<WorldObject>();
                // Interact 메소드를 호출하여 도구에 따른 데미지 계산 로직을 사용합니다.
                harvestable.Interact(gameObject);
            }
        }
    }

    [PunRPC]
    private void RPC_SetMining(bool isMining)
    {
        animator.SetBool("IsMining", isMining);
    }

    public void Teleport(Vector3 destination)
    {
        if (photonView.IsMine)
        {
            transform.position = destination;
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
            stream.SendNext(animator.GetBool("IsMining"));
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
            networkIsMining = (bool)stream.ReceiveNext();
        }
    }
}