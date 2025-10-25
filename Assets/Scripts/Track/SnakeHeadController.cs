using UnityEngine;

public class SnakeHeadController : MonoBehaviour
{
    [Header("Forward Motion")]
    public float forwardSpeed = 20f;

    [Header("Left/Right Wave (X)")]
    public float lateralAmp = 16f;
    public float lateralWavelengthM = 200f;

    [Header("Up/Down Wave (Y)")]
    public float verticalAmp = 8f;
    public float verticalWavelengthM = 340f;
    public float phaseOffset = 0.7f;

    [Header("Turn Limiting")]
    public float maxTurnRateDegPerSec = 120f;

    [Header("Base Height")]
    public float baseY = 100f;

    // internals
    private float sMeters = 0f;
    private Vector3 lastPos;
    private bool initialized = false;

    // new random phase values
    private float lateralPhase;
    private float verticalPhase;

    void Start()
    {
        // Randomize where in the sine wave we start (0 to 2π)
        lateralPhase = Random.Range(0f, Mathf.PI * 2f);
        verticalPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float dz = forwardSpeed * dt;
        sMeters += dz;

        float kLat = (lateralWavelengthM  > 0.001f) ? (Mathf.PI * 2f / lateralWavelengthM)  : 0f;
        float kVert = (verticalWavelengthM > 0.001f) ? (Mathf.PI * 2f / verticalWavelengthM) : 0f;

        // apply the random phase offsets
        float x = lateralAmp * Mathf.Sin(kLat * sMeters + lateralPhase);
        float y = baseY + verticalAmp * Mathf.Sin(kVert * sMeters + verticalPhase + phaseOffset);
        float z = sMeters;

        Vector3 newPos = new Vector3(x, y, z);

        if (!initialized)
        {
            transform.position = newPos;
            lastPos = newPos - Vector3.forward * 0.01f;
            initialized = true;
        }

        Vector3 tangent = (newPos - lastPos);
        if (tangent.sqrMagnitude < 1e-6f) tangent = transform.forward;
        tangent.Normalize();

        float maxStepRad = maxTurnRateDegPerSec * Mathf.Deg2Rad * dt;
        Vector3 dir = Vector3.RotateTowards(transform.forward, tangent, maxStepRad, Mathf.Infinity).normalized;

        transform.position = newPos;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        lastPos = newPos;
    }
}
