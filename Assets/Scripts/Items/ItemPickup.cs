using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            bool added = inventory.AddItem(itemData);

            if (added)
            {
                Destroy(gameObject); // Item verschwindet nur wenn aufgenommen
            }
        }
    }
}