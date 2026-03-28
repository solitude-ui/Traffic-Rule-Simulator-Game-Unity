using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTrafficManager : MonoBehaviour
{
    [SerializeField] private Transform[] Lane;
    [SerializeField] private GameObject[] TrafficVehicle;
    [SerializeField] private float minspawnTime = 30f;
    [SerializeField] private float maxspawnTime = 60f;
    [SerializeField] private float spawnSafeDistance = 50f; // Check this distance for other vehicles
    [SerializeField] private CarController carController;

    private int lastUsedLane = -1;

    void Start()
    {
        StartCoroutine(TrafficSpawner());
    }

    IEnumerator TrafficSpawner()
    {
        yield return new WaitForSeconds(2f);
        
        while (true)
        {
            if (carController != null && carController.CarSpeed() > 20f)
            {
                // Try to spawn, if successful wait based on speed
                if (TrySpawnVehicle())
                {
                    float dynamicTime = Random.Range(minspawnTime, maxspawnTime) / carController.CarSpeed();
                    dynamicTime = Mathf.Max(dynamicTime, 0.5f);
                    yield return new WaitForSeconds(dynamicTime);
                }
                else
                {
                    // Couldn't spawn, try again soon
                    yield return new WaitForSeconds(0.3f);
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    bool TrySpawnVehicle()
    {
        // Find a safe lane
        List<int> safeLanes = new List<int>();
        
        for (int i = 0; i < Lane.Length; i++)
        {
            if (IsLaneSafe(i))
            {
                safeLanes.Add(i);
            }
        }
        
        // No safe lanes available
        if (safeLanes.Count == 0)
            return false;
        
        // Pick a lane (prefer different from last)
        int chosenLane;
        if (safeLanes.Count > 1 && safeLanes.Contains(lastUsedLane))
        {
            safeLanes.Remove(lastUsedLane);
        }
        chosenLane = safeLanes[Random.Range(0, safeLanes.Count)];
        
        // Spawn vehicle
        lastUsedLane = chosenLane;
        int randomVehicle = Random.Range(0, TrafficVehicle.Length);
        Instantiate(TrafficVehicle[randomVehicle], Lane[chosenLane].position, Quaternion.identity);
        
        return true;
    }

    bool IsLaneSafe(int laneIndex)
    {
        Vector3 spawnPos = Lane[laneIndex].position;
        
        // Check if any vehicle is near the spawn point
        Collider[] nearby = Physics.OverlapSphere(spawnPos, spawnSafeDistance);
        
        foreach (Collider col in nearby)
        {
            if (col.CompareTag("TrafficVehicle") || col.CompareTag("Player"))
            {
                return false; // Lane not safe
            }
        }
        
        return true; // Lane is safe
    }
}