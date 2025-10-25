using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f; // degrees per second

    void Update()
    {
        float input = 0f;

        // Keyboard input
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            input = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            input = 1f;

        // Mouse input (optional)
        float mouseInput = Input.GetAxis("Mouse X"); // -1 to 1
        input += mouseInput;

        // Only rotate if there is input
        if (Mathf.Abs(input) > 0.01f)
        {
            transform.Rotate(0f, 0f, input * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}