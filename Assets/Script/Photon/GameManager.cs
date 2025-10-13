using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Check if we are in a room
        if (PhotonNetwork.InRoom)
        {
            // Instantiate the player prefab located in the "Resources" folder
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            Debug.Log("Player instantiated.");
        }
        else
        {
            Debug.LogWarning("Not in a room, player not instantiated.");
        }
    }
}
