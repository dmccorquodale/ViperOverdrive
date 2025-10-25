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
        Start(); // this is so I can tweak values while game is running - delete later

        steeringAmount = car.GetComponent<Car>().steeringInput * steeringLock;
        steeringOutput = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y + steeringAmount, transform.localRotation.z);

        transform.localRotation = steeringOutput;
    }
}
