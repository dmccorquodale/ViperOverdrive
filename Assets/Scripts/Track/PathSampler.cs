using System.Collections.Generic;
using UnityEngine;

public class PathSampler : MonoBehaviour
{
    [Header("Sampling")]
    public float sampleSpacing = 0.2f; // meters between path samples
    public int maxSamples = 4096;      // ring buffer

    [Header("Stability / Teleport Guards")]
    [Tooltip("Ignore sampling for this long after enable/spawn.")]
    public float warmupSeconds = 0.05f;
    [Tooltip("If the frame-to-frame distance exceeds this, treat it as a teleport and do not accumulate arc length.")]
    public float teleportThreshold = 5f;

    public struct Sample
    {
        public Vector3 pos;
        public Vector3 tangent; // unit
        public float s;         // cumulative arc length from start
    }

    public readonly List<Sample> samples = new();
    private Vector3 lastPos;
    private float lastS = 0f;

    private float warmupLeft = 0f;
    private bool armed = false;

    public float LatestS => samples.Count > 0 ? samples[^1].s : 0f;

    void OnEnable()
    {
        // Fresh arm each time this component is enabled (works with pooling)
        samples.Clear();
        lastS = 0f;
        lastPos = transform.position;
        warmupLeft = warmupSeconds;
        armed = false;

        AddSample(initial: true, overridePos: lastPos, tangent: transform.forward);
    }

    void Start()
    {
        // If OnEnable already ran, this is a no-op; kept for safety in case of script order edge cases
        if (samples.Count == 0)
        {
            lastPos = transform.position;
            AddSample(initial: true, overridePos: lastPos, tangent: transform.forward);
        }
    }

    void Update()
    {
        // 1) Warm-up: track current pose, but don't accumulate distance yet.
        if (!armed)
        {
            warmupLeft -= Time.deltaTime;

            // Keep the last (initial) sample snapped to current transform without adding distance
            lastPos = transform.position;
            if (samples.Count > 0)
            {
                samples[^1] = new Sample
                {
                    pos = lastPos,
                    tangent = transform.forward.normalized,
                    s = 0f
                };
            }

            if (warmupLeft <= 0f)
                armed = true;

            return; // skip regular sampling this frame
        }

        float frameDist = Vector3.Distance(transform.position, lastPos);

        // 2) Teleport rejection: swallow big jumps without increasing s
        if (frameDist > teleportThreshold)
        {
            lastPos = transform.position;

            // Overwrite the latest sample position, keep arc length unchanged
            if (samples.Count > 0)
            {
                float keepS = samples[^1].s;
                samples[^1] = new Sample
                {
                    pos = lastPos,
                    tangent = transform.forward.normalized,
                    s = keepS
                };
            }
            else
            {
                AddSample(initial: true, overridePos: lastPos, tangent: transform.forward);
            }

            return; // do not accumulate this jump
        }

        // 3) Normal spaced sampling
        while (frameDist >= sampleSpacing)
        {
            Vector3 dir = (transform.position - lastPos).normalized;
            lastPos += dir * sampleSpacing;
            AddSample(initial: false, overridePos: lastPos, tangent: dir);
            frameDist -= sampleSpacing;
        }
    }

    void AddSample(bool initial, Vector3? overridePos = null, Vector3? tangent = null)
    {
        Vector3 p = overridePos ?? transform.position;
        Vector3 t = (tangent ?? transform.forward).normalized;

        if (samples.Count >= maxSamples) samples.RemoveAt(0);

        float sAccum = lastS;
        if (samples.Count > 0)
        {
            // Distance from previous stored point contributes to arc length
            sAccum += Vector3.Distance(p, samples[^1].pos);
        }

        lastS = sAccum;
        samples.Add(new Sample { pos = p, tangent = t, s = sAccum });
    }

    // Optional: call this if YOU know you're teleporting (e.g., from a spawner)
    public void TeleportTo(Vector3 newPos, Vector3 newForward)
    {
        samples.Clear();
        lastS = 0f;
        lastPos = newPos;
        armed = false;            // re-run warmup to swallow any immediate adjustments
        warmupLeft = warmupSeconds;
        AddSample(initial: true, overridePos: newPos, tangent: newForward);
    }

    // Get indices that span [sStart, sEnd] along the recorded curve.
    public bool GetSpanByS(float sStart, float sEnd, out int iStart, out int iEnd)
    {
        iStart = iEnd = -1;
        if (samples.Count < 2 || sEnd <= 0f) return false;

        iEnd = samples.Count - 1;
        while (iEnd > 0 && samples[iEnd].s > sEnd) iEnd--;

        iStart = iEnd;
        while (iStart > 0 && samples[iStart - 1].s >= sStart) iStart--;

        return iStart >= 0 && iEnd > iStart;
    }
}
