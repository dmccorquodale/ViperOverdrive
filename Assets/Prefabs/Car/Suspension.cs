using UnityEngine;

public class Suspension : MonoBehaviour
{
    private Rigidbody rb;
    public GameObject car;

    public bool wheelTouchingGround;
    public GameObject tyre;

    public float wheelRadius;
    
    private float restLength;
    private float springTravel;
    private float springStiffness;
    private float damperStiffness;

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

        springTravel = car.GetComponent<Car>().springTravel;
        springStiffness = car.GetComponent<Car>().springStiffness;
        damperStiffness = car.GetComponent<Car>().damperStiffness;
        restLength = car.GetComponent<Car>().restLength;

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    void FixedUpdate()
    {
        Start(); // this is so I can tweak values while game is running - delete later

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            wheelTouchingGround = true;

            //Debug.DrawRay(transform.position, -transform.up * hit.distance, Color.red);

            lastLength = springLength;

            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);

            springForce = springStiffness * (restLength - springLength);

            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
            damperForce = damperStiffness * springVelocity;

            float upDot = Vector3.Dot(Vector3.up, transform.up);

            suspensionForce = (springForce + damperForce) * transform.up * upDot;            

            tyre.transform.position = new Vector3(hit.point.x, hit.point.y + wheelRadius, hit.point.z);
            rb.AddForceAtPosition(suspensionForce, transform.position);

            //Debug.DrawLine(transform.position, suspensionForce / 5000 + transform.position , Color.white);
        }

        else
        {
            //Debug.Log("Wheels not on ground");
            wheelTouchingGround = false;
            suspensionForce = new Vector3(0f, 0f, 0f);
        }
    }
}
