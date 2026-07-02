using UnityEngine;

public class ItemWorldPickup : MonoBehaviour
{
    public ItemData itemData;

    private bool isPickedUp = false;

    // wird beim Spawnen gesetzt (optional für Drop-System)
    public void Init(ItemData item)
    {
        itemData = item;
    }

    private void OnTriggerEnter(Collider other)
    {
        //  schon eingesammelt? sofort raus
        if (isPickedUp) return;

        //  nur Spieler darf aufnehmen
        if (!other.CompareTag("Player"))
            return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();

        if (inv != null && itemData != null)
        {
            bool added = inv.AddItem(itemData);

            if (added)
            {
                isPickedUp = true;

                // verhindert doppelte Trigger in gleichem Frame
                gameObject.SetActive(false);

                // optional 
                // Destroy(gameObject);
            }
        }
    }
}