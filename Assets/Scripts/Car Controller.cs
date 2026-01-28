using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;   
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    

    public float motorForce = 1500f;

    public float breakForce = 1000f;

    public Transform centerOfMassTransform;

    public Rigidbody carRigidbody;
    

    float verticalInput;

    float horizontalInput;
    // Start is called before the first frame update
    void Start()
    {
        carRigidbody.centerOfMass = centerOfMassTransform.localPosition;
    }
    void Update()
    {
          GetInput();
           Debug.Log("Vertical Input: " + verticalInput);
              Debug.Log("Horizontal Input: " + horizontalInput);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // GetInput();
        MotorForce();
        updateWheels();
        Steering();
        ApplyBreaks();
        PowerSteering();
        Debug.Log(CarSpeed());
       
    }

    void MotorForce()
    {

        frontLeftWheel.motorTorque = motorForce * verticalInput;
        frontRightWheel.motorTorque = motorForce * verticalInput;
    //     rearLeftWheel.motorTorque = motorForce;
    //     rearRightWheel.motorTorque = motorForce;
    }

     void updateWheels()
    {
        RotateWheels(frontLeftWheel, frontLeftWheelTransform);
        RotateWheels(frontRightWheel, frontRightWheelTransform);
        RotateWheels(rearLeftWheel, rearLeftWheelTransform);
        RotateWheels(rearRightWheel, rearRightWheelTransform);
    }
    void RotateWheels(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelTransform.position = position;
        wheelTransform.rotation = rotation;
    }

    void GetInput()
    {
     verticalInput = Input.GetAxis("Vertical");
     horizontalInput = Input.GetAxis("Horizontal");
    // verticalInput = Input.GetKey(KeyCode.W) ? 1f : (Input.GetKey(KeyCode.S) ? -1f : 0f);
    // horizontalInput = Input.GetKey(KeyCode.D) ? 1f : (Input.GetKey(KeyCode.A) ? -1f : 0f);
    // Debug.Log($"Vertical: {verticalInput}, Horizontal: {horizontalInput}");
   
    }

    void Steering()
    {
        float steeringAngle = 30f;
        frontLeftWheel.steerAngle = steeringAngle * horizontalInput;
        frontRightWheel.steerAngle = steeringAngle * horizontalInput;
    }

    void ApplyBreaks()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            frontRightWheel.brakeTorque=breakForce;
            frontLeftWheel.brakeTorque=breakForce;
            rearLeftWheel.brakeTorque=breakForce;
            rearRightWheel.brakeTorque=breakForce;
            carRigidbody.drag=1f;
        }
        else
        {
            frontRightWheel.brakeTorque=0f;
            frontLeftWheel.brakeTorque=0f;
            rearLeftWheel.brakeTorque=0f;
            rearRightWheel.brakeTorque=0f;
            carRigidbody.drag=0f;
        }
    }

    void PowerSteering()
    {
        if (horizontalInput == 0)
        {
            transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.Euler(0,0,0),Time.deltaTime*2);
        }
    }

    public float CarSpeed()
    {
        float speed=carRigidbody.velocity.magnitude*2.23693629f;
        return speed;
    }

}


