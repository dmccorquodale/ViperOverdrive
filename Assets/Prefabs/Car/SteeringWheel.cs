using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheelController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Which Transform to rotate. Leave empty to use this object.")]
    [SerializeField] private Transform wheelTransform;

    [Tooltip("Local axis to spin around (in the target's LOCAL space). Try (0,0,1) or (0,1,0) etc.")]
    [SerializeField] private Vector3 localSpinAxis = new Vector3(0, 0, 1); // Z by default

    [SerializeField] private bool invert;
    [SerializeField] private float maxWheelAngle = 180f;
    [SerializeField] private float rotateSmooth = 20f;

    [Header("Input")]
    public InputActionReference steerAxis; // Value/Axis (-1..1)

    private float targetAngle, currentAngle;
    private Quaternion initialLocalRot;

    private void Awake()
    {
        if (wheelTransform == null) wheelTransform = transform;
        initialLocalRot = wheelTransform.localRotation;

        if (localSpinAxis.sqrMagnitude < 1e-6f) localSpinAxis = Vector3.forward; // safety
        localSpinAxis = localSpinAxis.normalized;
    }

    private void OnEnable()  { steerAxis?.action?.Enable(); }
    private void OnDisable() { steerAxis?.action?.Disable(); }

    private void Update()
    {
        float axis = steerAxis ? steerAxis.action.ReadValue<float>() : 0f;
        targetAngle = Mathf.Clamp(axis * maxWheelAngle, -maxWheelAngle, maxWheelAngle);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, 1f - Mathf.Exp(-rotateSmooth * Time.deltaTime));

        float sign = invert ? -1f : 1f;
        Quaternion rot = Quaternion.AngleAxis(sign * currentAngle, localSpinAxis);
        wheelTransform.localRotation = initialLocalRot * rot;
    }

    // Outputs for your teammate
    public float Steering01   => Mathf.InverseLerp(-maxWheelAngle, maxWheelAngle, currentAngle);
    public float SteeringSign => Mathf.Clamp(currentAngle / maxWheelAngle, -1f, 1f);
}