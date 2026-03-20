using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class EnemyGrunt : Enemy
{
    [System.Serializable]
    public struct GruntStats
    {
        public int hp;
        public float speed;
        public int attackDamage;
        public float scale;
        public int experienceGain;
    }

    [Header("Version Stats")]
    public GruntStats[] versionStats = new GruntStats[4];
    protected int selectedVersion;

    [Header("Patrol Settings")]
    protected Transform[] patrolPoints;
    protected int currentPatrolIndex;

    private bool isDirectChaseMode = false;
    private Transform chaseTarget;

    protected override void Start()
    {
        SelectVersion();
        ApplyVersionStats();
        base.Start();
    }

    private void SelectVersion()
    {
        selectedVersion = Random.Range(0, versionStats.Length);
    }

    private void ApplyVersionStats()
    {
        if (versionStats == null || selectedVersion >= versionStats.Length) return;
        
        var stats = versionStats[selectedVersion];
        MinStartingHealth = stats.hp;
        MaxStartingHealth = stats.hp;
        attackDamage = stats.attackDamage;
        ExperienceGainedOnKill = stats.experienceGain;
        
        if (agent != null)
            agent.speed = stats.speed;
            
        transform.localScale = Vector3.one * stats.scale;
    }

    public void AssignPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

    public void InitializeBehavior()
    {
        if (!agent) return;

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[Random.Range(0, patrolPoints.Length)].position);
        }
    }

    public void SetDirectChaseMode(Transform target)
    {
        isDirectChaseMode = true;
        chaseTarget = target;
    }

    private void Update()
    {
        if (isDirectChaseMode && chaseTarget != null)
        {
            agent.SetDestination(chaseTarget.position);
        }
    }
}