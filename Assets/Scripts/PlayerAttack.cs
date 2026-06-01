using UnityEngine;

public class PlayerAttack: MonoBehaviour
{
    public float attackRange = 3f;
    public int damage = 25;
    public LayerMask enemyLayer;

    [Header("Attack Delay")]
    public float attackCooldown = 1f;

    private bool canAttack = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && canAttack)
        {
            StartCoroutine(Attack());
        }
    }

    System.Collections.IEnumerator Attack()
    {
        canAttack = false;

        PerformAttack();

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    void PerformAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}