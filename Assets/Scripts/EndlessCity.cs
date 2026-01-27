using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessCity : MonoBehaviour
{
    [SerializeField]private Transform otherCityTransform;
    [SerializeField]private Transform playerCarTransform;
    [SerializeField]private float HalflengthOfCity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCarTransform.position.z > transform.position.z + HalflengthOfCity + 10f)
        {
            transform.position=new Vector3(0,0,otherCityTransform.position.z+HalflengthOfCity*2);
        }
        else if (playerCarTransform.position.z < transform.position.z - HalflengthOfCity - 10f)
        {
            transform.position = new Vector3(0, 0, otherCityTransform.position.z - HalflengthOfCity * 2);
        }
    }
}
