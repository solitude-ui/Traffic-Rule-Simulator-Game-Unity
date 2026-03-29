using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class SpeedZone : MonoBehaviour
{
    private const float ZoneEntryNotificationDuration = 3.5f;

    [Header("Speed Rule")]
    [SerializeField] private float speedLimit = 50f;
    [SerializeField] private float warningSpeedThreshold = 40f;
    [SerializeField] private string playerTag = "Player";

    [Header("UI References")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private Image warningImage;
    [SerializeField] private UIManager uiManager;

    [Header("Warning Settings")]
    [SerializeField] private float warningBlinkInterval = 0.2f;

    private bool playerInside;
    private bool speedLimitBrokenThisPass;
    private Transform playerRoot;
    private NewCarController playerCarController;
    private Coroutine warningCoroutine;
    private bool warningActive;

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void Start()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.enabled = true;
        trigger.isTrigger = true;

        SetWarningState(false);
    }

    private void Update()
    {
        if (!playerInside || playerCarController == null)
        {
            StopWarningBlink();
            return;
        }

        float currentSpeed = playerCarController.CarSpeed();

        if (currentSpeed > speedLimit)
            speedLimitBrokenThisPass = true;

        if (currentSpeed > warningSpeedThreshold)
            StartWarningBlink();
        else
            StopWarningBlink();
    }

    private void AutoAssignReferences()
    {
        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (uiRoot == null)
            uiRoot = GameObject.Find("GameUI");

        if (warningImage == null && uiRoot != null)
        {
            Transform warningTransform = uiRoot.transform.Find("Warning");
            if (warningTransform != null)
                warningImage = warningTransform.GetComponent<Image>();
        }
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered: " + other.name);

        if (!IsPlayerCollider(other) || playerInside)
            return;

        Debug.Log("SpeedZone: Player car entered the speed zone.");
        playerInside = true;
        speedLimitBrokenThisPass = false;
        playerRoot = other.transform.root;
        playerCarController = playerRoot.GetComponentInChildren<NewCarController>();

        if (playerCarController == null)
            playerCarController = FindFirstObjectByType<NewCarController>();

        if (uiManager != null)
            uiManager.ShowNotification(GetSpeedZoneMessage(), ZoneEntryNotificationDuration);

    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit: " + other.name);

        if (!IsPlayerCollider(other) || !playerInside)
            return;

        if (speedLimitBrokenThisPass)
        {
            Debug.Log("SpeedZone: Speed limit was broken inside the zone. Applying -50 penalty.", this);
            if (uiManager != null)
            {
                uiManager.MarkSpeedZoneObjective(false);
                uiManager.ApplyTrafficViolationPenalty();
            }
        }
        else
        {
            Debug.Log("SpeedZone: Speed zone completed successfully. Applying +50 reward.", this);
            if (uiManager != null)
            {
                uiManager.MarkSpeedZoneObjective(true);
                uiManager.ApplyTrafficSuccessReward();
            }
        }

        playerInside = false;
        speedLimitBrokenThisPass = false;
        playerRoot = null;
        playerCarController = null;
        StopWarningBlink();
    }

    private void StartWarningBlink()
    {
        if (warningImage == null || warningActive)
            return;

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
        if (warningImage == null)
            return;

        warningImage.enabled = isActive;
        if (warningImage.gameObject.activeSelf != isActive)
            warningImage.gameObject.SetActive(isActive);
    }

    private string GetSpeedZoneMessage()
    {
        return $"Speed zone ahead. Keep your speed at or below {Mathf.RoundToInt(speedLimit)} km/h.";
    }
}
