using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public InventoryUI inventoryUI;
    public PauseMenuUI pauseMenu;

    void Update()
    {
        // =========================
        // INVENTAR (I)
        // =========================
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryUI.IsOpen)
                inventoryUI.Close();
            else
                inventoryUI.Open();

            return;
        }

        // =========================
        // ESC
        // =========================
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Inventar schließen
            if (inventoryUI.IsOpen)
            {
                inventoryUI.Close();
                return;
            }

            // Pause schließen
            if (pauseMenu.IsPaused)
            {
                pauseMenu.ClosePause();
                return;
            }

            // Pause öffnen
            pauseMenu.OpenPause();
        }
    }
}