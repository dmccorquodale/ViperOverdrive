using UnityEngine;

public class Steering : MonoBehaviour
{
    public GameObject car;
    private float steeringLock;
    private float steeringAmount;
    private Quaternion steeringOutput;

    void Start()
    {
        steeringLock = car.GetComponent<Car>().steeringLock;
    }

    void Update()
    {
        //Start(); // this is so I can tweak values while game is running - delete later

        //at 100kph, you can only steer 20 degrees

        steeringAmount = car.GetComponent<Car>().steeringInput * steeringLock;

        float steeringAmountLimitFactor = (150 - car.GetComponent<Car>().currentSpeedKmH) / 100;
        steeringAmountLimitFactor = Mathf.Clamp(steeringAmountLimitFactor, 0, 1);
        //Debug.Log(steeringAmountLimitFactor);

        steeringOutput = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y + (steeringAmount * steeringAmountLimitFactor), transform.localRotation.z);

        transform.localRotation = steeringOutput;
    }
}
