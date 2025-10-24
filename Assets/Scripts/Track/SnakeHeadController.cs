using UnityEngine;

public class SnakeHeadController : MonoBehaviour
{
    [Header("Forward Motion")]
    public float forwardSpeed = 20f;   // m/s along the path

    [Header("Left/Right Wave (X)")]
    public float lateralAmp = 16f;      // meters side-to-side (try 6–12)
    public float lateralWavelengthM = 200f; // meters per full left-right-left cycle

    [Header("Up/Down Wave (Y)")]
    public float verticalAmp = 8f;          // meters up/down (try 4–10)
    public float verticalWavelengthM = 200f;// meters per full up/down cycle
    public float phaseOffset = 0.7f;        // radians offset between lateral & vertical

    [Header("Turn Limiting")]
    public float maxTurnRateDegPerSec = 120f; // cap how quickly the head can rotate

    [Header("Base Height")]
    public float baseY = 100f; // the average altitude to cruise at

    // internals
    private float sMeters = 0f;       // distance traveled along path
    private Vector3 lastPos;          // for tangent
    private bool initialized = false;

    void Update()
    {
        float dt = Time.deltaTime;
        float dz = forwardSpeed * dt;
        sMeters += dz;

        // Convert meters -> phase
        float kLat   = (lateralWavelengthM  > 0.001f) ? (Mathf.PI * 2f / lateralWavelengthM)  : 0f;
        float kVert  = (verticalWavelengthM > 0.001f) ? (Mathf.PI * 2f / verticalWavelengthM) : 0f;

        // Evaluate clean, zero-mean waves
        float x = lateralAmp  * Mathf.Sin(kLat * sMeters);
        float y = baseY + verticalAmp * Mathf.Sin(kVert * sMeters + phaseOffset);
        float z = sMeters; // march forward along +Z in world space

        Vector3 newPos = new Vector3(x, y, z);

        if (!initialized)
        {
            transform.position = newPos;
            lastPos = newPos - Vector3.forward * 0.01f; // tiny backward offset to give an initial tangent
            initialized = true;
        }

        // Tangent = where we're actually moving
        Vector3 tangent = (newPos - lastPos);
        if (tangent.sqrMagnitude < 1e-6f) tangent = transform.forward;
        tangent.Normalize();

        // Rate-limit rotation so it never snaps
        float maxStepRad = maxTurnRateDegPerSec * Mathf.Deg2Rad * dt;
        Vector3 dir = Vector3.RotateTowards(transform.forward, tangent, maxStepRad, Mathf.Infinity).normalized;

        // Apply
        transform.position = newPos;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        lastPos = newPos;
    }
}
