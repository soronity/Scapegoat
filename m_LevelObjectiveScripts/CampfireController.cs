using System;
using UnityEngine;
using System.Collections;
using m_Programmers.p_Scripts.s_Jesper;

public class CampfireController : MonoBehaviour, IInteractable
{
   [Header("Light Settings")]
   public Light campfireLight;
   public float lightTransitionTime = 3f;
   public float initialIntensity = 0f;
   public float activeIntensity = 50f;
   public float extinguishedIntensity = 5f;
   public Color extinguishedColor = Color.red;
   public Color activatedColor = Color.blue;
   private Color originalLightColor;


   [Header("Objective Settings")]
   public GameObject interactPrompt;
   [SerializeField] private float objectiveTimer;
   [SerializeField] private int requiredEnemyKills = 15;

   [Header("Interaction")]
   public AudioSource interactionSound;
   public ParticleSystem glowEffect;

   public bool isActivated;
   public bool isCompleted;

   [Header("Enemy Spawn Settings")]
   public EnemySpawner enemySpawner;
   public bool activateHordeModeOnInteract = true;

   void Start()
   {
       if (campfireLight != null)
       {
           //originalLightColor = campfireLight.color;
           //campfireLight.intensity = initialIntensity;
           //StartLightTransition(activeIntensity, originalLightColor);
       }
   }

   public void Interact(PlayerController player)
   {
       if (isActivated || isCompleted)
       {
           return;
       }
       
       interactPrompt.SetActive(false);
       bool activeCampfireFound = false;
       bool completedCampfireFound = false;
       for (int i = 0; i < CampfireManager.Instance.campfireList.Length; i++)
       {
           if (CampfireManager.Instance.campfireList[i].isActivated)
               activeCampfireFound = true;

           if (CampfireManager.Instance.campfireList[i].isCompleted)
               completedCampfireFound = true;
       }

       if (!activeCampfireFound && completedCampfireFound)
           CampfireManager.Instance.enemiesKilled = 0;
       
       CampfireManager.CampfireLit(requiredEnemyKills);
       CampfireManager.Instance.OnEnemyKilled += EnemyKilled;
       StartObjective();
   }

   private void OnDisable()
   {
       CampfireManager.Instance.OnEnemyKilled -= EnemyKilled;
   }

   private void StartObjective()
   {
       isActivated = true;

       // Change to activated color
       //StartLightTransition(activeIntensity, activatedColor);

       if (enemySpawner != null)
       {
           if (activateHordeModeOnInteract)
           {
               enemySpawner.ActivateHordeMode();
           }
           else
           {
               enemySpawner.timeBetweenWaves = 1f;
           }
       }

       //StartCoroutine(ObjectiveTimer());
   }

   private IEnumerator ObjectiveTimer()
   {
       float elapsed = 0f;
       while (elapsed < objectiveTimer)
       {
           yield return new WaitForSeconds(1f);
           elapsed += 1f;
       }

       CompleteObjective();
   }
   
   public void EnemyKilled()
   {
       if (CampfireManager.Instance.enemiesKilled >= CampfireManager.Instance.enemiesToKill)
       {
           CompleteObjective();
       }
   }

   private void CompleteObjective()
   {
       isCompleted = true;
       isActivated = false;
       StartLightTransition(extinguishedIntensity, extinguishedColor);
       CampfireManager.Instance.OnEnemyKilled -= EnemyKilled;
       CampfireManager.Instance?.CampfireExtinguished();
   }
   
   public void ResetCampfire()
   {
       isCompleted = false;
       isActivated = false;

       StartLightTransition(initialIntensity, originalLightColor);
   }

   private void StartLightTransition(float targetIntensity, Color targetColor)
   {
       StartCoroutine(TransitionLight(targetIntensity, targetColor));
   }

   private IEnumerator TransitionLight(float targetIntensity, Color targetColor)
   {
       float startIntensity = campfireLight.intensity;
       Color startColor = campfireLight.color;
       float elapsedTime = 0f;

       while (elapsedTime < lightTransitionTime)
       {
           elapsedTime += Time.deltaTime;
           float t = elapsedTime / lightTransitionTime;

           campfireLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
           campfireLight.color = Color.Lerp(startColor, targetColor, t);

           yield return null;
       }

       campfireLight.intensity = targetIntensity;
       campfireLight.color = targetColor;
   }
}