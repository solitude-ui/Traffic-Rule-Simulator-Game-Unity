using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessCityManager : MonoBehaviour
{
    [SerializeField] private Transform city1;
    [SerializeField] private Transform city2;
    [SerializeField] private Transform playerCarTransform;
    [SerializeField] private float cityLength = 80f; // Full length of one city segment
    
    private float halfLength;
    private bool isCity1Ahead = true;

    void Start()
    {
        halfLength = cityLength / 2f;
        
        // Initialize positions
        city1.position = new Vector3(0, 0, 0);
        city2.position = new Vector3(0, 0, cityLength);
    }

    void Update()
    {
        if (isCity1Ahead)
        {
            // Check if player passed city1's midpoint
            if (playerCarTransform.position.z > city1.position.z + halfLength)
            {
                // Move city1 ahead of city2
                city1.position = new Vector3(0, 0, city2.position.z + cityLength);
                isCity1Ahead = false;
            }
        }
        else
        {
            // Check if player passed city2's midpoint
            if (playerCarTransform.position.z > city2.position.z + halfLength)
            {
                // Move city2 ahead of city1
                city2.position = new Vector3(0, 0, city1.position.z + cityLength);
                isCity1Ahead = true;
            }
        }
    }
}