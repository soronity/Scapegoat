using System;
using System.Collections;
using m_Programmers.p_Scripts.s_Jesper;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CampfireManager : MonoBehaviour
{
    public static CampfireManager Instance { get; private set; }

    public GameObject[] enemyList;
    public int[] maxEnemies;
    public EnemySpawner enemySpawner;

    [Header("Objective Settings")]
    public CampfireController[] campfireList;
    [SerializeField] private int requiredCampfires;
    private int extinguishedCampfires = 0;
    private int campfiresLit = 0;
    public int enemiesKilled = 0;
    public int enemiesToKill = 10;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI killsToProceedText;
    [SerializeField] private TextMeshProUGUI bigPromptText;

    [Header("Events")]
    public UnityEvent onAllCampfiresExtinguished;

    [Header("Portal Settings")]
    [SerializeField] private string portalIdToUnlock;  // Assign the correct portal ID in the inspector

    public Action OnEnemyKilled;
    
    private void OnEnable()
    {
        enemySpawner.OnEnemyKilled += EnemyKilled;
    }

    private void OnDisable()
    {
        enemySpawner.OnEnemyKilled -= EnemyKilled;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        campfireList = FindObjectsOfType<CampfireController>();
        requiredCampfires = campfireList.Length;
        UpdateObjectiveUI();
        
        Instance.bigPromptText.color = Color.white;
        Instance.bigPromptText.text = "Extinguish " + requiredCampfires + " campfires!";
        if (Instance._fadeOutBigPromptRoutine != null)
        {
            Instance.StopCoroutine(Instance._fadeOutBigPromptRoutine);
        }
        Instance._fadeOutBigPromptRoutine = Instance.StartCoroutine(Instance.FadeOutBigPromptRoutine());
    }

    private Coroutine _fadeOutBigPromptRoutine = null;
    private IEnumerator FadeOutBigPromptRoutine()
    {
        yield return new WaitForSeconds(3f);

        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime;
            bigPromptText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

    public static void CampfireLit(int enemiesToKillAdd)
    {
        Instance.enemiesToKill += enemiesToKillAdd;
        Instance.bigPromptText.color = Color.white;
        Instance.bigPromptText.text = "Slaughter the birds!";
        if (Instance._fadeOutBigPromptRoutine != null)
        {
            Instance.StopCoroutine(Instance._fadeOutBigPromptRoutine);
        }
        Instance._fadeOutBigPromptRoutine = Instance.StartCoroutine(Instance.FadeOutBigPromptRoutine());
        
        Instance.InternalCampfireLit();
    }

    public static void EnemyKilled()
    {
        bool activeCampfireFound = false;
        bool completedCampfireFound = false;
        for (int i = 0; i < Instance.campfireList.Length; i++)
        {
            if (Instance.campfireList[i].isActivated)
                activeCampfireFound = true;

            if (Instance.campfireList[i].isCompleted)
                completedCampfireFound = true;
        }

        if (!activeCampfireFound)
        {
            Instance.killsToProceedText.color = Color.clear;
            return;
        }
        
        if (Instance.enemiesKilled < Instance.enemiesToKill && activeCampfireFound)
        {
            Instance.enemiesKilled++;
            Instance.OnEnemyKilled?.Invoke();
            Instance.UpdateKillCounter();

            if (Instance.enemiesKilled >= Instance.enemiesToKill)
            {
                Instance.bigPromptText.color = Color.white;
                Instance.bigPromptText.text = "Campfire extinguished";
                if (Instance._fadeOutBigPromptRoutine != null)
                {
                    Instance.StopCoroutine(Instance._fadeOutBigPromptRoutine);
                }
                Instance._fadeOutBigPromptRoutine = Instance.StartCoroutine(Instance.FadeOutBigPromptRoutine());
                
                Instance.killsToProceedText.color = Color.clear;                
            }
        }
    }

    private void UpdateKillCounter()
    {
        killsToProceedText.text = $"Enemies killed: {enemiesKilled}/{enemiesToKill}";
    }

    private void InternalCampfireLit()
    {
        killsToProceedText.color = Color.white;
        enemySpawner.meleeEnemyPrefabs.Clear();
        enemySpawner.meleeEnemyPrefabs.Add(enemyList[campfiresLit]);
        
        enemySpawner.rangedEnemyPrefabs.Clear();
        enemySpawner.rangedEnemyPrefabs.Add(enemyList[campfiresLit]);

        enemySpawner.maxActiveEnemies = maxEnemies[campfiresLit];
        campfiresLit++;
        
        Instance.UpdateKillCounter();
    }
    
    public void CampfireExtinguished()
    {
        extinguishedCampfires++;

        bool activeCampfireFound = false;
        bool completedCampfireFound = false;
        for (int i = 0; i < campfireList.Length; i++)
        {
            if (campfireList[i].isActivated)
                activeCampfireFound = true;

            if (campfireList[i].isCompleted)
                completedCampfireFound = true;
        }

        if (!activeCampfireFound && completedCampfireFound)
        {
            killsToProceedText.color = Color.clear;
            enemiesKilled = 0;
        }
        
        UpdateObjectiveUI();

        if (extinguishedCampfires >= requiredCampfires)
        {
            Debug.Log("All campfires extinguished! Level complete!");
            Instance.bigPromptText.color = Color.white;
            Instance.bigPromptText.text = "Objective complete!";
            if (Instance._fadeOutBigPromptRoutine != null)
            {
                Instance.StopCoroutine(Instance._fadeOutBigPromptRoutine);
            }
            Instance._fadeOutBigPromptRoutine = Instance.StartCoroutine(Instance.FadeOutBigPromptRoutine());
            PlayerController player = FindFirstObjectByType<PlayerController>();
            player._playerStats.DamageTakenModifier = 0f;
            StartCoroutine(UnlockPortalAfterSeconds(4f));
        }
        else
        {
            Instance.bigPromptText.color = Color.white;
            Instance.bigPromptText.text = "Campfire extinguished!";
            if (Instance._fadeOutBigPromptRoutine != null)
            {
                Instance.StopCoroutine(Instance._fadeOutBigPromptRoutine);
            }
            Instance._fadeOutBigPromptRoutine = Instance.StartCoroutine(Instance.FadeOutBigPromptRoutine());
        }
    }

    private IEnumerator UnlockPortalAfterSeconds(float seconds)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        float timer = seconds;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            player._playerStats.DamageTakenModifier = 0f;
            yield return null;
        }
        
        onAllCampfiresExtinguished?.Invoke();

        // Unlock the portal when all campfires are out
        PortalManager pm = FindObjectOfType<PortalManager>();
        if (pm != null)
        {
            pm.UnlockPortal(portalIdToUnlock);
            SaveManager.Instance.SaveGame(); // Save the unlocked portal
        }
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText != null)
        {
            objectiveText.text = $"Campfires Extinguished: {extinguishedCampfires}/{requiredCampfires}";
        }
    }

    public void ResetObjectives()
    {
        extinguishedCampfires = 0;
        UpdateObjectiveUI();

        foreach (CampfireController campfire in FindObjectsOfType<CampfireController>())
        {
            campfire.ResetCampfire();
        }
    }
}
