// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Photon.Pun;

// public class PlayerMovement : MonoBehaviourPunCallbacks
// {
//     private PhotonView photonView;
//     public float moveSpeed = 5f;

//     void Start()
//     {
//         photonView = GetComponent<PhotonView>();
//     }

//     void Update()
//     {
//         if (photonView.IsMine)
//         {
//             // Handle player input and movement
//             float moveX = Input.GetAxis("Horizontal");
//             float moveY = Input.GetAxis("Vertical");

//             Vector3 move = new Vector3(moveX, moveY, 0);
//             transform.position += move * moveSpeed * Time.deltaTime;
//         }
//     }
// }