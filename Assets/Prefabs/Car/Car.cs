using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    [Header("Engine")]
    public float acceleration;
    private Rigidbody rb;

    InputAction move;

    [Header("Tyres")]
    public float steeringLock;
    public float tyreForce;
    public float tyreForceClamp;

    [Header("Suspension")]
    public float springStiffness;
    public float damperStiffness;
    public float restLength;
    public float springTravel;

    [Header("Steering Input")]
    public float steeringInput;
    public float throttle;

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
        throttle = move.ReadValue<Vector2>().y;
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * acceleration * throttle);
    }
}
