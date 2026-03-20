using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public List<GameObject> meleeEnemyPrefabs;
    public List<GameObject> rangedEnemyPrefabs;
    public Transform[] globalPatrolPoints;

    [Header("Initial Settings")]
    public float initialGracePeriod = 5f;
    public int initialPassiveCount = 5;

    [Header("Wave Settings")]
    public float spawnRadius = 10f;
    public int maxSpawnAttempts = 10;
    public float timeBetweenWaves = 30f;
    public int baseWaveSize = 10;

    [Header("Difficulty Scaling")]
    public float spawnRateIncreaseMultiplier = 1.2f;
    [Range(0f, 1f)]
    public float initialMeleeRatio = 0.7f;
    public float meleeRatioDecreaseRate = 0.05f;

    [Header("Spawn Limits & Debug Info")]
    public int maxActiveEnemies = 50;  
    [SerializeField] private int totalEnemiesSpawned = 0;
    [SerializeField] private List<EnemyGrunt> activeEnemies = new List<EnemyGrunt>();

    private bool isHordeModeActive = false;
    private float currentMeleeRatio;
    private int waveCounter = 0;
    private float currentWaveTimer;

    public Transform playerTransform;

    public Action OnEnemyKilled;

    private void Start()
    {
        if (meleeEnemyPrefabs == null || meleeEnemyPrefabs.Count == 0)
        {
            Debug.LogError("No melee enemy prefabs assigned!");
            enabled = false;
            return;
        }

        if (rangedEnemyPrefabs == null || rangedEnemyPrefabs.Count == 0)
        {
            Debug.LogError("No ranged enemy prefabs assigned!");
            enabled = false;
            return;
        }

        currentMeleeRatio = initialMeleeRatio;
        StartCoroutine(InitialPhaseSequence());
    }

    private IEnumerator InitialPhaseSequence()
    {
        yield return new WaitForSeconds(initialGracePeriod);
        SpawnPassiveWave();
        currentWaveTimer = timeBetweenWaves;
    }

    private void Update()
    {
        if (isHordeModeActive)
        {
            HandleHordeMode();
            return;
        }

        currentWaveTimer -= Time.deltaTime;
        if (currentWaveTimer <= 0)
        {
            SpawnWave();
            currentWaveTimer = timeBetweenWaves;
        }
    }

    private void SpawnPassiveWave()
    {
        for (int i = 0; i < initialPassiveCount; i++)
        {
            SpawnEnemy(true);
        }
    }

    private void SpawnWave()
    {
        if (activeEnemies.Count >= maxActiveEnemies) return;

        int waveSize = Mathf.RoundToInt(baseWaveSize * Mathf.Pow(spawnRateIncreaseMultiplier, waveCounter));
        int meleeCount = Mathf.RoundToInt(waveSize * currentMeleeRatio);
        int rangedCount = waveSize - meleeCount;

        for (int i = 0; i < meleeCount; i++)
            SpawnEnemy(false, true);

        for (int i = 0; i < rangedCount; i++)
            SpawnEnemy(false, false);

        currentMeleeRatio = Mathf.Max(0.3f, currentMeleeRatio - meleeRatioDecreaseRate);
        waveCounter++;
    }

    private void SpawnEnemy(bool isPassive, bool isMelee = true)
    {
        if (activeEnemies.Count >= maxActiveEnemies) return;

        List<GameObject> prefabList = isMelee ? meleeEnemyPrefabs : rangedEnemyPrefabs;
        if (prefabList == null || prefabList.Count == 0)
        {
            Debug.LogError($"No {(isMelee ? "melee" : "ranged")} enemy prefabs assigned to spawn!");
            return;
        }

        Vector3 spawnPoint = GetRandomSpawnPoint();
        Vector3 validSpawnPoint;

        if (FindValidNavMeshPosition(spawnPoint, out validSpawnPoint))
        {
            GameObject prefab = prefabList[Random.Range(0, prefabList.Count)];
            GameObject enemy = Instantiate(prefab, validSpawnPoint, Quaternion.identity, transform);

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(validSpawnPoint);
            else enemy.transform.position = validSpawnPoint;

            EnemyGrunt enemyGrunt = enemy.GetComponent<EnemyGrunt>();
            if (enemyGrunt != null)
            {
                enemyGrunt.AssignPatrolPoints(globalPatrolPoints);
                enemyGrunt.InitializeBehavior();
                
                activeEnemies.Add(enemyGrunt);

                if (isHordeModeActive)
                {
                    enemyGrunt.SetDirectChaseMode(playerTransform);
                }
            }
        }
    }

    private void HandleHordeMode()
    {
        if (Random.value < Time.deltaTime * 4 && activeEnemies.Count < maxActiveEnemies)
        {
            SpawnEnemy(false, Random.value < currentMeleeRatio);
        }
    }

    private bool FindValidNavMeshPosition(Vector3 origin, out Vector3 validPosition)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0f;
            Vector3 testPosition = origin + randomOffset;

            if (NavMesh.SamplePosition(testPosition, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            {
                validPosition = hit.position;
                return true;
            }
        }
        validPosition = Vector3.zero;
        return false;
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (isHordeModeActive && playerTransform != null)
        {
            Vector3 playerPosition = playerTransform.position;
            Vector3 spawnOffset = Random.onUnitSphere * (spawnRadius * 0.6f);
            spawnOffset.y = 0f;

            return playerPosition + spawnOffset;  // Spread enemies out better
        }

        if (globalPatrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points assigned!");
            return Vector3.zero;
        }

        return globalPatrolPoints[Random.Range(0, globalPatrolPoints.Length)].position;
    }


    public void OnEnemyDefeated(EnemyGrunt enemy)
    {
        OnEnemyKilled?.Invoke();
    }

    public void OnEnemyDeactivated(EnemyGrunt enemy)
    {
        activeEnemies.Remove(enemy);
    }

    private Coroutine _hordeModeRoutine = null;    
    public void ActivateHordeMode()
    {
        isHordeModeActive = true;
        OverridePatrolPoints();

        if (_hordeModeRoutine != null)
            StopCoroutine(_hordeModeRoutine);
        
        _hordeModeRoutine = StartCoroutine(HordeModeTimer());
    }

    private IEnumerator HordeModeTimer()
    {
        while (CampfireManager.Instance.enemiesKilled < CampfireManager.Instance.enemiesToKill)
        {
            yield return null;
        }
        
        isHordeModeActive = false;
    }

    private void OverridePatrolPoints()
    {
        foreach (EnemyGrunt enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.SetDirectChaseMode(playerTransform);
            }
        }
    }
}
