using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public InventorySlotUI slotPrefab;
    public Transform slotParent;

    [Header("Item Details")]
    public Image detailIconImage;
    public TMP_Text detailNameText;
    public TMP_Text detailDescriptionText;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public PlayerHealth playerHealth;
    public Transform player;

    [Header("UI Windows & Animation")]
    public GameObject inventoryPanel;
    public CanvasGroup canvasGroup; // Steuert die Transparenz
    public float animationDuration = 0.25f; // Dauer der Animation in Sekunden

    [Header("Gameplay UI zum Ausblenden")]
    [Tooltip("Ziehe hier alle UI-Elemente rein, die beim Öffnen verschwinden sollen (auch Prompts). Das Skript merkt sich, was vorher aktiv war!")]
    [SerializeField] private List<GameObject> gameplayUIElementsToHide = new List<GameObject>();
    
    // Merkt sich dynamisch, welche Gameplay-UIs VOR dem Öffnen wirklich aktiv waren
    private List<GameObject> previouslyActiveUIElements = new List<GameObject>();

    [Header("State")]
    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private InventorySlotUI[] slots;
    private Coroutine currentAnimation; // Verhindert Animations-Konflikte

    [Header("Drop")]
    public Transform dropPoint;
    public float dropHeightOffset = 0.1f;

    [Header("Equipped Slots Trackers")]
    public int equippedArmorSlotIndex = -1; // -1 bedeutet: Nichts ausgerüstet
    public int equippedDamageSlotIndex = -1;

    void Start()
    {
        CreateUI();
        
        // Sicherstellen, dass CanvasGroup zugewiesen ist
        if (canvasGroup == null && inventoryPanel != null)
        {
            canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        }

        // Zu Beginn unsichtbar machen & deaktivieren
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void OpenInventory()
    {
        if (isOpen) return;
        isOpen = true;

        inventoryPanel.SetActive(true);
        UpdateUI();
        SelectSlot(null);
        ToggleGameplayUI(false);

        if (playerMovement != null) playerMovement.canMove = false;
        if (playerAttack != null) playerAttack.DisableCombat();

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateInventory(true));
    }

    public void CloseInventory()
    {
        if (!isOpen) return;
        isOpen = false;

        ToggleGameplayUI(true);

        if (playerMovement != null) playerMovement.canMove = true;
        if (playerAttack != null) playerAttack.EnableCombat();

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(AnimateInventory(false));
    }

    private IEnumerator EnableCameraInputNextFrame(CinemachineInputAxisController controller)
    {
        yield return null; // Wartet 1 Frame, damit Unity den Cursor-Lock verarbeitet

        if (controller != null)
        {
            controller.enabled = true;
        }

        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.PreviousStateIsValid = false;
        }
    }

    private IEnumerator AnimateInventory(bool open)
    {
        float timer = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : (open ? 0f : 1f);
        float targetAlpha = open ? 1f : 0f;

        Vector3 startScale = open ? new Vector3(0.9f, 0.9f, 0.9f) : Vector3.one;
        Vector3 targetScale = open ? Vector3.one : new Vector3(0.9f, 0.9f, 0.9f);

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / animationDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, smoothProgress);
            }

            if (inventoryPanel != null)
            {
                inventoryPanel.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothProgress);
            }

            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = targetAlpha;
        if (inventoryPanel != null)
        {
            inventoryPanel.transform.localScale = targetScale;
            if (!open)
            {
                inventoryPanel.SetActive(false);
            }
        }
    }

    public void Open() => OpenInventory();
    public void Close() => CloseInventory();

    public void SelectSlot(ItemData item)
    {
        if (item == null)
        {
            if (detailIconImage) detailIconImage.enabled = false;
            if (detailNameText) detailNameText.text = "";
            if (detailDescriptionText) detailDescriptionText.text = "";
            return;
        }

        if (detailIconImage)
        {
            detailIconImage.enabled = true;
            detailIconImage.sprite = item.icon;
        }

        if (detailNameText) detailNameText.text = item.itemName;
        if (detailDescriptionText) detailDescriptionText.text = item.description;
    }

    // --- GAMEPLAY UI TOGGLE (SMART) ---
    private void ToggleGameplayUI(bool show)
    {
        if (!show)
        {
            // VOR DEM AUSBLENDEN: Merken, welche UI-Elemente wirklich aktiv waren
            previouslyActiveUIElements.Clear();

            foreach (GameObject uiElement in gameplayUIElementsToHide)
            {
                if (uiElement != null && uiElement.activeSelf)
                {
                    previouslyActiveUIElements.Add(uiElement);
                    uiElement.SetActive(false);
                }
            }
        }
        else
        {
            // BEIM EINBLENDEN: Nur die wieder anmachen, die vorher aktiv waren!
            foreach (GameObject uiElement in previouslyActiveUIElements)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(true);
                }
            }
            previouslyActiveUIElements.Clear();
        }
    }

    void CreateUI()
    {
        if (inventory == null || slotPrefab == null || slotParent == null) return;

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
        if (slots == null || inventory == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (inventory.items != null && i < inventory.items.Length)
            {
                slots[i].SetItem(inventory.items[i]);
            }
            else
            {
                slots[i].SetItem(null);
            }

            // Status für Ausrüstung setzen
            if (i == equippedArmorSlotIndex || i == equippedDamageSlotIndex)
            {
                slots[i].isEquipped = true;
            }
            else
            {
                slots[i].isEquipped = false;
            }

            slots[i].UpdateVisuals();
        }
    }

    public void DropItem(int index)
    {
        if (inventory == null || inventory.items == null) return;

        if (index < 0 || index >= inventory.items.Length || inventory.items[index] == null) return;

        ItemData item = inventory.items[index];

        if (item.worldPrefab == null)
        {
            Debug.LogWarning("Kein WorldPrefab für: " + item.itemName);
            return;
        }

        Vector3 spawnPos = (dropPoint != null) ? dropPoint.position : (player != null ? player.position : transform.position);
        spawnPos += Vector3.up * dropHeightOffset;

        Instantiate(item.worldPrefab, spawnPos, Quaternion.identity);

        inventory.PlayDropSound();
        inventory.RemoveItem(index);

        UpdateUI();
        SelectSlot(null);
    }

    public InventorySlotUI GetSlotByIndex(int index)
    {
        if (slots != null && index >= 0 && index < slots.Length)
        {
            return slots[index];
        }
        return null;
    }
}