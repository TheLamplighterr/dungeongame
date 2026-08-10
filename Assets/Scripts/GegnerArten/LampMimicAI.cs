using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class LampMimicAI : MonoBehaviour
{
    public enum MimicState { Dormant, Awakening, Chasing, Attacking }

    [Header("Zustand")]
    public MimicState currentState = MimicState.Dormant;

    [Header("Objekt-Transformation (Visuelles)")]
    [Tooltip("Das normale Lampen-Mesh/Objekt, das im schlafenden Zustand sichtbar ist")]
    public GameObject normalLampModel;
    
    [Tooltip("Das Monster-Lampen-Mesh/Objekt, das beim Erwachen sichtbar wird")]
    public GameObject monsterLampModel;

    [Header("Reichweiten & Zeiten")]
    [Tooltip("Ab welcher Distanz zum Spieler wacht die Lampe auf?")]
    public float activationRange = 5.0f;

    [Tooltip("Dauer der Erwach-Animation / Pause vor der Jagd in Sekunden")]
    public float awakenDuration = 1.5f;

    [Tooltip("Ab welcher Distanz greift das Monster an?")]
    public float attackRange = 1.8f;

    [Tooltip("Pause zwischen zwei Angriffen in Sekunden")]
    public float attackCooldown = 2.0f;

    [Header("Bewegung")]
    public float chaseSpeed = 3.5f;

    [Header("Audio & VFX (Optional)")]
    [Tooltip("Sound beim Erwachen (Schrei/Knurren)")]
    public AudioClip awakenSFX;

    [Tooltip("Sound beim Angriff")]
    public AudioClip attackSFX;

    [Tooltip("VFX-Partikel beim Erwachen (z.B. Staub/Rauch)")]
    public GameObject awakenVFXPrefab;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private Animator monsterAnimator;
    private AudioSource audioSource;
    private bool canAttack = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Spieler in der Szene suchen (per Tag)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Animator im Monster-Modell suchen (falls vorhanden)
        if (monsterLampModel != null)
        {
            monsterAnimator = monsterLampModel.GetComponentInChildren<Animator>();
        }

        // Zu Beginn in den Schlafzustand versetzen
        SetDormantState();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case MimicState.Dormant:
                // Prüfen, ob der Spieler nahe genug herangekommen ist
                if (distanceToPlayer <= activationRange)
                {
                    StartCoroutine(AwakenSequence());
                }
                break;

            case MimicState.Awakening:
                // Während des Erwachens macht die Logik eine Pause (wird von der Coroutine gesteuert)
                break;

            case MimicState.Chasing:
                HandleChasing(distanceToPlayer);
                break;

            case MimicState.Attacking:
                HandleAttacking(distanceToPlayer);
                break;
        }
    }

    private void SetDormantState()
    {
        currentState = MimicState.Dormant;

        // Normales Lampenmodell an, Monster-Modell aus
        if (normalLampModel != null) normalLampModel.SetActive(true);
        if (monsterLampModel != null) monsterLampModel.SetActive(false);

        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = chaseSpeed;
        }
    }

    private IEnumerator AwakenSequence()
    {
        currentState = MimicState.Awakening;

        // 1. Modelle austauschen
        if (normalLampModel != null) normalLampModel.SetActive(false);
        if (monsterLampModel != null) monsterLampModel.SetActive(true);

        // 2. Sound & VFX abspielen
        if (awakenSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(awakenSFX);
        }

        if (awakenVFXPrefab != null)
        {
            GameObject vfx = Instantiate(awakenVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3.0f);
        }

        // 3. Animation 'Awaken' oder 'Wake' triggern (falls vorhanden)
        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger("Awaken");
        }

        // Kurz warten (z.B. für die Erwach-Animation)
        yield return new WaitForSeconds(awakenDuration);

        // 4. Jagd starten
        if (agent != null)
        {
            agent.isStopped = false;
        }

        currentState = MimicState.Chasing;
    }

    private void HandleChasing(float distanceToPlayer)
    {
        // NavMeshAgent zum Spieler schicken
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }

        // Animation für Laufen aktivieren
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool("IsWalking", true);
        }

        // Ist der Spieler in Angriffsreichweite?
        if (distanceToPlayer <= attackRange)
        {
            currentState = MimicState.Attacking;
        }
    }

    private void HandleAttacking(float distanceToPlayer)
    {
        // Beim Angreifen anhalten und zum Spieler ausrichten
        if (agent != null)
        {
            agent.isStopped = true;
        }

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // Nicht nach oben/unten kippen
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);
        }

        // Angriff ausführen, wenn Cooldown abgelaufen ist
        if (canAttack)
        {
            StartCoroutine(PerformAttack());
        }

        // Wenn der Spieler wieder wegläuft, zurück in den Jagdmodus
        if (distanceToPlayer > attackRange)
        {
            currentState = MimicState.Chasing;
        }
    }

    private IEnumerator PerformAttack()
    {
        canAttack = false;

        // Angriffssound
        if (attackSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSFX);
        }

        // Angriffsanimation
        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger("Attack");
        }

        Debug.Log($"<color=red>[LampMimic]</color> Greift den Spieler an!");

        // Hier kann später z.B. dem Spieler Leben abgezogen werden:
        // PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
        // if (health != null) health.TakeDamage(10);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    // Zeigt die Reichweiten als bunte Kreise im Scene-Fenster an (zum einfachen Einstellen)
    private void OnDrawGizmosSelected()
    {
        // Grüner Kreis = Erwach-Reichweite
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Roter Kreis = Angriffs-Reichweite
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}