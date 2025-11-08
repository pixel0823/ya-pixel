using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonConnector : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // 마스터 서버에 접속을 시도합니다.
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버에 성공적으로 접속했을 때 호출됩니다.
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server!");
    }

    // 서버와의 연결이 끊겼을 때 호출됩니다.
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
    }
}
