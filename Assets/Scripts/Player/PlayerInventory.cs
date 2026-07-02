using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int inventorySize = 12;
    public ItemData[] items;

    void Awake()
    {
        items = new ItemData[inventorySize];
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
}