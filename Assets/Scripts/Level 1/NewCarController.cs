using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NewCarController : MonoBehaviour
{
    private const float BaseDrag = 0.05f;

    [Header("Car Settings")]
    public float motorForce = 2000f;
    public float brakeForce = 4000f;
    public float maxSteerAngle = 30f;
    public float reverseForce = 1200f;
    public float steeringResponse = 8f;
    public float steeringReturnSpeed = 10f;
    public float powerSteeringStrength = 2.5f;
    public float movingBrakeSpeedThreshold = 1.5f;
    public float extraBrakeDrag = 1.5f;
    public float lateralStability = 3f;
    public float coastingDrag = 0.35f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;
    

    [Header("Wheel Transforms")]
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isBraking;
    private bool isPressingForward;
    private bool isPressingReverse;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.mass = 1200f;
        rb.drag = BaseDrag;
        rb.angularDrag = 0.5f;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        GetInput();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();
        ApplyPowerSteering();
        UpdateWheels();
    }

    void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
        isPressingForward = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        isPressingReverse = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
    }

    void HandleMotor()
    {
        float torque = 0f;
        float forwardSpeed = GetForwardSpeed();

        if (verticalInput > 0f)
        {
            torque = verticalInput * motorForce;
        }
        else if (verticalInput < 0f && forwardSpeed <= movingBrakeSpeedThreshold)
        {
            torque = verticalInput * reverseForce;
        }

        frontLeftWheel.motorTorque = 0f;
        frontRightWheel.motorTorque = 0f;
        rearLeftWheel.motorTorque = torque;
        rearRightWheel.motorTorque = torque;
    }

    void HandleSteering()
    {
        float targetSteerAngle = maxSteerAngle * horizontalInput;
        float steerSpeed = Mathf.Abs(horizontalInput) > 0.01f ? steeringResponse : steeringReturnSpeed;
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, steerSpeed * maxSteerAngle * Time.fixedDeltaTime);

        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    void HandleBraking()
    {
        float forwardSpeed = GetForwardSpeed();
        bool shouldBrakeForReverse = isPressingReverse && forwardSpeed > movingBrakeSpeedThreshold;
        bool shouldBrakeForForward = isPressingForward && forwardSpeed < -movingBrakeSpeedThreshold;
        bool shouldApplyCoastingDrag = Mathf.Abs(verticalInput) < 0.01f && Mathf.Abs(forwardSpeed) > 0.1f;

        currentBrakeForce = (isBraking || shouldBrakeForReverse || shouldBrakeForForward) ? brakeForce : 0f;
        rb.drag = (isBraking || shouldBrakeForReverse || shouldBrakeForForward) ? extraBrakeDrag : (shouldApplyCoastingDrag ? coastingDrag : BaseDrag);
        ApplyBraking();
    }

    void ApplyBraking()
    {
        frontLeftWheel.brakeTorque = currentBrakeForce;
        frontRightWheel.brakeTorque = currentBrakeForce;
        rearLeftWheel.brakeTorque = currentBrakeForce;
        rearRightWheel.brakeTorque = currentBrakeForce;
    }

    void ApplyPowerSteering()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        Vector3 stabilizingForce = -transform.right * localVelocity.x * lateralStability;
        rb.AddForce(stabilizingForce, ForceMode.Acceleration);

        if (Mathf.Abs(horizontalInput) > 0.01f) return;
        if (Mathf.Abs(localVelocity.z) < 0.1f) return;

        rb.AddRelativeTorque(0f, -localVelocity.x * powerSteeringStrength, 0f, ForceMode.Acceleration);
    }

    float GetForwardSpeed()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        return localVelocity.z;
    }

    void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftTransform);
        UpdateSingleWheel(frontRightWheel, frontRightTransform);
        UpdateSingleWheel(rearLeftWheel, rearLeftTransform);
        UpdateSingleWheel(rearRightWheel, rearRightTransform);
    }

    void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        if (wheelCollider == null || wheelTransform == null) return;

        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    public float CarSpeed()
    {
        return rb.velocity.magnitude * 3.6f;
    }
}
