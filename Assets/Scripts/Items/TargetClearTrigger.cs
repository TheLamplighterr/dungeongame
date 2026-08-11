using System.Collections.Generic;
using UnityEngine;

public class TargetClearTrigger : MonoBehaviour
{
    [Header("Challenge Targets")]
    [SerializeField] private List<DestroyableTarget> targets = new List<DestroyableTarget>();

    [Header("Reward Object")]
    [SerializeField] private GameObject rewardChest;

    [Header("Spawn FX")]
    [Tooltip("Partikeleffekt / Magie-Effekt, der beim Erscheinen der Truhe erzeugt wird")]
    [SerializeField] private GameObject chestSpawnVFX;
    [Tooltip("Dauer in Sekunden, bis der Spawneffekt automatisch gelöscht wird")]
    [SerializeField] private float vfxDestroyDelay = 3.0f;

    private int remainingTargets;

    private void Start()
    {
        // DIAGNOSE 1: Ist die Truhe zugewiesen?
        if (rewardChest == null)
        {
            Debug.LogError($"[DIAGNOSE] FEHLER auf {gameObject.name}: Das Feld 'Reward Chest' ist LEER! Bitte im Inspector zuweisen.");
            return;
        }

        // DIAGNOSE 2: Wurden Ziele zugewiesen?
        if (targets == null || targets.Count == 0)
        {
            Debug.LogError($"[DIAGNOSE] FEHLER auf {gameObject.name}: Die Liste 'Targets' ist LEER! Keine Ziele im Inspector zugewiesen.");
            return;
        }

        remainingTargets = targets.Count;
        Debug.Log($"<color=cyan>[DIAGNOSE] Trigger gestartet! {remainingTargets} Targets gefunden. Truhe wird versteckt.</color>");

        // Truhe zu Spielbeginn deaktivieren
        rewardChest.SetActive(false);

        // Bei allen Targets für das Event registrieren
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.OnTargetDestroyedAction += HandleTargetDestroyed;
            }
            else
            {
                Debug.LogWarning($"[DIAGNOSE] Ein Element in der Targets-Liste von {gameObject.name} ist NULL (Fehlt in der Hierarchy)!");
            }
        }
    }

    private void HandleTargetDestroyed(DestroyableTarget destroyedTarget)
    {
        if (destroyedTarget != null)
        {
            destroyedTarget.OnTargetDestroyedAction -= HandleTargetDestroyed;
        }

        remainingTargets--;
        Debug.Log($"<color=yellow>[DIAGNOSE] Target zerstört! Noch übrig: {remainingTargets}</color>");

        if (remainingTargets <= 0)
        {
            CompleteChallenge();
        }
    }

    private void CompleteChallenge()
    {
        Debug.Log("<color=green>[DIAGNOSE] ERFOLG! Alle Targets zerstört! Truhe wird JETZT aktiviert.</color>");

        if (rewardChest != null)
        {
            rewardChest.SetActive(true);

            // Spawneffekt erzeugen, falls zugewiesen
            if (chestSpawnVFX != null)
            {
                GameObject spawnedVFX = Instantiate(chestSpawnVFX, rewardChest.transform.position, Quaternion.identity);
                
                // Effekttag nach Ablauf der eingestellten Zeit aufräumen
                Destroy(spawnedVFX, vfxDestroyDelay);
            }
        }
    }
}