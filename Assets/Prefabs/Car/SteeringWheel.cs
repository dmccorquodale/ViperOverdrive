using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    [SerializeField] private float steeringSpeed = 2f;    // How fast the wheel moves
    [SerializeField] private float maxRotation = 180f;    // Max rotation in degrees
    private float steering = 0.5f;                        // Normalized steering (0=full left,1=full right)

    void Update()
    {
        float input = 0f;

        // Keyboard input
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            input = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            input = 1f;

        // Update normalized steering
        steering += input * steeringSpeed * Time.deltaTime;
        steering = Mathf.Clamp01(steering);

        // Rotate the wheel mesh based on steering
        float wheelRotation = Mathf.Lerp(-maxRotation/2f, maxRotation/2f, steering);
        transform.localRotation = Quaternion.Euler(0f, 0f, wheelRotation);
    }
}