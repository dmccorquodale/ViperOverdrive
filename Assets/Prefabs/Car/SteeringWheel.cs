using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheelController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform wheelTransform;                  // rotates this; defaults to self
    [SerializeField] private Vector3 localSpinAxis = new Vector3(0,0,1);// local axis to spin around
    [SerializeField] private bool invert = false;

    [Header("Limits")]
    [SerializeField] private float maxWheelAngle = 200f;   // deg, each side

    [Header("Feel (make it cumbersome)")]
    [SerializeField] private float torquePerInput = 600f;  // deg/s^2 for input = 1
    [SerializeField] private float damping = 4f;           // viscous friction; higher = heavier
    [SerializeField] private float centerSpring = 0.0f;    // 0 = no snap-back. Try 0..2 for weak return
    [SerializeField] private float stopFriction = 20f;     // extra braking near the hard stop

    [Header("Visual Smoothing")]
    [SerializeField] private float rotateSmooth = 20f;

    [Header("Input (New Input System)")]
    public InputActionReference steerAxis; // -1..1

    // runtime
    private float angle;            // current angle in deg (-max..+max)
    private float visualAngle;      // smoothed for rendering
    private float angVel;           // deg/s
    private Quaternion initialLocalRot;

    private void Awake()
    {
        if (!wheelTransform) wheelTransform = transform;
        initialLocalRot = wheelTransform.localRotation;

        if (localSpinAxis.sqrMagnitude < 1e-6f) localSpinAxis = Vector3.forward;
        localSpinAxis = localSpinAxis.normalized;
    }

    private void OnEnable()  { steerAxis?.action?.Enable(); }
    private void OnDisable() { steerAxis?.action?.Disable(); }

    private void Update()
    {
        float input = steerAxis ? steerAxis.action.ReadValue<float>() : 0f; // -1..1

        // ---- Physics-y integration ----
        float dt = Time.deltaTime;

        // 1) Apply input torque
        float torque = input * torquePerInput;

        // 2) Damping (friction opposing motion)
        float dampingTorque = -angVel * damping;

        // 3) Optional weak center spring (pulls to 0)
        float springTorque = -angle * centerSpring;

        // 4) Hard-stop friction near limits
        float stop = Mathf.InverseLerp(maxWheelAngle * 0.8f, maxWheelAngle, Mathf.Abs(angle));
        float stopTorque = -Mathf.Sign(angVel) * stop * stopFriction * Mathf.Abs(angVel);

        // Sum torques → angular acceleration
        float angAcc = torque + dampingTorque + springTorque + stopTorque;

        // Integrate
        angVel += angAcc * dt;
        angle  += angVel * dt;

        // Clamp angle and kill velocity when pinned at the stop
        if (Mathf.Abs(angle) > maxWheelAngle)
        {
            angle = Mathf.Sign(angle) * maxWheelAngle;
            if (Mathf.Sign(angVel) == Mathf.Sign(angle)) angVel = 0f;
        }

        // ---- Visuals ----
        visualAngle = Mathf.Lerp(visualAngle, angle, 1f - Mathf.Exp(-rotateSmooth * dt));
        float s = invert ? -1f : 1f;
        Quaternion rot = Quaternion.AngleAxis(s * visualAngle, localSpinAxis);
        wheelTransform.localRotation = initialLocalRot * rot;
    }

    // Outputs for the car
    public float Steering01 => Mathf.InverseLerp(-maxWheelAngle, maxWheelAngle, angle);
    public float SteeringSign => Mathf.Clamp(angle / maxWheelAngle, -1f, 1f);
}