using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    public float acceleration;
    private Rigidbody rb;

    InputAction move;
    public float steeringInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (InputSystem.actions)
        {
            move = InputSystem.actions.FindAction("Player/Move");
        }
    }

    void Update()
    {
        steeringInput = move.ReadValue<Vector2>().x;
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * acceleration);
    }
}
