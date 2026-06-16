using UnityEngine;

public class EnemyReaction : MonoBehaviour
{
    public Animator animator;
    public float knockbackForce = 2f;

    void Awake()
    {
        animator = GetComponent<Animator>();

        EnemyHealth hp = GetComponent<EnemyHealth>();
        if (hp != null)
            hp.OnHit += React;
    }

    void React()
    {
        // Animation reaction (universal fallback)

        //Falls es bei Gegnern Reaktions-Animationen auf einen Angriff gibt 
        //bei slime gerade nicht der fall
        if (animator != null)
        {
            animator.Play("Hit", 0, 0f);
        }

        // optional: small shake / scale punch later
        transform.localScale *= 1.05f;
        Invoke(nameof(ResetScale), 0.1f);
    }

    void ResetScale()
    {
        transform.localScale = Vector3.one;
    }
}