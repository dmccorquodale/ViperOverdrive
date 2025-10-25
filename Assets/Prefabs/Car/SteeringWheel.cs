using UnityEngine;
using UnityEngine.InputSystem;

public class SteeringWheelController : MonoBehaviour
{
    [SerializeField] private float maxRotation = 180f;
    private float steering = 0.5f;
    public InputAction steeringAction; // Assign in Inspector

    private void OnEnable() => steeringAction.Enable();
    private void OnDisable() => steeringAction.Disable();

    void Update()
    {
        float input = steeringAction.ReadValue<float>();
        steering += input * Time.deltaTime;
        steering = Mathf.Clamp01(steering);

        float wheelRotation = Mathf.Lerp(-maxRotation/2f, maxRotation/2f, steering);
        transform.localRotation = Quaternion.Euler(0f, 0f, wheelRotation);
    }
}