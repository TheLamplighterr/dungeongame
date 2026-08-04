using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;

    [Header("Audio")]
    [Tooltip("Sound beim Einsammeln des Items (z. B. Pop / Pling).")]
    [SerializeField] private AudioClip pickupSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

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
                // Sound an der Position des Items abspielen
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }

                Destroy(gameObject); // Item verschwindet nur wenn aufgenommen
            }
        }
    }
}