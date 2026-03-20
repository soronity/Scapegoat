using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Serialization;

public abstract class Enemy : MonoBehaviour
{
    [Header("Basic Stats")]
    public float aggroRange = 3f;
    public float attackRange = 2f;
    public float stoppingDistance = 1.5f;
    public float attackRate = 1.5f;
    public int attackDamage = 10;

    [Header("Health Settings")]
    public int MinStartingHealth = 15;
    public int MaxStartingHealth = 65;
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    [Header("Experience Gain")]
    public int ExperienceGainedOnKill = 50;

    [Header("Visual Effects")]
    public float DamageShakeDuration = 0.35f;

    [Header("Death Settings")]
    public float deathAnimationDuration = 2f;      // Initial death animation time
    public float fadeOutDuration = 60f;            // How long to fade into ground
    public float sinkDistance = 2f;                // How far to sink into ground
    public bool useAnimatorLength = true;          // Use animation length for initial death

    private static GameObject playerObject; // Cache player reference
    protected Transform PlayerTransform
    {
        get
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject == null) return null;
            }
            return playerObject.transform;
        }
    }

    protected Vector3 PlayerPosition
    {
        get
        {
            if (PlayerTransform == null) return transform.position;
            return PlayerTransform.position;
        }
    }

    protected NavMeshAgent agent;
    protected Animator animator;
    protected float timeSinceLastAttack;
    protected Collider enemyCollider;
    
    protected static readonly int IsWalking = Animator.StringToHash("isWalking");
    protected static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    protected static readonly int IsTakingDamage = Animator.StringToHash("isTakingDamage");
    protected static readonly int IsDead = Animator.StringToHash("isDead");

    private Vector3 originPosition;
    private Vector3 deathPosition;
    private Material[] allMaterials;
    private float[] originalAlpha;
    private bool isDying = false;

    protected enum EnemyState { Patrolling, Chasing, Attacking, Dying }
    protected EnemyState currentState;

    private bool isHordeMode = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider>();

        // Get all renderers and their materials
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        allMaterials = new Material[renderers.Length];
        originalAlpha = new float[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            allMaterials[i] = renderers[i].material;
            // Store original alpha values
            if (allMaterials[i].HasProperty("_Color"))
            {
                originalAlpha[i] = allMaterials[i].color.a;
            }
        }
    }

    protected virtual void Start()
    {
        float randomValue = Random.Range(0f, 1f);
        MaxHealth = Mathf.RoundToInt(Mathf.Lerp(MinStartingHealth, MaxStartingHealth, randomValue));
        CurrentHealth = MaxHealth;
        timeSinceLastAttack = 0f;

        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.updateUpAxis = false;
            agent.autoBraking = false;
            agent.acceleration = 12f;
            agent.angularSpeed = 120f;
        }

        SetState(EnemyState.Patrolling);
    }

    public void ActivateHordeMode()
    {
        isHordeMode = true;
        SetState(EnemyState.Chasing);
    }

    protected virtual void Update()
    {
        if (CurrentHealth <= 0 || !agent || !agent.isOnNavMesh || isDying) return;

        timeSinceLastAttack += Time.deltaTime;

        if (currentState == EnemyState.Chasing)
        {
            Chase();
            return;
        }

        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (isHordeMode)
                    SetState(EnemyState.Chasing);
                else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    Patrol();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
        }
    }

    protected void SetState(EnemyState newState)
    {
        if (!agent) return;
        
        currentState = newState;
        switch (newState)
        {
            case EnemyState.Patrolling:
                if (animator != null)
                {
                    animator.SetBool(IsWalking, true);
                    animator.SetBool(IsAttacking, false);
                }
                agent.isStopped = false;
                agent.ResetPath();
                Patrol();
                break;

            case EnemyState.Chasing:
                if (animator != null)
                {
                    animator.SetBool(IsWalking, true);
                    animator.SetBool(IsAttacking, false);
                }
                agent.isStopped = false;
                agent.ResetPath();
                Chase();
                break;

            case EnemyState.Attacking:
                if (animator != null)
                {
                    animator.SetBool(IsWalking, false);
                }
                agent.isStopped = true;
                break;

            case EnemyState.Dying:
                if (animator != null)
                {
                    animator.SetBool(IsWalking, false);
                    animator.SetBool(IsAttacking, false);
                }
                agent.isStopped = true;
                break;
        }
    }
    
    public void Hit(float damage)
    {
        if (isDying) return; // Prevent multiple hits during death

        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            SetState(EnemyState.Dying);
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger(IsTakingDamage);
        }
    }

    private void Die()
    {
        if (isDying) return;
        isDying = true;
        
        if (animator != null)
        {
            animator.SetTrigger(IsDead);
        }
        
        // Disable components but keep object active
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        // Store death position
        deathPosition = transform.position;
        
        // Start death sequence
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Wait for initial death animation
        float initialWaitTime = useAnimatorLength && animator != null ? 
            animator.GetCurrentAnimatorStateInfo(0).length : 
            deathAnimationDuration;
            
        // Notify spawner before deactivating
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyDefeated(this as EnemyGrunt);
        }
        
        yield return new WaitForSeconds(initialWaitTime);

        // Start fading and sinking
        float elapsedTime = 0f;
        Vector3 startPos = deathPosition;
        Vector3 endPos = startPos - new Vector3(0, sinkDistance, 0);

        // Gradually fade out and sink
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeOutDuration;

            // Update position - sink into ground
            transform.position = Vector3.Lerp(startPos, endPos, progress);

            // Update material opacity
            for (int i = 0; i < allMaterials.Length; i++)
            {
                if (allMaterials[i] != null && allMaterials[i].HasProperty("_Color"))
                {
                    Color currentColor = allMaterials[i].color;
                    float newAlpha = Mathf.Lerp(originalAlpha[i], 0f, progress);
                    allMaterials[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
                }
            }

            if (spawner != null)
            {
                spawner.OnEnemyDeactivated(this as EnemyGrunt);
            }
            
            yield return null;
        }

        // Reset materials before deactivating
        for (int i = 0; i < allMaterials.Length; i++)
        {
            if (allMaterials[i] != null && allMaterials[i].HasProperty("_Color"))
            {
                Color currentColor = allMaterials[i].color;
                allMaterials[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, originalAlpha[i]);
            }
        }

        // Finally deactivate
        gameObject.SetActive(false);
    }

    protected virtual void Patrol() { }
    protected virtual void Chase() { }
    protected virtual void Attack() { }
}