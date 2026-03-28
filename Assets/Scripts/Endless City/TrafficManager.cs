using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [SerializeField]private Transform[] Lane;
    [SerializeField]private GameObject[] TrafficVehicle;

    private int lastUsedLane = -1;

    [SerializeField] private float spawnSafeDistance = 50f;
    private Transform playerTransform;

    [SerializeField]private float minspawnTime=30f;
    [SerializeField]private float maxspawnTime=60f;

    [SerializeField]CarController carController;
    // Start is called before the first frame update
    void Start()
    {
        playerTransform = carController.transform;
        StartCoroutine(TrafficSpawner());
    }

    // Update is called once per frame
    IEnumerator TrafficSpawner()
    {
        yield return new WaitForSeconds(2f);
        while (true)
         {
        
            // float dynamicTime=Random.Range(minspawnTime,maxspawnTime)/carController.CarSpeed();

            // if(carController.CarSpeed()>20f)
            // {
            //     SpawnTrafficVehicle();
            // }
           
            // yield return new WaitForSeconds(dynamicTime);

            if(carController.CarSpeed()>20f && carController!=null)
            {
                SpawnTrafficVehicle();

                // Calculate wait time (inverse relationship with speed)
                float currentSpeed = carController.CarSpeed();
                float dynamicTime = Random.Range(minspawnTime, maxspawnTime) / currentSpeed;

                // Add minimum wait time to prevent too-frequent spawning
                dynamicTime = Mathf.Max(dynamicTime, 0.5f);

                yield return new WaitForSeconds(dynamicTime);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }

        
        }
    }

    void SpawnTrafficVehicle()
    {
        //  int RandomLane = Random.Range(0, Lane.Length);
        // int RandomVehicle = Random.Range(0, TrafficVehicle.Length);
        // Instantiate(TrafficVehicle[RandomVehicle], Lane[RandomLane].position, Quaternion.identity);

         int RandomLane;
        
        // Ensure different lane from last spawn
        do
        {
            RandomLane = Random.Range(0, Lane.Length);
        } while (RandomLane == lastUsedLane && Lane.Length > 1);
        
        lastUsedLane = RandomLane;
        
        // Only spawn if far enough from player
        Vector3 spawnPos = Lane[RandomLane].position;
        if (playerTransform != null && Vector3.Distance(spawnPos, playerTransform.position) < spawnSafeDistance)
        {
            Debug.Log("⚠ Vehicle spawn blocked - too close to player (distance: " + 
                Vector3.Distance(spawnPos, playerTransform.position).ToString("F1") + "m)");
            return;
        }
        
        int RandomVehicle = Random.Range(0, TrafficVehicle.Length);
        Instantiate(TrafficVehicle[RandomVehicle], spawnPos, Quaternion.identity);
    }
    
}
