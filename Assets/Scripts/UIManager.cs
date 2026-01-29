using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private CarController carController;

    [SerializeField]private Transform carTransform;
    [SerializeField]private TextMeshProUGUI DistanceText;

    [SerializeField]private TextMeshProUGUI ScoreText;

    [SerializeField] private GameObject GameOverPanel;

    [SerializeField]private TextMeshProUGUI TotalScoreText;

    [SerializeField]private TextMeshProUGUI TotalDistanceText;

    [SerializeField]private TextMeshProUGUI MaximumSpeedText;

    
    [SerializeField]private GameObject SpeedIcon;

    [SerializeField]private GameObject DistanceIcon;

    [SerializeField]private GameObject ScoreIcon;


    


    private float speed=0f;
    private float distance=0f;

    private float Score=0f;

    private float maximumSpeed=0f;
    // Start is called before the first frame update
    void Start()
    {
        GameOverPanel.SetActive(false);
        SpeedIcon.SetActive(true);
        DistanceIcon.SetActive(true);
        ScoreIcon.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        SpeedUI();
        DistanceUI();
        ScoreUI();
        MaximumSpeed();
       
    }

    void SpeedUI()
    {
        speed=carController.CarSpeed();
        speedText.text=speed.ToString("0"+" km/h");
    }

    void DistanceUI()
    {
        distance=carTransform.position.z/1000;//convert to km
        DistanceText.text=distance.ToString("0.00"+" km");
    }

    void ScoreUI()
    {
        Score=carTransform.position.z*6;
        ScoreText.text=Score.ToString("0");
    }

    public void GameOver()
    {
        Time.timeScale=0f;
        GameOverPanel.SetActive(true);
        SpeedIcon.SetActive(false);
        DistanceIcon.SetActive(false);
        ScoreIcon.SetActive(false);
        TotalScoreText.text=Score.ToString("0");
        TotalDistanceText.text=distance.ToString("0.00"+" km");


    }

    void MaximumSpeed()
    {
        float currentSpeed=carController.CarSpeed();
        if(currentSpeed>maximumSpeed)
        {
            maximumSpeed=currentSpeed;
        }
        MaximumSpeedText.text=maximumSpeed.ToString("0"+" km/h");
    }


    public void TryAgain()
    {
         Time.timeScale = 1f;//time pause reset
        var CurrentScene=SceneManager.GetActiveScene();
        SceneManager.LoadScene(CurrentScene.name);
    }

}
