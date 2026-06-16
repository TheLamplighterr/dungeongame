using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attackRange = 3f;
    public int damage = 25;
    public LayerMask enemyLayer;
    public float attackCooldown = 1f;

    private bool canAttack = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && canAttack)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;

        PerformAttack();

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    void PerformAttack()
{
    Debug.Log("[PlayerAttack] Attack triggered");

    // ALLE Collider in der Range suchen
    Collider[] allHits = Physics.OverlapSphere(
        transform.position,
        attackRange
    );

    Debug.Log("[PlayerAttack] Total colliders found: " + allHits.Length);

    foreach (Collider hit in allHits)
    {
        Debug.Log(
            "[PlayerAttack] Found Collider: " +
            hit.name +
            " | Layer: " +
            LayerMask.NameToLayer(LayerMask.LayerToName(hit.gameObject.layer))
        );

        EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            Debug.Log("[PlayerAttack] ENEMY FOUND: " + enemy.name);

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