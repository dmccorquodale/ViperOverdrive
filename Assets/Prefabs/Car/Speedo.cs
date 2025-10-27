using UnityEngine;

public class Speedo : MonoBehaviour
{
    public Car car;
    public GameObject needleRotationObject;

    private float secondsBetweenUpdate = 0.25f;
    private float frameCounter = 0f;

    void Update()
    {
        if (frameCounter > secondsBetweenUpdate)
        {
            needleRotationObject.transform.localEulerAngles = new Vector3(0f, car.currentSpeedKmH, 0f);
            frameCounter = 0f;
        }
        else
        {
            frameCounter += Time.deltaTime;
        }

        Debug.Log(frameCounter);
    }
}
