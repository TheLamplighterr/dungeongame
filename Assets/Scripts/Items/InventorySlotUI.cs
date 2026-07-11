using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;

    [HideInInspector] public InventoryUI inventoryUI;
    [HideInInspector] public int slotIndex;

    private ItemData currentItem;

    // =========================
    // SET ITEM
    // =========================
    public void SetItem(ItemData newItem)
    {
        currentItem = newItem;

        if (currentItem == null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = currentItem.icon;
        iconImage.color = new Color(1.2f, 1.2f, 1.2f, 1f);    }

    // =========================
    // USE ITEM
    // =========================
    public void OnClick()
    {
        if (currentItem == null)
            return;

        UseItem();
    }

    void UseItem()
    {
        // über InventoryUI → Player Transform → GetComponent
        if (inventoryUI == null || inventoryUI.player == null)
            return;

        PlayerHealth playerHealth =
            inventoryUI.player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        switch (currentItem.itemType)
        {
            case ItemType.Heal:
                playerHealth.Heal(currentItem.value);
                Debug.Log(" Heal used: " + currentItem.value);
                break;


            case ItemType.DamageBoost:
                PlayerAttack attack =
                    inventoryUI.player.GetComponent<PlayerAttack>();

                if (attack != null)
                {
                    attack.BoostDamage(currentItem.value, 10f);
                    Debug.Log("⚔ Damage Boost activated!");
                }
                break;
        }

        // Item entfernen
        if (inventoryUI != null)
        {
            inventoryUI.inventory.RemoveItem(slotIndex);
            inventoryUI.UpdateUI();
        }
    }

    // =========================
    // DROP
    // =========================
    void Update()
    {
        if (currentItem == null)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            inventoryUI?.DropItem(slotIndex);
        }
    }
}