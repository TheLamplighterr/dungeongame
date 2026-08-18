using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Das Item, das in dieser Truhe liegt")]
    public ItemData containedItem;

    [Header("Deckel-Steuerung")]
    [SerializeField] private Transform lidTransform;
    [SerializeField] private Vector3 openRotationAngle = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float openSpeed = 2.5f;

    [Header("Visuals & FX")]
    public GameObject openEffect;
    [SerializeField] private float effectDestroyDelay = 3.0f;

    private bool isOpened = false;
    private bool playerInRange = false;
    private GameObject currentPlayerObj; // Speichert Referenz zum Spieler

    private void Update()
    {
        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();

        if (playerInventory != null && containedItem != null)
        {
            bool addedSuccessfully = playerInventory.AddItem(containedItem);

            if (addedSuccessfully)
            {
                isOpened = true;

                // NEU: Interaktions-Animation auf dem Spieler auslösen
                if (currentPlayerObj != null)
                {
                    PlayerAnimationController anim = currentPlayerObj.GetComponent<PlayerAnimationController>();
                    if (anim == null)
                    {
                        anim = currentPlayerObj.GetComponentInChildren<PlayerAnimationController>();
                    }
                    if (anim != null)
                    {
                        anim.TriggerInteract();
                    }
                }

                // 1. UI Prompt verstecken & Pop-up anzeigen
                if (ChestInteractionUI.Instance != null)
                {
                    ChestInteractionUI.Instance.HidePrompt();
                    ChestInteractionUI.Instance.ShowItemNotification(containedItem.itemName);
                }

                // 2. Inventar-UI aktualisieren
                InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
                if (inventoryUI != null)
                {
                    inventoryUI.UpdateUI();
                }

                // 3. Deckel animieren
                if (lidTransform != null)
                {
                    StartCoroutine(OpenLidRoutine());
                }

                // 4. Partikeleffekt spawnen
                if (openEffect != null)
                {
                    GameObject spawnedEffect = Instantiate(openEffect, transform.position + Vector3.up, Quaternion.identity);
                    Destroy(spawnedEffect, effectDestroyDelay);
                }

                Debug.Log($"[Truhe] Geöffnet! {containedItem.itemName} wurde aufgenommen.");
            }
            else
            {
                // Zeigt die rote Warnung im UI-Banner an
                if (ChestInteractionUI.Instance != null)
                {
                    ChestInteractionUI.Instance.ShowWarningNotification("Inventory is full!");
                }

                Debug.LogWarning("[Truhe] Inventar ist voll! Kiste bleibt geschlossen.");
            }
        }
    }

    private IEnumerator OpenLidRoutine()
    {
        Quaternion startRotation = lidTransform.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(openRotationAngle);
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * openSpeed;
            lidTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            playerInRange = true;
            currentPlayerObj = other.gameObject; // Spieler-Referenz merken

            // UI Prompt anzeigen
            if (ChestInteractionUI.Instance != null)
            {
                ChestInteractionUI.Instance.ShowPrompt("Drücke [E] zum Öffnen");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayerObj = null;

            // UI Prompt verstecken
            if (ChestInteractionUI.Instance != null)
            {
                ChestInteractionUI.Instance.HidePrompt();
            }
        }
    }
}