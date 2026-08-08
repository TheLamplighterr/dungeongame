using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image iconImage;

    [HideInInspector] public InventoryUI inventoryUI;
    [HideInInspector] public int slotIndex;

    private ItemData currentItem;
    private bool isHovered = false;
    public bool isEquipped = false; 

    [Header("Keybindings")]
    [SerializeField] private KeyCode dropKey = KeyCode.Q;

    // =========================
    // SET ITEM
    // =========================
    public void SetItem(ItemData newItem)
    {
        // WICHTIG: isEquipped nur zurücksetzen, wenn der Slot LEER wird (null)
        if (newItem == null)
        {
            currentItem = null;
            iconImage.enabled = false;
            iconImage.sprite = null;
            isEquipped = false;
            return;
        }

        // Falls sich das Item geändert hat, altes Item ausziehen
        if (currentItem != newItem && isEquipped)
        {
            UnequipThisItem();
        }

        currentItem = newItem;
        iconImage.enabled = true;
        iconImage.sprite = currentItem.icon;

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (iconImage == null) return;

        if (isEquipped)
        {
            iconImage.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Grau = Ausgerüstet       
        }
        else
        {
            iconImage.color = Color.white; // Normal
        }
    }

    void Update()
    {
        if (isHovered && currentItem != null && Input.GetKeyDown(dropKey))
        {
            DropThisItem();
        }
    }

    // =========================
    // MOUSE EVENTS
    // =========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (inventoryUI != null) inventoryUI.SelectSlot(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            UseItem();
        }
    }

    // =========================
    // ITEM VERWENDUNG & EQUIP
    // =========================
   void UseItem()
    {
        if (inventoryUI == null || inventoryUI.player == null) return;

        PlayerHealth playerHealth = inventoryUI.player.GetComponent<PlayerHealth>();
        PlayerAttack playerAttack = inventoryUI.player.GetComponent<PlayerAttack>();

        float itemValue = GetItemValue(currentItem);
        bool isConsumable = true;

        switch (currentItem.itemType)
        {
            case ItemType.Heal:
                // Löst Heal() aus -> Spielt automatisch das Heil-Partikel im PlayerHealth ab!
                if (playerHealth != null) playerHealth.Heal((int)itemValue);
                isConsumable = true;
                break;

            case ItemType.DamageBoost:
                // Löst BoostDamage() aus -> Spielt automatisch das Boost-Partikel im PlayerAttack ab!
                if (playerAttack != null) playerAttack.BoostDamage((int)itemValue, 10f);
                isConsumable = true;
                break;

            case ItemType.SuperPotion:
                if (playerHealth != null) playerHealth.Heal((int)itemValue);
                if (playerAttack != null) playerAttack.BoostDamage((int)itemValue, 10f);
                isConsumable = true;
                break;

            case ItemType.ArmorItem:
                isConsumable = false;
                if (!isEquipped)
                {
                    // Falls bereits eine Rüstung an ist, erst ablegen
                    if (inventoryUI.equippedArmorSlotIndex != -1)
                    {
                        InventorySlotUI oldSlot = inventoryUI.GetSlotByIndex(inventoryUI.equippedArmorSlotIndex);
                        if (oldSlot != null) oldSlot.UnequipThisItem();
                    }

                    // Rüstungs-Bonus auf HP anrechnen OHNE das Trank-Partikel (healVFX) auszulösen!
                    if (playerHealth != null) 
                    {
                        playerHealth.maxHealth += (int)itemValue;
                        playerHealth.currentHealth += (int)itemValue;
                        // Rufe nur das UI-Update auf, damit keine Trank-Partikel spielen
                        // (falls du eine öffentliche UpdateUI Methode in PlayerHealth hast)
                    }

                    inventoryUI.equippedArmorSlotIndex = slotIndex;
                    isEquipped = true;
                }
                else
                {
                    UnequipThisItem();
                }
                break;

            case ItemType.PermanentDamage:
                isConsumable = false;
                if (!isEquipped)
                {
                    if (inventoryUI.equippedDamageSlotIndex != -1)
                    {
                        if (playerAttack != null) playerAttack.AddPermanentDamage(-(int)itemValue);
                    }

                    if (playerAttack != null) playerAttack.AddPermanentDamage((int)itemValue);
                    inventoryUI.equippedDamageSlotIndex = slotIndex;
                    isEquipped = true;
                }
                else
                {
                    UnequipThisItem();
                }
                break;
        }

        UpdateVisuals();

        if (inventoryUI.inventory != null)
        {
            inventoryUI.inventory.PlayUseSound(currentItem);
        }

        if (isConsumable)
        {
            inventoryUI.inventory.RemoveItem(slotIndex);
        }

        inventoryUI.UpdateUI();
        inventoryUI.SelectSlot(null);
    }
    public void UnequipThisItem()
{
    if (!isEquipped || currentItem == null || inventoryUI == null || inventoryUI.player == null) return;

    float itemValue = GetItemValue(currentItem);

    if (currentItem.itemType == ItemType.ArmorItem)
    {
        PlayerHealth playerHealth = inventoryUI.player.GetComponent<PlayerHealth>();
        if (playerHealth != null) 
        {
            // Sanftes Abziehen der Bonus-HP, ohne TakeDamage (und somit ohne GameOver) auszulösen!
            // Stellt sicher, dass die HP nie unter 1 fällt beim Ausziehen.
            int newHealth = Mathf.Max(1, playerHealth.currentHealth - (int)itemValue);
            playerHealth.currentHealth = newHealth;
            
            // Falls du einen HP-Balken/UI hast, rufe hier deine UI-Update Methode vom PlayerHealth auf
            // playerHealth.UpdateHealthUI(); 
        }
        if (inventoryUI.equippedArmorSlotIndex == slotIndex) inventoryUI.equippedArmorSlotIndex = -1;
    }
    else if (currentItem.itemType == ItemType.PermanentDamage)
    {
        PlayerAttack playerAttack = inventoryUI.player.GetComponent<PlayerAttack>();
        if (playerAttack != null) playerAttack.AddPermanentDamage(-(int)itemValue);
        if (inventoryUI.equippedDamageSlotIndex == slotIndex) inventoryUI.equippedDamageSlotIndex = -1;
    }

    isEquipped = false;
    UpdateVisuals();
}

    // =========================
    // HELFER-FUNKTIONEN
    // =========================
    private float GetItemValue(ItemData item)
    {
        if (item == null) return 0f;

        var valueField = item.GetType().GetField("value");
        if (valueField != null) return System.Convert.ToSingle(valueField.GetValue(item));

        var amountField = item.GetType().GetField("amount");
        if (amountField != null) return System.Convert.ToSingle(amountField.GetValue(item));

        var healField = item.GetType().GetField("healAmount");
        if (healField != null) return System.Convert.ToSingle(healField.GetValue(item));

        return 0f;
    }

    void DropThisItem()
    {
        if (isEquipped)
        {
            UnequipThisItem();
        }

        if (inventoryUI != null)
        {
            inventoryUI.DropItem(slotIndex);
        }
    }

    private void PlayItemVFX()
{
    if (currentItem != null && currentItem.equipParticlePrefab != null && inventoryUI != null && inventoryUI.player != null)
    {
        // Spawnt das Partikelsystem an der Position des Spielers
        GameObject vfx = Instantiate(currentItem.equipParticlePrefab, inventoryUI.player.position, Quaternion.identity);

        // Heftet den Effekt an den Spieler an (falls er sich bewegt)
        vfx.transform.SetParent(inventoryUI.player);

        // Zerstört das Partikel-Objekt automatisch nach 3 Sekunden
        Destroy(vfx, 3f);
    }
}
}