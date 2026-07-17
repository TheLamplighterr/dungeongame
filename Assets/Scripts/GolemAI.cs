using UnityEngine;
using System.Collections;

public class StoneGolemAI : BaseEnemyAI
{
    public Animator animator;

    private EnemyDamage enemyDamage;
    private EnemyHealth enemyHealth; // NEU: Referenz auf die eigene Lebenskomponente

    [Header("Boss Settings")]
    public float moveSpeed = 2f;
    public float attackHitTime = 1.0f;
    public float attackCooldown = 1.5f;

    [Header("Impact")]
    public GameObject impactEffect;
    public Transform impactPoint;

    private bool isAttacking;
    private bool hasTriggeredBossUI = false; // NEU: Verhindert mehrfaches Triggern

    // Speichert die aktuell laufende Animation, um Spamming zu verhindern
    private string currentAnimation = "";

    protected override void Awake()
    {
        base.Awake();

        // Sucht den Animator im Kind-Objekt (dem Golem-Modell)
        animator = GetComponentInChildren<Animator>();
        enemyDamage = GetComponent<EnemyDamage>();
        enemyHealth = GetComponent<EnemyHealth>(); // NEU: Holt sich das EnemyHealth-Skript

        if (agent != null)
            agent.speed = moveSpeed;
    }

    // Wir klinken uns in Update ein, um die UI zu aktivieren, sobald der Kampf losgeht
    protected override void Update()
    {
        base.Update();

        // NEU: Sobald der Golem den Spieler sieht (nicht mehr im Idle ist), 
        // aktivieren wir die Boss-UI auf dem Bildschirm!
        if (!hasTriggeredBossUI && player != null && agent != null && agent.enabled)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= sightRange)
            {
                TriggerBossFightUI();
            }
        }
    }

    // NEU: Aktiviert die Boss-UI über das BossUI-Singleton
    private void TriggerBossFightUI()
    {
        if (BossUI.Instance != null && enemyHealth != null)
        {
            BossUI.Instance.StartBossFight(enemyHealth);
            hasTriggeredBossUI = true;
            Debug.Log($"[BOSS-AI] Bosskampf-UI für {gameObject.name} erfolgreich gestartet!");
        }
    }

    protected override void Idle()
    {
        base.Idle();

        if (isAttacking || animator == null)
            return;

        // Spielt CrossFade nur ab, wenn nicht schon im Idle-Zustand 
        if (currentAnimation != "Idle")
        {
            currentAnimation = "Idle";
            animator.CrossFade("Golem_Idle", 0.1f);
        }
    }

    protected override void Chase()
    {
        base.Chase();

        if (isAttacking || animator == null)
            return;

        // Spielt CrossFade nur ab, wenn wir nicht schon im Laufen-Zustand sind
        if (currentAnimation != "Walk")
        {
            currentAnimation = "Walk";
            animator.CrossFade("Golem_Walk", 0.15f);
        }
    }

    protected override void Attack()
    {
        base.Attack();

        if (isAttacking)
            return;

        // Setzt den String auf Attack, damit Idle/Walk während der Coroutine blockiert werden
        currentAnimation = "Attack";
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAct = false;

        agent.isStopped = true;
        agent.ResetPath();

        if (animator != null)
            animator.Play("Golem_Attack", 0, 0f);

        // Zeitpunkt, an dem der Schlag treffen soll
        yield return new WaitForSeconds(attackHitTime);

        SpawnImpact();

        if (enemyDamage != null)
            enemyDamage.DealDamage();

        // Rest der Animation abwarten
        yield return new WaitForSeconds(attackCooldown);

        if (animator != null)
        {
            currentAnimation = "Idle"; // Setzt den Zustand zurück auf Idle
            animator.Play("Golem_Idle", 0, 0f);
        }

        agent.isStopped = false;

        isAttacking = false;
        canAct = true;
    }

    void SpawnImpact()
    {
        if (impactEffect == null)
            return;

        Vector3 spawnPos = impactPoint != null ? impactPoint.position : transform.position;

        Instantiate(
            impactEffect,
            spawnPos,
            Quaternion.identity
        );
    }
}