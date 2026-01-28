using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private CarController carController;

    [SerializeField]private Transform carTransform;
    [SerializeField]private TextMeshProUGUI DistanceText;

    [SerializeField]private TextMeshProUGUI ScoreText;

    private float speed=0f;
    private float distance=0f;

    private float Score=0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SpeedUI();
        DistanceUI();
        ScoreUI();
       
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
}
