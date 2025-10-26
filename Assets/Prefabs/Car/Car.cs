using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    public GameObject steeringWheel;
    public AudioSource carEngineAduio;
    public bool engineOn;
    public GameObject[] suspensions;

    [Header("Engine")]
    public float acceleration;
    public float targetSpeedKmH;
    public float currentSpeedKmH;
    private Rigidbody rb;

    InputAction move;

    [Header("Tyres")]
    public float steeringLock;
    public float tyreForce;
    public float tyreForceClamp;
    public float rearTyreGripFactor;

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

        engineOn = true;
    }

    void Update()
    {
        steeringInput = move.ReadValue<Vector2>().x;
        //steeringInput = steeringWheel.GetComponent<SteeringWheelController>().SteeringSign;

        //throttle = move.ReadValue<Vector2>().y;
        if (engineOn && AreWheelsTouchingGround())
        {
            if (currentSpeedKmH < targetSpeedKmH)
            {
                throttle = 1f;
            }
            else
            {
                throttle = 0f;
            }
        }
        else
        {
            throttle = 0f;
        }

        currentSpeedKmH = rb.linearVelocity.magnitude * 3.6f;

        carEngineAduio.pitch = Mathf.Lerp(0.2f, 2f, currentSpeedKmH / 200f);
    }

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * acceleration * throttle);
    }

    public void Go()
    {
        engineOn = true;
    }

    bool AreWheelsTouchingGround()
    {
        foreach (GameObject i in suspensions)
        {
            if (i.GetComponent<Suspension>().wheelTouchingGround)
            {
                return true;
            }
        }

        return false;
    }
}
