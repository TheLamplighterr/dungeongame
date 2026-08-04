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
    private bool isHovered = false; // Merkt sich, ob die Maus gerade auf DIESEM Slot steht

    [Header("Keybindings")]
    [SerializeField] private KeyCode dropKey = KeyCode.Q; // Standardmäßig 'Q' (kannst du im Inspector ändern)

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
        iconImage.color = Color.white;
    }

    void Update()
    {
        // Wenn die Maus auf diesem Slot steht, ein Item da ist UND 'Q' gedrückt wird -> Droppen!
        if (isHovered && currentItem != null && Input.GetKeyDown(dropKey))
        {
            DropThisItem();
        }
    }

    // =========================
    // MOUSE HOVER EVENTS
    // =========================
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (inventoryUI != null)
        {
            inventoryUI.SelectSlot(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // =========================
    // MOUSE CLICK (NUR NOCH LINKSKLICK ZUM BENUTZEN)
    // =========================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Nur noch Linksklick zum Verbrauchen / Benutzen!
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            UseItem();
        }
    }

    void UseItem()
    {
        if (inventoryUI == null || inventoryUI.player == null)
            return;

        PlayerHealth playerHealth = inventoryUI.player.GetComponent<PlayerHealth>();

        switch (currentItem.itemType)
        {
            case ItemType.Heal:
                if (playerHealth != null)
                {
                    playerHealth.Heal(currentItem.value);
                    Debug.Log("Heal used: " + currentItem.value);
                }
                break;

            case ItemType.DamageBoost:
                PlayerAttack attack = inventoryUI.player.GetComponent<PlayerAttack>();
                if (attack != null)
                {
                    attack.BoostDamage(currentItem.value, 10f);
                    Debug.Log(" Damage Boost activated!");
                }
                break;
        }

        inventoryUI.inventory.RemoveItem(slotIndex);
        inventoryUI.UpdateUI();
        inventoryUI.SelectSlot(null);
    }

    void DropThisItem()
    {
        if (inventoryUI != null)
        {
            inventoryUI.DropItem(slotIndex);
        }
    }
}