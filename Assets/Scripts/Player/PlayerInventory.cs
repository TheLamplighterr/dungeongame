using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int inventorySize = 12;
    public ItemData[] items;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Sound, der beim Wegwerfen/Droppen eines Items gespielt wird.")]
    [SerializeField] private AudioClip dropSound;
    [Tooltip("Standard-Sound für das Benutzen/Einsetzen eines Items (falls im ItemData kein eigener Sound hinterlegt ist).")]
    [SerializeField] private AudioClip defaultUseSound;

    void Awake()
    {
        items = new ItemData[inventorySize];

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public bool AddItem(ItemData item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                Debug.Log("Item added: " + item.itemName);
                return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Length)
            return;

        items[index] = null;
    }

    public void PlayDropSound()
    {
        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }

    /// <summary>
    /// Spielt den Benutzungssound für ein Item ab. 
    /// Verwendet erst den spezifischen Sound aus ItemData, sonst den Standard-Use-Sound.
    /// </summary>
    public void PlayUseSound(ItemData item)
    {
        if (audioSource == null) return;

        // 1. Hat das Item einen eigenen Sound?
        if (item != null && item.useSound != null)
        {
            audioSource.PlayOneShot(item.useSound);
        }
        // 2. Wenn nicht, nimm den Standard-Use-Sound
        else if (defaultUseSound != null)
        {
            audioSource.PlayOneShot(defaultUseSound);
        }
    }
}