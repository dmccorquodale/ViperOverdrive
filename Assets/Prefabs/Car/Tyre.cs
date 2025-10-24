using UnityEngine;

public class Tyre : MonoBehaviour
{
    public GameObject outputForceLocation;
    public GameObject car;
    private Rigidbody rb;
    public Suspension suspension;

    void Start()
    {
        rb = car.GetComponent<Rigidbody>();
    }

    void Update()
    {
        
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
