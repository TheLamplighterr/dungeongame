using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab;
    [TextArea] public string description;

    public ItemType itemType;
    public int value; // <-- Hier gefehlt! (z.B. für Heal-Punkte oder Boost-Wert)

    [Header("Audio")]
    public AudioClip useSound;

    [Header("VFX / Partikel")]
    public GameObject equipParticlePrefab; // Das Partikel-Prefab für dieses spezifische Item
    
}