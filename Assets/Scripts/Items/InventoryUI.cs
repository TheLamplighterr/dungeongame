using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public InventorySlotUI slotPrefab;
    public Transform slotParent;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public PlayerHealth playerHealth;

    [Header("UI")]
    public GameObject inventoryPanel;

    [Header("State")]
    private bool isOpen = false;
    public bool IsOpen => isOpen;
    public bool JustClosedInventory { get; private set; }

    private InventorySlotUI[] slots;

    [Header("Drop")]
    public Transform dropPoint;
    public float dropHeightOffset = 0.1f;

    public Transform player;

    void Start()
    {
        CreateUI();
        UpdateUI();

        inventoryPanel.SetActive(false);
    }

    void Update()
    {
    
        UpdateUI();
    }

    void OpenInventory()
    {
        isOpen = true;
        inventoryPanel.SetActive(true);

        // PLAYER LOCK
        playerMovement.canMove = false;
        playerAttack.DisableCombat();

        // GAME PAUSE (optional aber stabil)
        Time.timeScale = 0f;

        // CURSOR
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void CloseInventory()
    {
        isOpen = false;
        inventoryPanel.SetActive(false);

        // PLAYER UNLOCK
        playerMovement.canMove = true;
        playerAttack.EnableCombat();

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void CreateUI()
    {
        slots = new InventorySlotUI[inventory.inventorySize];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = Instantiate(slotPrefab, slotParent);
            slots[i].slotIndex = i;
            slots[i].inventoryUI = this;
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetItem(inventory.items[i]);
        }
    }

    public void RemoveItem(int index)
    {
        inventory.items[index] = null;
        UpdateUI();
    }


public LayerMask groundMask;

    public void DropItem(int index)
{
    if (inventory.items[index] == null)
        return;

    ItemData item = inventory.items[index];

    if (item.worldPrefab == null)
    {
        Debug.LogWarning("Kein WorldPrefab für: " + item.itemName);
        return;
    }

    if (dropPoint == null)
    {
        Debug.LogError("DropPoint nicht gesetzt!");
        return;
    }

    //  Spawn Position leicht über Boden
    Vector3 spawnPos = dropPoint.position + Vector3.up * dropHeightOffset;

    Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);

    inventory.RemoveItem(index);
    UpdateUI();
}

public void Open()
{
    if (!isOpen)
        OpenInventory();
}

public void Close()
{
    if (isOpen)
        CloseInventory();
}

}