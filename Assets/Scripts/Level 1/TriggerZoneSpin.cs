using UnityEngine;

public class TriggerZoneSpin : MonoBehaviour
{
    public float spinSpeed = 60f;
    // 60 = one full rotation per second

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
}