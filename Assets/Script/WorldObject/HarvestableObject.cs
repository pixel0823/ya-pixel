
using UnityEngine;
using Photon.Pun;
using YAPixel;

public class HarvestableObject : MonoBehaviour, IInteractable
{
    [Header("Harvestable Settings")]
    public int health = 100;
    public LootTable lootTable;
    public ItemDatabase itemDatabase;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void TakeDamage(int damage)
    {
        photonView.RPC("TakeDamageRPC", RpcTarget.All, damage);
    }

    [PunRPC]
    void TakeDamageRPC(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DropLoot();
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    void DropLoot()
    {
        if (lootTable != null && itemDatabase != null)
        {
            var itemsToDrop = lootTable.GenerateRandomItems();
            foreach (var itemDrop in itemsToDrop)
            {
                int itemIndex = itemDatabase.GetIndex(itemDrop.item);
                if (itemIndex != -1)
                {
                    object[] instantiationData = new object[] { itemIndex, itemDrop.amount };
                    PhotonNetwork.Instantiate("WorldItem", transform.position, Quaternion.identity, 0, instantiationData);
                }
            }
        }
    }

    public string GetInteractText()
    {
        return "Attack";
    }

    public void Interact(GameObject interactor)
    {
        // Left empty because the main interaction is TakeDamage
    }
}
