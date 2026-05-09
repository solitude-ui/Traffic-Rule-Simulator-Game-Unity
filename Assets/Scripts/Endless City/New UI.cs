using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NewUI : MonoBehaviour
{
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

    private float speed = 0f;
    private float distance = 0f;
    private float score = 0f;
    private float maximumSpeed = 0f;
    private Vector3 startPosition;
    private bool finalResultSent;

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

        startPosition = carTransform.position;
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
        distance = Vector3.Distance(startPosition, carTransform.position) / 1000f;
        DistanceText.text = distance.ToString("0.00") + " km";
    }

    void UpdateScore()
    {
        score = distance * 1000f * 6f;
        ScoreText.text = score.ToString("0");
    }

    void UpdateMaxSpeed()
    {
        float currentSpeed = carController.CarSpeed();

        if (currentSpeed > maximumSpeed)
            maximumSpeed = currentSpeed;

        MaximumSpeedText.text = maximumSpeed.ToString("0") + " km/h";
    }

    public void GameOver(bool completed = false)
    {
        Time.timeScale = 0f;

        GameOverPanel.SetActive(true);

        SpeedIcon.SetActive(false);
        DistanceIcon.SetActive(false);
        ScoreIcon.SetActive(false);

        TotalScoreText.text = score.ToString("0");
        TotalDistanceText.text = distance.ToString("0.00") + " km";
        SendFinalResultOnce(completed);
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void SendFinalResultOnce(bool completed)
    {
        if (finalResultSent)
            return;

        finalResultSent = true;
        WebGLBridge.SendFinalResult(Mathf.RoundToInt(score), completed);
    }
}
