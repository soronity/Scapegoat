using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RangedEnemy : EnemyGrunt
{
    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 10f;
    public float areaDamageRadius = 3f;
    public GameObject attackIndicatorPrefab;
    public float minAttackDistance = 5f;
    public float preferredAttackDistance = 8f;

    private GameObject currentIndicator;
    private Vector3 targetPosition;

    protected override void Update()
    {
        base.Update();
        if (CurrentHealth <= 0 || !agent) return;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerPosition);

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distanceToPlayer <= aggroRange)
                {
                    SetState(EnemyState.Chasing);
                }
                break;

            case EnemyState.Chasing:
                if (distanceToPlayer <= aggroRange && distanceToPlayer > minAttackDistance)
                {
                    SetState(EnemyState.Attacking);
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer < minAttackDistance)
                {
                    Vector3 directionFromPlayer = transform.position - PlayerPosition;
                    Vector3 retreatPosition = transform.position + directionFromPlayer.normalized * 
                        (preferredAttackDistance - distanceToPlayer);
                    if (agent && agent.isOnNavMesh)
                    {
                        agent.SetDestination(retreatPosition);
                    }
                }
                else if (distanceToPlayer > aggroRange)
                {
                    SetState(EnemyState.Chasing);
                }
                break;
        }
    }

    protected override void Chase()
    {
        if (PlayerTransform == null || !agent || !agent.isOnNavMesh) return;

        Vector3 directionToPlayer = (PlayerPosition - transform.position).normalized;
        Vector3 targetPosition = PlayerPosition - directionToPlayer * preferredAttackDistance;
        
        agent.stoppingDistance = minAttackDistance;
        agent.SetDestination(targetPosition);
    }

    protected override void Attack()
    {
        if (PlayerTransform == null) return;

        transform.LookAt(new Vector3(PlayerPosition.x, transform.position.y, PlayerPosition.z));

        if (timeSinceLastAttack >= attackRate)
        {
            StartAttackSequence();
            timeSinceLastAttack = 0f;
        }
    }

    private void StartAttackSequence()
    {
        targetPosition = PlayerPosition;
        if (animator != null) animator.SetTrigger(IsAttacking);
        
        if (currentIndicator == null)
        {
            currentIndicator = Instantiate(attackIndicatorPrefab, targetPosition, Quaternion.identity);
            currentIndicator.transform.localScale = Vector3.one * areaDamageRadius * 2f;
        }
        else
        {
            currentIndicator.transform.position = targetPosition;
        }

        Invoke(nameof(LaunchProjectile), 0.5f);
    }

    private void LaunchProjectile()
    {
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
        }

        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            ProjectileController projectileScript = projectile.GetComponent<ProjectileController>();
            
            if (projectileScript != null)
            {
                projectileScript.Initialize(projectileSpawnPoint.position, targetPosition, projectileSpeed, 
                    attackDamage, areaDamageRadius);
            }
        }
    }
}