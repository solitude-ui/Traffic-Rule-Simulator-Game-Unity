using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const float ScorePerMeter = 1f;
    private const float TrafficViolationPenalty = 50f;
    private const float TrafficSuccessReward = 50f;
    private const float DistanceScale = 0.75f;
    private const string IntroNotificationMessage = "Follow the road signs and drive safely.";
    private const float IntroNotificationDuration = 4f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private NewCarController carController;
    [SerializeField] private Transform carTransform;

    [Header("Live UI")]
    [SerializeField] private TextMeshProUGUI DistanceText;
    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private TextMeshProUGUI MaximumSpeedText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private TextMeshProUGUI TotalScoreText;
    [SerializeField] private TextMeshProUGUI TotalDistanceText;

    [Header("Icons")]
    [SerializeField] private GameObject SpeedIcon;
    [SerializeField] private GameObject DistanceIcon;
    [SerializeField] private GameObject ScoreIcon;

    [Header("Score Popups")]
    [SerializeField] private Image plus50Image;
    [SerializeField] private Image minus50Image;
    [SerializeField] private float scorePopupDuration = 1.5f;

    [Header("Notifications")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3.5f;

    [Header("Splash Screen")]
    [SerializeField] private GameObject splashRoot;
    [Min(0f)]
    [Tooltip("Time in seconds that the splash screen stays visible at game start.")]
    [SerializeField] private float splashDisplayDuration = 1f;

    private float speed = 0f;
    private float distance = 0f;
    private float score = 0f;
    private float maximumSpeed = 0f;
    private Vector3 lastPosition;
    private Coroutine scorePopupCoroutine;
    private Coroutine notificationCoroutine;
    private CanvasGroup splashCanvasGroup;

    void Awake()
    {
        AutoAssignSplash();

        if (splashRoot != null)
            ShowSplashImmediately();
    }

    void Start()
    {
        GameOverPanel.SetActive(false);

        SpeedIcon.SetActive(true);
        DistanceIcon.SetActive(true);
        ScoreIcon.SetActive(true);

        if (carController == null)
            Debug.LogError("NewCarController not assigned!");

        if (carTransform == null)
            Debug.LogError("Car Transform not assigned!");

        if (carTransform != null)
            lastPosition = carTransform.position;

        AutoAssignScorePopups();
        HideScorePopups();
        AutoAssignNotificationText();
        HideNotification();
        StartCoroutine(PlayIntroSequence());
    }

    void Update()
    {
        if (carController == null || carTransform == null) return;

        UpdateSpeed();
        UpdateDistance();
        UpdateScore();
        UpdateMaxSpeed();
    }

    void UpdateSpeed()
    {
        speed = carController.CarSpeed();
        speedText.text = speed.ToString("0") + " km/h";
    }

    void UpdateDistance()
    {
        float distanceTravelledThisFrame = Vector3.Distance(lastPosition, carTransform.position) * DistanceScale;
        distance += distanceTravelledThisFrame / 1000f;
        score += distanceTravelledThisFrame * ScorePerMeter;
        lastPosition = carTransform.position;

        DistanceText.text = distance.ToString("0.00") + " km";
    }

    void UpdateScore()
    {
        ScoreText.text = score.ToString("0");
    }

    void UpdateMaxSpeed()
    {
        float currentSpeed = carController.CarSpeed();

        if (currentSpeed > maximumSpeed)
            maximumSpeed = currentSpeed;

        MaximumSpeedText.text = maximumSpeed.ToString("0") + " km/h";
    }

    public void GameOver()
    {
        Time.timeScale = 0f;

        GameOverPanel.SetActive(true);

        SpeedIcon.SetActive(false);
        DistanceIcon.SetActive(false);
        ScoreIcon.SetActive(false);

        TotalScoreText.text = score.ToString("0");
        TotalDistanceText.text = distance.ToString("0.00") + " km";
    }

    public void ApplyTrafficViolationPenalty()
    {
        score = Mathf.Max(0f, score - TrafficViolationPenalty);
        ScoreText.text = score.ToString("0");
        Debug.Log($"UIManager: Traffic violation applied. -{TrafficViolationPenalty:0} points. Current score: {score:0}");
        ShowScorePopup(minus50Image);
    }

    public void ApplyTrafficSuccessReward()
    {
        score += TrafficSuccessReward;
        ScoreText.text = score.ToString("0");
        Debug.Log($"UIManager: Traffic rule completed. +{TrafficSuccessReward:0} points. Current score: {score:0}");
        ShowScorePopup(plus50Image);
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ShowNotification(string message, float duration = -1f)
    {
        if (notificationText == null)
        {
            AutoAssignNotificationText();
            if (notificationText == null) return;
        }

        if (notificationCoroutine != null)
            StopCoroutine(notificationCoroutine);

        float displayDuration = duration > 0f ? duration : notificationDuration;
        notificationCoroutine = StartCoroutine(ShowNotificationRoutine(message, displayDuration));
    }

    private void AutoAssignScorePopups()
    {
        GameObject uiRoot = GameObject.Find("GameUI");
        if (uiRoot == null)
        {
            Debug.LogWarning("UIManager: GameUI was not found, so +50/-50 popups could not be auto-assigned.");
            return;
        }

        if (plus50Image == null)
            plus50Image = FindImage(uiRoot.transform, "+50");

        if (minus50Image == null)
            minus50Image = FindImage(uiRoot.transform, "-50");

        Debug.Log($"UIManager: Score popup references. +50 assigned = {plus50Image != null}, -50 assigned = {minus50Image != null}");
    }

    private void AutoAssignNotificationText()
    {
        GameObject uiRoot = GameObject.Find("GameUI");
        if (uiRoot == null) return;

        if (notificationPanel == null)
        {
            Transform notificationPanelTransform = uiRoot.transform.Find("Notification");
            if (notificationPanelTransform != null)
                notificationPanel = notificationPanelTransform.gameObject;
        }

        if (notificationText != null) return;

        Transform notificationTransform = notificationPanel != null
            ? notificationPanel.transform.Find("Notification text")
            : uiRoot.transform.Find("Notification text");

        if (notificationTransform == null) return;

        notificationText = notificationTransform.GetComponent<TextMeshProUGUI>();
    }

    private void AutoAssignSplash()
    {
        if (splashRoot != null && splashCanvasGroup != null) return;

        if (splashRoot == null)
            splashRoot = FindSplashRoot();

        if (splashRoot == null) return;

        splashCanvasGroup = splashRoot.GetComponent<CanvasGroup>();
        if (splashCanvasGroup == null)
            splashCanvasGroup = splashRoot.AddComponent<CanvasGroup>();
    }

    private Image FindImage(Transform parent, string objectName)
    {
        Transform target = parent.Find(objectName);
        if (target == null) return null;

        return target.GetComponent<Image>();
    }

    private void ShowScorePopup(Image targetImage)
    {
        if (targetImage == null)
        {
            Debug.LogWarning("UIManager: Tried to show a score popup, but the target Image reference is missing.");
            return;
        }

        if (scorePopupCoroutine != null)
            StopCoroutine(scorePopupCoroutine);

        Debug.Log($"UIManager: Showing score popup '{targetImage.gameObject.name}' for {scorePopupDuration:0.##} seconds.");
        scorePopupCoroutine = StartCoroutine(ShowScorePopupRoutine(targetImage));
    }

    private IEnumerator ShowScorePopupRoutine(Image targetImage)
    {
        HideScorePopups();
        SetImageState(targetImage, true);

        yield return new WaitForSeconds(scorePopupDuration);

        SetImageState(targetImage, false);
        scorePopupCoroutine = null;
    }

    private void HideScorePopups()
    {
        SetImageState(plus50Image, false);
        SetImageState(minus50Image, false);
    }

    private IEnumerator PlayIntroSequence()
    {
        if (splashRoot != null && splashCanvasGroup != null)
        {
            splashCanvasGroup.alpha = 1f;
            if (splashDisplayDuration > 0f)
                yield return new WaitForSeconds(splashDisplayDuration);

            splashRoot.SetActive(false);
        }

        ShowNotification(IntroNotificationMessage, IntroNotificationDuration);
    }

    private GameObject FindSplashRoot()
    {
        GameObject uiRoot = GameObject.Find("GameUI");
        if (uiRoot == null) return null;

        Transform splashTransform = uiRoot.transform.Find("Splash");
        return splashTransform != null ? splashTransform.gameObject : null;
    }

    private void ShowSplashImmediately()
    {
        if (splashRoot == null || splashCanvasGroup == null) return;

        splashRoot.SetActive(true);
        splashRoot.transform.SetAsLastSibling();
        splashCanvasGroup.alpha = 1f;
    }

    private IEnumerator ShowNotificationRoutine(string message, float duration)
    {
        notificationText.text = message;
        SetNotificationState(true);

        yield return new WaitForSeconds(duration);

        HideNotification();
        notificationCoroutine = null;
    }

    private void HideNotification()
    {
        if (notificationText == null) return;

        notificationText.text = string.Empty;
        SetNotificationState(false);
    }

    private void SetNotificationState(bool isActive)
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(isActive);

        if (notificationText != null)
            notificationText.gameObject.SetActive(isActive);
    }

    private void SetImageState(Image image, bool isActive)
    {
        if (image == null) return;

        image.enabled = isActive;
        if (image.gameObject.activeSelf != isActive)
            image.gameObject.SetActive(isActive);
    }
}
