using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private int damage;
    private float damageRadius;
    private bool hasHit = false;

    public void Initialize(Vector3 startPos, Vector3 targetPos, float projectileSpeed, int projectileDamage, float radius)
    {
        transform.position = startPos;
        targetPosition = targetPos;
        speed = projectileSpeed;
        damage = projectileDamage;
        damageRadius = radius;
    }

    private void Update()
    {
        if (hasHit) return;

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Impact();
        }
    }

    private void Impact()
    {
        hasHit = true;

        // Area damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (var hitCollider in hitColliders)
        {
            m_PlayerStats playerHealth = hitCollider.GetComponent<m_PlayerStats>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        // VFX and cleanup
        // TODO: Add impact effects
        Destroy(gameObject, 0.1f);
    }
}