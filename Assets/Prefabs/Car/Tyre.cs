using UnityEngine;

public class Tyre : MonoBehaviour
{
    public GameObject outputForceLocation;
    public GameObject car;
    private Rigidbody rb;
    public Suspension suspension;
    private float lateralForce;
    public float tyreForce;
    public float maximumTyreGrip;

    void Start()
    {
        rb = car.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (suspension.wheelTouchingGround)
        {
            float lateralVelocity = getLateralVelocity();
            lateralForce = Mathf.Clamp(lateralVelocity * tyreForce, -maximumTyreGrip, maximumTyreGrip);
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
