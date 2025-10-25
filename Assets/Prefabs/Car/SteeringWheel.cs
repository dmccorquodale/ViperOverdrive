using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheelController : MonoBehaviour
{
    [Header("Wheel")]
    [SerializeField] private float maxWheelAngle = 180f;   // lock-to-lock/2 (e.g., 180 = 360° total)
    [SerializeField] private float returnSpeed = 360f;     // deg/sec back to center when no input
    [SerializeField] private float rotateSmooth = 20f;     // visual smoothing
    [SerializeField] private float mouseSensitivity = 0.25f; // how fast drag = degrees

    [Header("Input (New Input System)")]
    [Tooltip("Float axis -1..1 (e.g., A/D, Left/Right, gamepad stick X). Action type: Value, Control type: Axis")]
    public InputActionReference steerAxis;
    [Tooltip("Pointer delta. Action type: Value, Control type: Vector2, binding: Pointer/Delta")]
    public InputActionReference pointerDelta;
    [Tooltip("Pointer press. Action type: Button, binding: Mouse/LeftButton (or your choice)")]
    public InputActionReference pointerPress;

    private float targetAngle;           // desired angle (deg, -max..+max)
    private float currentAngle;          // smoothed visual angle
    private Quaternion initialLocalRot;  // to preserve original orientation

    public float Steering01 => Mathf.InverseLerp(-maxWheelAngle, maxWheelAngle, currentAngle);
    public float SteeringSigned => Mathf.InverseLerp(-maxWheelAngle, maxWheelAngle, currentAngle) * 2f - 1f;

    private void Awake()
    {
        initialLocalRot = transform.localRotation;
    }

    private void OnEnable()
    {
        steerAxis?.action?.Enable();
        pointerDelta?.action?.Enable();
        pointerPress?.action?.Enable();
    }

    private void OnDisable()
    {
        steerAxis?.action?.Disable();
        pointerDelta?.action?.Disable();
        pointerPress?.action?.Disable();
    }

    private void Update()
    {
        // 1) Keyboard/Gamepad axis (-1..1)
        float axis = steerAxis != null ? steerAxis.action.ReadValue<float>() : 0f;

        // Map axis directly to a target angle
        float axisTarget = axis * maxWheelAngle;

        // 2) Mouse drag (only while pressed)
        if (pointerPress != null && pointerPress.action.IsPressed() && pointerDelta != null)
        {
            Vector2 d = pointerDelta.action.ReadValue<Vector2>();
            // Horizontal drag adds/subtracts degrees
            targetAngle += d.x * mouseSensitivity;
        }
        else
        {
            // When not dragging, target follows the axis input
            targetAngle = Mathf.MoveTowards(targetAngle, axisTarget, returnSpeed * Time.deltaTime);
        }

        // Clamp and smooth
        targetAngle = Mathf.Clamp(targetAngle, -maxWheelAngle, +maxWheelAngle);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, 1f - Mathf.Exp(-rotateSmooth * Time.deltaTime));

        // Apply rotation around the wheel’s local forward axis (Z)
        transform.localRotation = initialLocalRot * Quaternion.Euler(0f, 0f, -currentAngle);
    }
}