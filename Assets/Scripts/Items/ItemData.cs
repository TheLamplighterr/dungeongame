using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    [TextArea]
    public string description;

    public ItemType itemType;
    public int value;

    public GameObject worldPrefab;
}