using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheelController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform wheelTransform;                  // rotates this; defaults to self
    [SerializeField] private Vector3 localSpinAxis = new Vector3(0,0,1);// local axis to spin around
    [SerializeField] private bool invertVisual = false;                 // flip visual only

    [Header("Limits")]
    [SerializeField] private float maxWheelAngle = 200f;                // deg, each side

    [Header("Feel (make it cumbersome)")]
    [SerializeField] private float torquePerInput = 600f;               // deg/s^2 for input = 1
    [SerializeField] private float damping = 4f;                        // viscous friction; higher = heavier
    [SerializeField] private float centerSpring = 0.0f;                 // 0 = no snap-back
    [SerializeField] private float stopFriction = 20f;                  // extra braking near the hard stop

    [Header("Visual Smoothing")]
    [SerializeField] private float rotateSmooth = 20f;

    [Header("Input (New Input System)")]
    public InputActionReference steerAxis;                              // -1..1

    [Header("Mouse Drag")]
    [SerializeField] private Camera inputCamera;                        // if null, uses Camera.main
    [SerializeField] private LayerMask wheelMask = ~0;                  // set to wheel layer or leave Everything
    [SerializeField] private float dragDegreesPerPixel = 0.25f;         // horizontal pixels -> degrees
    [SerializeField, Range(0f,1f)] private float releaseInertia = 1f;   // how much angVel to keep on release
    [SerializeField] private bool invertDrag = false;                   // flip mouse drag direction
    [SerializeField] private bool invertKeys = false;                   // flip keyboard direction

    private bool dragging;

    // runtime
    private float angle;            // current angle in deg (-max..+max)
    private float visualAngle;      // smoothed for rendering
    private float angVel;           // deg/s
    private Quaternion initialLocalRot;

    private void Awake()
    {
        if (!wheelTransform) wheelTransform = transform;
        if (!inputCamera) inputCamera = Camera.main;
        initialLocalRot = wheelTransform.localRotation;

        if (localSpinAxis.sqrMagnitude < 1e-6f) localSpinAxis = Vector3.forward;
        localSpinAxis = localSpinAxis.normalized;
    }

    private void OnEnable()  => steerAxis?.action?.Enable();
    private void OnDisable() => steerAxis?.action?.Disable();

    private void Update()
    {
        float input = steerAxis ? steerAxis.action.ReadValue<float>() : 0f;
        if (invertKeys) input = -input;

        float dt = Time.deltaTime;

        // ---------- Mouse drag ----------
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                dragging = IsPointerOverWheel();

            if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                dragging = false;
                angVel *= Mathf.Clamp01(releaseInertia);
            }

            if (dragging)
            {
                float dx = Mouse.current.delta.ReadValue().x;
                float dragSign = invertDrag ? -1f : 1f;
                angle += dx * dragDegreesPerPixel * dragSign;
            }
        }

        // ---- “cumbersome” steering physics ----
        float torque = input * torquePerInput;
        float dampingTorque = -angVel * damping;
        float springTorque  = -angle * centerSpring;

        float stop = Mathf.InverseLerp(maxWheelAngle * 0.8f, maxWheelAngle, Mathf.Abs(angle));
        float stopTorque = -Mathf.Sign(angVel) * stop * stopFriction * Mathf.Abs(angVel);

        float angAcc = torque + dampingTorque + springTorque + stopTorque;
        angVel += angAcc * dt;
        angle  += angVel * dt;

        // clamp + pin
        if (Mathf.Abs(angle) > maxWheelAngle)
        {
            angle = Mathf.Sign(angle) * maxWheelAngle;
            if (Mathf.Sign(angVel) == Mathf.Sign(angle)) angVel = 0f;
        }

        // ---- visuals ----
        visualAngle = Mathf.Lerp(visualAngle, angle, 1f - Mathf.Exp(-rotateSmooth * dt));
        float visualSign = invertVisual ? -1f : 1f;
        Quaternion rot = Quaternion.AngleAxis(visualSign * visualAngle, localSpinAxis);
        wheelTransform.localRotation = initialLocalRot * rot;
    }

    private bool IsPointerOverWheel()
    {
        if (!inputCamera || Mouse.current == null) return false;
        if (!Physics.Raycast(inputCamera.ScreenPointToRay(Mouse.current.position.ReadValue()),
                             out var hit, 1000f, wheelMask))
            return false;

        return hit.transform == wheelTransform || hit.transform.IsChildOf(wheelTransform);
    }

    // Outputs for the car
    public float Steering01   => Mathf.InverseLerp(-maxWheelAngle, maxWheelAngle, angle);
    public float SteeringSign => Mathf.Clamp(angle / maxWheelAngle, -1f, 1f);
}