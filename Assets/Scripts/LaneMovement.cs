using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneMovement : MonoBehaviour
{
    // Start is called before the first frame update
   [SerializeField]private Transform PlayerCarTransform;
    [SerializeField] float offset=100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 Cameraposition = transform.position;
        Cameraposition.z = PlayerCarTransform.position.z + offset;
        transform.position = Cameraposition;

    }
}
