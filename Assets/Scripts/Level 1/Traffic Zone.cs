using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrafficZone : MonoBehaviour
{
    [Header("Traffic Light")]
    [SerializeField] private TrafficLight trafficLight;

    [Header("UI References")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Image redImage;
    [SerializeField] private Image yellowImage;
    [SerializeField] private Image greenImage;
    [SerializeField] private Image warningImage;
    [SerializeField] private string playerTag = "Player";

    [Header("Warning Settings")]
    [SerializeField] private float warningEdgeDistance = 2f;
    [SerializeField] private float warningBlinkInterval = 0.2f;

    [Header("Penalty")]
    [SerializeField] private UIManager uiManager;

    private bool playerInside = false;
    private BoxCollider boxCollider;
    private Transform playerRoot;
    private Coroutine warningCoroutine;
    private bool warningActive;

    private void Awake()
    {
        AutoAssignUIReferences();
    }

    private void Start()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
            boxCollider.isTrigger = true;
            Debug.Log("TrafficZone: Box Collider is enabled and set as trigger.");
        }

        if (uiPanel != null)
            uiPanel.SetActive(false);

        ResetUI();
        SetWarningState(false);

        if (trafficLight == null)
            Debug.LogWarning("TrafficZone: TrafficLight reference is missing.", this);

        if (uiPanel == null)
            Debug.LogWarning("TrafficZone: Traffic light UI panel was not found. Assign GameUI or uiPanel in the Inspector.", this);
    }

    private void Update()
    {
        if (!playerInside) return;
        if (trafficLight == null || uiPanel == null) return;

        UpdateUI();
        UpdateWarningZoneCheck();
    }

    private void AutoAssignUIReferences()
    {
        if (uiRoot == null)
        {
            GameObject rootObject = GameObject.Find("GameUI");
            if (rootObject != null)
                uiRoot = rootObject;
        }

        Transform rootTransform = uiRoot != null ? uiRoot.transform : null;

        if (uiPanel == null && rootTransform != null)
        {
            Transform panelTransform = rootTransform.Find("TrafficLight Panel");
            if (panelTransform != null)
                uiPanel = panelTransform.gameObject;
        }

        Transform searchRoot = uiPanel != null ? uiPanel.transform : rootTransform;

        if (redImage == null)
            redImage = FindImage(searchRoot, "red");

        if (yellowImage == null)
            yellowImage = FindImage(searchRoot, "yellow");

        if (greenImage == null)
            greenImage = FindImage(searchRoot, "green");

        if (warningImage == null && rootTransform != null)
            warningImage = FindImage(rootTransform, "Warning");
    }

    private Image FindImage(Transform parent, string objectName)
    {
        if (parent == null) return null;

        Transform target = parent.Find(objectName);
        if (target == null) return null;

        Image image = target.GetComponent<Image>();
        if (image == null)
            Debug.LogWarning($"TrafficZone: '{objectName}' exists but has no Image component.", target);

        return image;
    }

    private void UpdateUI()
    {
        if (redImage == null || yellowImage == null || greenImage == null)
        {
            AutoAssignUIReferences();
            if (redImage == null || yellowImage == null || greenImage == null) return;
        }

        TrafficLight.State state = trafficLight.GetState();

        SetIndicatorState(redImage, false);
        SetIndicatorState(yellowImage, false);
        SetIndicatorState(greenImage, false);

        switch (state)
        {
            case TrafficLight.State.Red:
                SetIndicatorState(redImage, true);
                break;

            case TrafficLight.State.Yellow:
                SetIndicatorState(yellowImage, true);
                break;

            case TrafficLight.State.Green:
                SetIndicatorState(greenImage, true);
                break;
        }
    }

    private void ResetUI()
    {
        if (redImage != null) SetIndicatorState(redImage, false);
        if (yellowImage != null) SetIndicatorState(yellowImage, false);
        if (greenImage != null) SetIndicatorState(greenImage, false);
    }

    private void SetIndicatorState(Image image, bool isActive)
    {
        if (image == null) return;

        image.enabled = isActive;
        if (image.gameObject.activeSelf != isActive)
            image.gameObject.SetActive(isActive);
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

    private void UpdateWarningZoneCheck()
    {
        if (playerRoot == null || boxCollider == null || trafficLight == null)
        {
            StopWarningBlink();
            return;
        }

        TrafficLight.State currentState = trafficLight.GetState();
        bool shouldWarnForLight = currentState == TrafficLight.State.Red || currentState == TrafficLight.State.Yellow;

        if (!shouldWarnForLight)
        {
            StopWarningBlink();
            return;
        }

        Bounds zoneBounds = boxCollider.bounds;
        Vector3 playerPosition = playerRoot.position;

        float distanceToEdgeX = Mathf.Min(playerPosition.x - zoneBounds.min.x, zoneBounds.max.x - playerPosition.x);
        float distanceToEdgeZ = Mathf.Min(playerPosition.z - zoneBounds.min.z, zoneBounds.max.z - playerPosition.z);
        float closestDistanceToEdge = Mathf.Min(distanceToEdgeX, distanceToEdgeZ);

        if (closestDistanceToEdge <= warningEdgeDistance)
            StartWarningBlink();
        else
            StopWarningBlink();
    }

    private void StartWarningBlink()
    {
        if (warningImage == null || warningActive) return;

        warningActive = true;

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningCoroutine = StartCoroutine(BlinkWarning());
    }

    private void StopWarningBlink()
    {
        warningActive = false;

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        SetWarningState(false);
    }

    private IEnumerator BlinkWarning()
    {
        bool isVisible = false;

        while (playerInside && warningActive)
        {
            isVisible = !isVisible;
            SetWarningState(isVisible);

            yield return new WaitForSeconds(warningBlinkInterval);
        }

        warningCoroutine = null;
        warningActive = false;
        SetWarningState(false);
    }

    private void SetWarningState(bool isActive)
    {
        if (warningImage == null) return;

        warningImage.enabled = isActive;
        if (warningImage.gameObject.activeSelf != isActive)
            warningImage.gameObject.SetActive(isActive);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered: " + other.name);

        if (IsPlayerCollider(other))
        {
            Debug.Log("TrafficZone: Player car entered traffic zone.");
            playerInside = true;
            playerRoot = other.transform.root;

            if (uiPanel != null)
                uiPanel.SetActive(true);

            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit: " + other.name);

        if (IsPlayerCollider(other))
        {
            ApplyTrafficZoneScoreResult();
            Debug.Log("TrafficZone: Player car left traffic zone.");
            playerInside = false;
            playerRoot = null;
            StopWarningBlink();

            if (uiPanel != null)
                uiPanel.SetActive(false);

            ResetUI();
        }
    }

    private void ApplyTrafficZoneScoreResult()
    {
        if (trafficLight == null || uiManager == null) return;

        TrafficLight.State currentState = trafficLight.GetState();
        if (currentState == TrafficLight.State.Red || currentState == TrafficLight.State.Yellow)
            uiManager.ApplyTrafficViolationPenalty();
        else if (currentState == TrafficLight.State.Green)
            uiManager.ApplyTrafficSuccessReward();
    }
}
