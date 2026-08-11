using System.Collections.Generic;
using UnityEngine;

public class EnemyClearTrigger : MonoBehaviour
{
    [Header("Enemies")]
    [Tooltip("Ziehe hier alle Gegner des Raumes hinein.")]
    [SerializeField] private List<GameObject> enemies = new List<GameObject>();

    [Header("Reward Object")]
    [Tooltip("Die Truhe im Raum, die anfangs deaktiviert ist.")]
    [SerializeField] private GameObject rewardChest;

    [Header("Optional FX")]
    [SerializeField] private GameObject chestSpawnVFX;

    private bool challengeCompleted = false;

    private void Start()
    {
        // Truhe zu Spielbeginn verstecken
        if (rewardChest != null)
        {
            rewardChest.SetActive(false);
        }
    }

    private void Update()
    {
        if (challengeCompleted) return;

        // Entferne alle bereits zerstörten Gegner (null-Einträge) aus der Liste
        enemies.RemoveAll(enemy => enemy == null);

        // Wenn die Liste leer ist, sind alle Gegner tot
        if (enemies.Count == 0)
        {
            CompleteChallenge();
        }
    }

    private void CompleteChallenge()
    {
        challengeCompleted = true;
        Debug.Log("[Enemy Challenge] Alle Gegner besiegt! Truhe wird freigeschaltet.");

        if (rewardChest != null)
        {
            rewardChest.SetActive(true);

            if (chestSpawnVFX != null)
            {
                Instantiate(chestSpawnVFX, rewardChest.transform.position, Quaternion.identity);
            }
        }
    }
}