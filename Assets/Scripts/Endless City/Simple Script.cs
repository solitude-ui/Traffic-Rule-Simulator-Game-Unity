using UnityEngine;

public class SimpleScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER → " + other.name);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("STAY → " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("EXIT → " + other.name);
    }
}