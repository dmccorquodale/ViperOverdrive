using UnityEngine;

public class Suspension : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject car;

    public bool wheelTouchingGround;

    public float wheelRadius;
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;
    public GameObject tyre;

    private float maxLength;
    private float minLength;
    private float lastLength;
    private float springForce;
    private float springVelocity;
    private float springLength;
    private float damperForce;

    private Vector3 suspensionForce;

    void Start()
    {
        rb = car.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            wheelTouchingGround = true;

            Debug.DrawRay(transform.position, -transform.up * hit.distance, Color.red);

            lastLength = springLength;

            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);

            springForce = springStiffness * (restLength - springLength);

            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce + damperForce) * transform.up;

            tyre.transform.position = new Vector3(hit.point.x, hit.point.y + wheelRadius, hit.point.z);
            rb.AddForceAtPosition(suspensionForce, transform.position);
        }

        else
        {
            wheelTouchingGround = false;
            suspensionForce = new Vector3(0f, 0f, 0f);
        }
    }
}
