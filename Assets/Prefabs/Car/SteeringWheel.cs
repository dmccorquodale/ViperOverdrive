using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheelController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform wheelTransform;
    [SerializeField] private Vector3 localSpinAxis = new Vector3(0,0,1);
    [SerializeField] private bool invertVisual = false;

    [Header("Limits")]
    [SerializeField] private float maxWheelAngle = 200f;                // deg, each side

    [Header("Physics Feel (bus wheel)")]
    [SerializeField] private float momentOfInertia = 1500f;             // HUGE inertia
    [SerializeField] private float inputDamping = 0.6f;                 // damping while being torqued
    [SerializeField] private float freeSpinDamping = 0.02f;             // damping while free-spinning (tiny)
    [SerializeField] private float centerSpring = 0.0f;                 // no centering
    [SerializeField] private float stopFriction = 2f;                   // very light near hard stop
    [SerializeField] private float maxAngVel = 30000f;                  // deg/s safety cap

    [Header("Player Torque (keys/gamepad)")]
    [SerializeField] private float torquePerInput = 600f;               // optional; used if steerAxis present

    [Header("Visual Smoothing")]
    [SerializeField] private float rotateSmooth = 20f;

    [Header("Input (New Input System)")]
    public InputActionReference steerAxis;

    [Header("Mouse Drag")]
    [SerializeField] private Camera inputCamera;
    [SerializeField] private LayerMask wheelMask = ~0;
    [SerializeField] private float dragDegreesPerPixel = 0.5f;          // stronger coupling for flicks
    [SerializeField, Range(0f,1f)] private float releaseInertia = 1f;   // keep all velocity on release
    [SerializeField] private bool invertDrag = false;

    [Header("Fling Tuning (arcade)")]
    [SerializeField] private float injectGainWhileDragging = 0.75f;     // adds to angVel during drag
    [SerializeField] private float flingGain = 40f;                     // big boost from recent swipe speed
    [SerializeField] private float flingAverageTime = 0.06f;            // seconds of mouse history

    private bool dragging;
    private float angle;            // deg
    private float visualAngle;      // deg
    private float angVel;           // deg/s
    private Quaternion initialLocalRot;

    // mouse history for fling
    private float recentDragAngleAccum;  // deg
    private float recentDragTime;        // s

    private void Awake()
    {
        if (!wheelTransform) wheelTransform = transform;
        if (!inputCamera) inputCamera = Camera.main;
        initialLocalRot = wheelTransform.localRotation;

        if (localSpinAxis.sqrMagnitude < 1e-6f) localSpinAxis = Vector3.forward;
        localSpinAxis = localSpinAxis.normalized;

        momentOfInertia = Mathf.Max(0.01f, momentOfInertia);
        maxAngVel = Mathf.Max(1000f, maxAngVel);
    }

    private void OnEnable()  => steerAxis?.action?.Enable();
    private void OnDisable() => steerAxis?.action?.Disable();

    private void Update()
    {
        float input = steerAxis ? steerAxis.action.ReadValue<float>() : 0f;
        input = 0f;
        float dt = Time.deltaTime;

        // ---------- Mouse drag & fling capture ----------
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && IsPointerOverWheel())
            {
                dragging = true;
                angVel = 0f; // ← stop all momentum the instant you grab it
                recentDragAngleAccum = 0f;
                recentDragTime = 0f;
            }

            if (dragging)
            {
                float dx = Mouse.current.delta.ReadValue().x;
                float dragVisualSign = (invertDrag ? -1f : 1f) * (invertVisual ? -1f : 1f);

                float deltaAngle = dx * dragDegreesPerPixel * dragVisualSign; // deg this frame
                angle += deltaAngle;

                // add to velocity (stack momentum) — the magic sauce
                if (dt > 0f) angVel += (deltaAngle / dt) * injectGainWhileDragging;

                // accumulate short history to estimate fling velocity
                recentDragAngleAccum += deltaAngle;
                recentDragTime += dt;
                if (recentDragTime > flingAverageTime)
                {
                    // keep only the latest window proportionally
                    float keep = Mathf.Clamp01((flingAverageTime - (recentDragTime - dt)) / recentDragTime);
                    recentDragAngleAccum *= keep;
                    recentDragTime = flingAverageTime;
                }
            }

            if (dragging && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                dragging = false;

                // fling velocity from recent swipe
                if (recentDragTime > 0.0001f)
                {
                    float recentVel = (recentDragAngleAccum / recentDragTime); // deg/s
                    angVel += recentVel * flingGain * Mathf.Clamp01(releaseInertia);
                }

                // clear window
                recentDragAngleAccum = 0f;
                recentDragTime = 0f;
            }
        }

        // ---------- Dynamics ----------
        // Choose damping based on state
        float dampingNow = dragging ? inputDamping : freeSpinDamping;

        // Input torque (keys/gamepad), scaled by inertia
        float torque = input * torquePerInput;

        // Damping torque (viscous)
        float dampingTorque = -angVel * dampingNow * momentOfInertia;

        // Centering spring (usually 0 for bus feel)
        float springTorque  = -angle * centerSpring;

        // Hard-stop friction (only near ends; very light)
        float stop = Mathf.InverseLerp(maxWheelAngle * 0.9f, maxWheelAngle, Mathf.Abs(angle));
        float stopTorque = -Mathf.Sign(angVel) * stop * stopFriction * momentOfInertia * Mathf.Abs(angVel);

        // Sum torques -> angular acceleration
        float totalTorque = torque + dampingTorque + springTorque + stopTorque;
        float angAcc = totalTorque / momentOfInertia; // deg/s^2

        angVel += angAcc * dt;

        // Cap velocity for sanity
        angVel = Mathf.Clamp(angVel, -maxAngVel, maxAngVel);

        angle += angVel * dt;

        // clamp angle to limits and kill velocity if pushing further outward
        if (Mathf.Abs(angle) > maxWheelAngle)
        {
            angle = Mathf.Sign(angle) * maxWheelAngle;
            if (Mathf.Sign(angVel) == Mathf.Sign(angle)) angVel = 0f;
        }

        // ---------- Visuals ----------
        visualAngle = Mathf.Lerp(visualAngle, angle, 1f - Mathf.Exp(-rotateSmooth * dt));
        float visualSign = invertVisual ? -1f : 1f;
        Quaternion rot = Quaternion.AngleAxis(visualSign * visualAngle, localSpinAxis);
        wheelTransform.localRotation = initialLocalRot * rot;
    }

    private bool IsPointerOverWheel()
    {
        if (!inputCamera || Mouse.current == null) return false;
        return Physics.Raycast(inputCamera.ScreenPointToRay(Mouse.current.position.ReadValue()),
                               out var _, 1000f, wheelMask);
    }

    // Car-facing outputs
    public float Steering01   => Mathf.InverseLerp(-maxWheelAngle, maxWheelAngle, angle);
    public float SteeringSign => Mathf.Clamp(angle / maxWheelAngle, -1f, 1f);
}