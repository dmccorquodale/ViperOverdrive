using UnityEngine;

public class Steering : MonoBehaviour
{
    public GameObject car;
    public float steeringLock;
    private float steeringAmount;
    private Quaternion steeringOutput;

    void Update()
    {
        steeringAmount = car.GetComponent<Car>().steeringInput * steeringLock;
        steeringOutput = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y + steeringAmount, transform.localRotation.z);

        transform.localRotation = steeringOutput;
    }
}
