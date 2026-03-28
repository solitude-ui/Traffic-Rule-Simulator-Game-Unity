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

    [Header("Aerodynamics")]
    public float downforceCoefficient = 1.5f;

    [Header("High Speed Stability")]
    public float minAngularDrag = 0.5f;
    public float maxAngularDrag = 8f;
    public float stabilityMaxSpeed = 200f;
    public float minHighSpeedSteerAngle = 10f;

    [Header("Anti Roll Bar")]
    public float antiRollForce = 8000f;

    [Header("Wheel Friction")]
    public float baseSidewaysStiffness = 1f;
    public float highSpeedSidewaysStiffness = 2.5f;

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
        ApplyDownforce();
        UpdateAngularDrag();
        UpdateWheelFriction();
        ApplyAntiRollBar(frontLeftWheel, frontRightWheel);
        ApplyAntiRollBar(rearLeftWheel, rearRightWheel);
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
        float speed = CarSpeed();
        float speedFactor = GetSpeedFactor(speed);
        float effectiveSteerAngle = Mathf.Lerp(maxSteerAngle, minHighSpeedSteerAngle, speedFactor);
        float targetSteerAngle = effectiveSteerAngle * horizontalInput;
        float steerSpeed = Mathf.Abs(horizontalInput) > 0.01f ? steeringResponse : steeringReturnSpeed;
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, steerSpeed * effectiveSteerAngle * Time.fixedDeltaTime);

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
        float speed = CarSpeed();
        float speedStabilityMultiplier = 1f + GetSpeedFactor(speed) * 2f;
        Vector3 stabilizingForce = -transform.right * localVelocity.x * lateralStability * speedStabilityMultiplier;
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

    void ApplyDownforce()
    {
        float speed = CarSpeed();
        rb.AddForce(-transform.up * downforceCoefficient * speed * speed, ForceMode.Force);
    }

    void UpdateAngularDrag()
    {
        float speed = CarSpeed();
        rb.angularDrag = Mathf.Lerp(minAngularDrag, maxAngularDrag, GetSpeedFactor(speed));
    }

    void UpdateWheelFriction()
    {
        float speed = CarSpeed();
        float speedFactor = GetSpeedFactor(speed);
        float sidewaysStiffness = Mathf.Lerp(baseSidewaysStiffness, highSpeedSidewaysStiffness, speedFactor);

        UpdateSingleWheelFriction(frontLeftWheel, sidewaysStiffness);
        UpdateSingleWheelFriction(frontRightWheel, sidewaysStiffness);
        UpdateSingleWheelFriction(rearLeftWheel, sidewaysStiffness);
        UpdateSingleWheelFriction(rearRightWheel, sidewaysStiffness);
    }

    void UpdateSingleWheelFriction(WheelCollider wheelCollider, float sidewaysStiffness)
    {
        if (wheelCollider == null) return;

        WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
        sidewaysFriction.stiffness = sidewaysStiffness;
        wheelCollider.sidewaysFriction = sidewaysFriction;
    }

    void ApplyAntiRollBar(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        if (leftWheel == null || rightWheel == null) return;

        float leftTravel = 1f;
        float rightTravel = 1f;

        bool leftGrounded = leftWheel.GetGroundHit(out WheelHit leftHit);
        if (leftGrounded)
        {
            leftTravel = (-leftWheel.transform.InverseTransformPoint(leftHit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        }

        bool rightGrounded = rightWheel.GetGroundHit(out WheelHit rightHit);
        if (rightGrounded)
        {
            rightTravel = (-rightWheel.transform.InverseTransformPoint(rightHit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        float antiRoll = (leftTravel - rightTravel) * antiRollForce;

        if (leftGrounded)
        {
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRoll, leftWheel.transform.position, ForceMode.Force);
        }

        if (rightGrounded)
        {
            rb.AddForceAtPosition(rightWheel.transform.up * antiRoll, rightWheel.transform.position, ForceMode.Force);
        }
    }

    float GetSpeedFactor(float speed)
    {
        if (stabilityMaxSpeed <= 0f) return 1f;
        return Mathf.Clamp01(speed / stabilityMaxSpeed);
    }

    public float CarSpeed()
    {
        return rb.velocity.magnitude * 3.6f;
    }
}
