using UnityEngine;
using Scapegoat.SceneManagement;

public class Portal : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private bool activatePlayerOnSceneLoad = false;
    [SerializeField] private bool displayHUDOnSceneLoad = false;

    [Header("Portal Locking")]
    [SerializeField] private string portalId;
    [SerializeField] private bool requireUnlock = false;

    private bool isUnlocked = true;
    private PortalManager portalManager;

    public GameObject portalGate;

    private void Start()
    {
        if (requireUnlock)
        {
            portalGate.SetActive(false);
            portalManager = FindObjectOfType<PortalManager>();
            if (portalManager != null)
            {
                isUnlocked = portalManager.IsPortalUnlocked(portalId);
            }
            else
            {
                Debug.LogWarning("PortalManager not found in scene");
            }
        }

        if (isUnlocked)
        {
            portalGate.SetActive(true);
        }
        

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (requireUnlock && !isUnlocked)
        {
            Debug.Log("Portal " + portalId + " is locked");
            return;
        }

        SceneSystemManager.LoadSceneAdditiveAndUnload(
            sceneToLoad,
            gameObject.scene.name,
            activatePlayerOnSceneLoad,
            displayHUDOnSceneLoad
        );
    }

    public void Unlock()
    {
        isUnlocked = true;
        portalGate.SetActive(true);

        if (portalManager != null)
        {
            portalManager.UnlockPortal(portalId);
        }
    }
}