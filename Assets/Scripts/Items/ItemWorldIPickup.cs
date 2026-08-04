using UnityEngine;

public class ItemWorldPickup : MonoBehaviour
{
    public ItemData itemData;

    [Header("Audio")]
    [Tooltip("Sound beim Einsammeln des Items (z. B. Pop / Pling).")]
    [SerializeField] private AudioClip pickupSound;
    [Tooltip("Sound beim Landen/Droppen des Items auf dem Boden (z. B. leichtes Aufprallgeräusch).")]
    [SerializeField] private AudioClip dropSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    private bool isPickedUp = false;

    // wird beim Spawnen gesetzt (optional für Drop-System)
    public void Init(ItemData item)
    {
        itemData = item;

        // Spielt direkt beim Droppen/Spawnen in der Welt einen Drop-Sound ab
        if (dropSound != null)
        {
            AudioSource.PlayClipAtPoint(dropSound, transform.position, soundVolume);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // schon eingesammelt? sofort raus
        if (isPickedUp) return;

        // nur Spieler darf aufnehmen
        if (!other.CompareTag("Player"))
            return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();

        if (inv != null && itemData != null)
        {
            bool added = inv.AddItem(itemData);

            if (added)
            {
                isPickedUp = true;

                // Sound an der Position des Items abspielen
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }

                // verhindert doppelte Trigger in gleichem Frame
                gameObject.SetActive(false);

                // optional 
                Destroy(gameObject);
            }
        }
    }
}