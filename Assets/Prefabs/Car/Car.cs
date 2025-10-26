using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    public GameObject steeringWheel;
    public AudioSource carEngineAduio;

    public TMPro.TextMeshProUGUI timerText;
    public TMPro.TextMeshProUGUI speedText;
    public float speedUpdateDelayByFrames = 60f;
    private float updateSpeedCounter = 0f;
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
        //steeringInput = move.ReadValue<Vector2>().x;
        steeringInput = steeringWheel.GetComponent<SteeringWheelController>().SteeringSign;

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

        UpdateSpeedText();
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

    public void UpdateTimer(float time)
    {
        if (time > 0)
        {
            int hours = Mathf.FloorToInt(time / 3600f);
            int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
            timerText.text = $"{hours:00}:{minutes:00}:{seconds:00}:{milliseconds:000}";
        }
    }

    public void UpdateSpeedText()
    {
        Debug.Log(updateSpeedCounter);
        if (updateSpeedCounter >= speedUpdateDelayByFrames)
        {
            speedText.text = $"KMs: {currentSpeedKmH:00}";
            updateSpeedCounter = 0f;
        } else
        {
            updateSpeedCounter += 1f;
        }
    }
}
