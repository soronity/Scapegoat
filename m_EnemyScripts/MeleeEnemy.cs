using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MeleeEnemy : EnemyGrunt
{
    protected override void Update()
    {
        base.Update();
        if (CurrentHealth <= 0 || !agent) return;

        float sqrDistanceToPlayer = (PlayerPosition - transform.position).sqrMagnitude;

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (sqrDistanceToPlayer <= aggroRange * aggroRange)
                {
                    SetState(EnemyState.Chasing);
                }
                break;

            case EnemyState.Chasing:
                if (sqrDistanceToPlayer <= attackRange * attackRange)
                {
                    SetState(EnemyState.Attacking);
                }
                break;
        }
    }

    protected override void Chase()
    {
        if (PlayerTransform == null || !agent || !agent.isOnNavMesh) return;

        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(PlayerPosition);
    }

    protected override void Attack()
    {
        if (PlayerTransform == null) return;

        if (timeSinceLastAttack >= attackRate)
        {
            m_PlayerStats playerHealth = PlayerTransform.GetComponent<m_PlayerStats>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                if (animator != null) animator.SetTrigger(IsAttacking);
            }
            timeSinceLastAttack = 0f;
        }

        if ((PlayerPosition - transform.position).sqrMagnitude > attackRange * attackRange)
        {
            SetState(EnemyState.Chasing);
        }
    }
}
