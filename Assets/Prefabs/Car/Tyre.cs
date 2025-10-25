using UnityEngine;

public class Tyre : MonoBehaviour
{
    public GameObject outputForceLocation;
    public GameObject car;
    private Rigidbody rb;
    public Suspension suspension;
    private float lateralForce;
    private float tyreForce;
    private float tyreForceClamp;

    void Start()
    {
        rb = car.GetComponent<Rigidbody>();

        tyreForce = car.GetComponent<Car>().tyreForce;
        tyreForceClamp = car.GetComponent<Car>().tyreForceClamp;
    }

    void Update()
    {
        Start(); // this is so I can tweak values while game is running - delete later

        if (suspension.wheelTouchingGround)
        {
            float lateralVelocity = getLateralVelocity();
            lateralForce = Mathf.Clamp(lateralVelocity * tyreForce, -tyreForceClamp, tyreForceClamp);
            lateralForce = lateralForce * Time.deltaTime;
            Debug.Log(lateralForce);

            rb.AddForceAtPosition(lateralForce * -transform.right, outputForceLocation.transform.position);
        }
    }
    
    float getLateralVelocity()
    {
        Vector3 wheelVelocityWS = rb.GetPointVelocity(outputForceLocation.transform.position);

        Vector3 right = transform.TransformDirection(Vector3.right);

        //getting only the lateral component of wheel velocity
        float rightDot = Vector3.Dot(right, wheelVelocityWS);
        return rightDot;
    }
}
