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
    private float rearTyreGripFactor;
    public bool rearTyre;

    public AudioSource tyreSqueelAudio;

    void Start()
    {
        rb = car.GetComponent<Rigidbody>();

        tyreForce = car.GetComponent<Car>().tyreForce;
        tyreForceClamp = car.GetComponent<Car>().tyreForceClamp;
        rearTyreGripFactor = car.GetComponent<Car>().rearTyreGripFactor;

        if (rearTyre)
        {
            tyreForce = tyreForce * rearTyreGripFactor;
        }
    }

    void FixedUpdate()
    {
        Start(); // this is so I can tweak values while game is running - delete later

        if (suspension.wheelTouchingGround)
        {
            float lateralVelocity = getLateralVelocity();
            lateralForce = Mathf.Clamp(lateralVelocity * tyreForce, -tyreForceClamp, tyreForceClamp);
            //Debug.Log(lateralForce);
            //lateralForce = lateralForce;
            lateralForce = lateralForce * Time.deltaTime;

            //rb.AddForceAtPosition(lateralForce * -transform.right, outputForceLocation.transform.position);

            //new jank place to turn car from
            Vector3 jankRotationLocation = new Vector3(outputForceLocation.transform.position.x, car.transform.position.y, outputForceLocation.transform.position.z);
            rb.AddForceAtPosition(lateralForce * -transform.right, jankRotationLocation);

            Debug.DrawLine(outputForceLocation.transform.position, jankRotationLocation + ((lateralForce * -transform.right) / 500), Color.white);
        }
        else
        {
            //Debug.Log("NOT touching ground");
        }

        tyreSqueelAudio.volume = Mathf.Lerp(0f, 0.02f, lateralForce / 2000f);
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
