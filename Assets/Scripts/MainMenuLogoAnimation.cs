using UnityEngine;

public class MainMenuLogoAnimation : MonoBehaviour
{
    public float pulseSpeed;
    public float pulseAmount;
    public float overallScale;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float scaleFactor = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        scaleFactor = scaleFactor + overallScale;
        transform.localScale = new Vector3(initialScale.x * scaleFactor, initialScale.y * scaleFactor, initialScale.z * scaleFactor);
    }
}
