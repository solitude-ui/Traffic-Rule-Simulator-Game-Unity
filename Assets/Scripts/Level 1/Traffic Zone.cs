using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrafficZone : MonoBehaviour
{
    private const string ZoneEntryNotificationMessage = "Pass the traffic signal only when the light turns green.";
    private const float ZoneEntryNotificationDuration = 3.5f;

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

    [Header("Valid Exit Rule")]
    [SerializeField] private float exitEdgeTolerance = 0.25f;

    private bool playerInside = false;
    private bool hasShownZoneEntryNotification = false;
    private float entryXPosition = 0f;
    private bool enteredFromFront = false;
    private float zoneCenterX = 0f;
    private float zonePositiveEdgeX = 0f;
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
            zoneCenterX = boxCollider.bounds.center.x;
            zonePositiveEdgeX = boxCollider.bounds.max.x;
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
            if (playerInside) return;

            Debug.Log("TrafficZone: Player car entered traffic zone.");
            playerInside = true;
            playerRoot = other.transform.root;
            entryXPosition = playerRoot.position.x;
            enteredFromFront = entryXPosition > zoneCenterX;

            if (uiPanel != null)
                uiPanel.SetActive(true);

            if (uiManager != null && !hasShownZoneEntryNotification)
            {
                uiManager.ShowNotification(ZoneEntryNotificationMessage, ZoneEntryNotificationDuration);
                hasShownZoneEntryNotification = true;
            }

            UpdateUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit: " + other.name);

        if (IsPlayerCollider(other))
        {
            if (!playerInside) return;

            if (ShouldApplyTrafficRule(other.transform.root.position))
                ApplyTrafficZoneScoreResult();
            else
                Debug.Log($"TrafficZone: Rule not applied. Entry X = {entryXPosition:0.##}, Exit X = {other.transform.root.position.x:0.##}, Positive edge X = {zonePositiveEdgeX:0.##}, tolerance = {exitEdgeTolerance:0.##}.", this);

            Debug.Log("TrafficZone: Player car left traffic zone.");
            playerInside = false;
            playerRoot = null;
            entryXPosition = 0f;
            enteredFromFront = false;
            StopWarningBlink();

            if (uiPanel != null)
                uiPanel.SetActive(false);

            ResetUI();
        }
    }

    private void ApplyTrafficZoneScoreResult()
    {
        if (trafficLight == null || uiManager == null)
        {
            Debug.LogWarning("TrafficZone: Could not apply traffic zone result because TrafficLight or UIManager is missing.", this);
            return;
        }

        TrafficLight.State currentState = trafficLight.GetState();
        if (currentState == TrafficLight.State.Red || currentState == TrafficLight.State.Yellow)
        {
            Debug.Log($"TrafficZone: Rule completed on {currentState}. Applying -50 penalty.", this);
            uiManager.MarkTrafficZoneObjective(false);
            uiManager.ApplyTrafficViolationPenalty();
        }
        else if (currentState == TrafficLight.State.Green)
        {
            Debug.Log("TrafficZone: Rule completed on Green. Applying +50 reward.", this);
            uiManager.MarkTrafficZoneObjective(true);
            uiManager.ApplyTrafficSuccessReward();
        }
    }

    private bool ShouldApplyTrafficRule(Vector3 worldPosition)
    {
        float exitXPosition = worldPosition.x;
        bool exitedFromFront = exitXPosition > zoneCenterX;
        bool enteredFromBack = !enteredFromFront;
        bool crossedThrough = (enteredFromFront && !exitedFromFront) || (enteredFromBack && exitedFromFront);
        bool exitedThroughPositiveXFace = exitXPosition >= zonePositiveEdgeX - exitEdgeTolerance;

        Debug.Log($"TrafficZone: Checking rule. Entry X = {entryXPosition:0.##}, Exit X = {exitXPosition:0.##}, zoneCenterX = {zoneCenterX:0.##}, positiveEdgeX = {zonePositiveEdgeX:0.##}, enteredFromFront = {enteredFromFront}, exitedFromFront = {exitedFromFront}, crossedThrough = {crossedThrough}, exitedThroughPositiveXFace = {exitedThroughPositiveXFace}.", this);

        if (!crossedThrough)
            return false;

        return exitedThroughPositiveXFace;
    }
}
