using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerName : MonoBehaviourPun
{
    public TMP_Text nameText;

    [PunRPC]
    void SetNameRPC(string name)
    {
        nameText.text = name;
    }

    public void SetName(string name)
    {
        photonView.RPC("SetNameRPC", RpcTarget.AllBuffered, name);
    }
}
