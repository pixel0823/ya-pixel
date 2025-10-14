// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Photon.Pun;
// using Photon.Realtime;

// public class LobbyManager : MonoBehaviourPunCallbacks
// {
//     [SerializeField]
//     private CreateRoom createRoom; // 인스펙터에서 연결

//     void Start()
//     {

//     }

//     public void CreateRoom()
//     {
//         int maxPlayers = createRoom.GetMaxPlayerCount(); // CreateRoom에서 값 가져오기

//         RoomOptions roomOptions = new RoomOptions();
//         roomOptions.MaxPlayers = (byte)maxPlayers;
//         roomOptions.IsVisible = true;

//         PhotonNetwork.CreateRoom("RoomName", roomOptions, TypedLobby.Default);
//     }

//     public override void OnCreatedRoom()
//     {
//         base.OnCreatedRoom();
//         print("OnCreatedRoom");
//     }

//     public override void OnCreateRoomFailed(short returnCode, string message)
//     {
//         base.OnCreateRoomFailed(returnCode, message);
//         print("OnCreateRoomFailed: " + returnCode + " , " + message);
//         JoinRoom(); // 방 생성 실패 시 방에 참여 시도
//     }

//     public void JoinRoom()
//     {
//         PhotonNetwork.JoinRoom("RoomName");
//     }

//     public override void OnJoinedRoom()
//     {
//         base.OnJoinedRoom();
//         print("OnJoinedRoom");
//         // 예시: 게임 씬으로 이동
//         // PhotonNetwork.LoadLevel("GameScene");
//     }

//     public override void OnJoinRoomFailed(short returnCode, string message)
//     {
//         base.OnJoinRoomFailed(returnCode, message);
//         print("OnJoinRoomFailed: " + returnCode + " , " + message);
//     }

//     void OnRoomListUpdate(List<RoomInfo> roomList)
//     {
//         foreach (RoomInfo r in roomList)
//         {
//             print("Room Name: " + r.Name + " Player Count: " + r.PlayerCount + "/" + r.MaxPlayers); 
            
//         }
//     }

//     void Update()
//     {
//         //OnRoomListUpdate(PhotonNetwork.GetRoomList());
//     }
// }
