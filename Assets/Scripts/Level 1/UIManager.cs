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

    private float speed = 0f;
    private float distance = 0f;
    private float score = 0f;
    private float maximumSpeed = 0f;
    private Vector3 lastPosition;
    private Coroutine scorePopupCoroutine;

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
        ShowScorePopup(minus50Image);
    }

    public void ApplyTrafficSuccessReward()
    {
        score += TrafficSuccessReward;
        ScoreText.text = score.ToString("0");
        ShowScorePopup(plus50Image);
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void AutoAssignScorePopups()
    {
        GameObject uiRoot = GameObject.Find("GameUI");
        if (uiRoot == null) return;

        if (plus50Image == null)
            plus50Image = FindImage(uiRoot.transform, "+50");

        if (minus50Image == null)
            minus50Image = FindImage(uiRoot.transform, "-50");
    }

    private Image FindImage(Transform parent, string objectName)
    {
        Transform target = parent.Find(objectName);
        if (target == null) return null;

        return target.GetComponent<Image>();
    }

    private void ShowScorePopup(Image targetImage)
    {
        if (targetImage == null) return;

        if (scorePopupCoroutine != null)
            StopCoroutine(scorePopupCoroutine);

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

    private void SetImageState(Image image, bool isActive)
    {
        if (image == null) return;

        image.enabled = isActive;
        if (image.gameObject.activeSelf != isActive)
            image.gameObject.SetActive(isActive);
    }
}
