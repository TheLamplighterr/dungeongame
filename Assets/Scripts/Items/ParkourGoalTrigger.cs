using UnityEngine;

public class ParkourGoalTrigger : MonoBehaviour
{
    [Header("Reward Object")]
    [Tooltip("Die Truhe im Raum, die nach dem Parkour aktiviert wird.")]
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

    private void OnTriggerEnter(Collider other)
    {
        if (!challengeCompleted && other.CompareTag("Player"))
        {
            CompleteChallenge();
        }
    }

    private void CompleteChallenge()
    {
        challengeCompleted = true;
        Debug.Log("[Parkour] Ziel erreicht! Truhe wird freigeschaltet.");

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