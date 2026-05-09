using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinalPointTrigger : MonoBehaviour
{
    [Header("Flow References")]
    [SerializeField] private PathManager pathManager;
    [SerializeField] private UIManager uiManager;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Visibility")]
    [SerializeField] private bool hideUntilPathComplete = true;

    private bool hasTriggeredWin;
    private Collider triggerCollider;
    private Renderer[] cachedRenderers;
    private ParticleSystem[] cachedParticles;

    private void Start()
    {
        if (pathManager == null)
            pathManager = FindFirstObjectByType<PathManager>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;

        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedParticles = GetComponentsInChildren<ParticleSystem>(true);

        if (pathManager == null)
            Debug.LogWarning("FinalPointTrigger: PathManager reference is missing.", this);
        else
            pathManager.PathCompleted += HandlePathCompleted;

        if (uiManager == null)
            Debug.LogWarning("FinalPointTrigger: UIManager reference is missing.", this);

        bool shouldShowImmediately = !hideUntilPathComplete || (pathManager != null && pathManager.IsPathComplete);
        SetFinalPointVisible(shouldShowImmediately);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggeredWin || !IsPlayerCollider(other))
            return;

        if (pathManager == null)
            return;

        if (!pathManager.IsPathComplete)
        {
            Debug.Log("FinalPointTrigger: Player reached final point before collecting the last arrow.", this);
            return;
        }

        if (uiManager == null)
            return;

        hasTriggeredWin = true;
        uiManager.MarkArrowObjectiveComplete();
        Debug.Log("FinalPointTrigger: Final point reached. Triggering win scenario.", this);
        uiManager.GameOver(true);
    }

    private void OnDestroy()
    {
        if (pathManager != null)
            pathManager.PathCompleted -= HandlePathCompleted;
    }

    private void HandlePathCompleted()
    {
        SetFinalPointVisible(true);
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag(playerTag)) return true;
        if (other.transform.root.CompareTag(playerTag)) return true;

        Rigidbody attachedRigidbody = other.attachedRigidbody;
        if (attachedRigidbody != null && attachedRigidbody.CompareTag(playerTag))
            return true;

        return false;
    }

    private void SetFinalPointVisible(bool isVisible)
    {
        if (triggerCollider != null)
            triggerCollider.enabled = isVisible;

        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                    cachedRenderers[i].enabled = isVisible;
            }
        }

        if (cachedParticles != null)
        {
            for (int i = 0; i < cachedParticles.Length; i++)
            {
                if (cachedParticles[i] == null) continue;

                if (isVisible)
                    cachedParticles[i].Play();
                else
                    cachedParticles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
