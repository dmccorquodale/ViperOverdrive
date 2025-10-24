using System.Collections.Generic;
using UnityEngine;

public class PathSampler : MonoBehaviour
{
    [Header("Sampling")]
    public float sampleSpacing = 0.2f; // meters between path samples
    public int maxSamples = 4096;      // ring buffer

    public struct Sample
    {
        public Vector3 pos;
        public Vector3 tangent; // unit
        public float s;         // cumulative arc length from start
    }

    public readonly List<Sample> samples = new();
    private Vector3 lastPos;
    private float lastS = 0f;

    public float LatestS => samples.Count > 0 ? samples[^1].s : 0f;

    void Start()
    {
        lastPos = transform.position;
        AddSample(initial: true);
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPos);
        while (dist >= sampleSpacing)
        {
            Vector3 dir = (transform.position - lastPos).normalized;
            lastPos += dir * sampleSpacing;
            AddSample(initial: false, overridePos: lastPos, tangent: dir);
            dist -= sampleSpacing;
        }
    }

    void AddSample(bool initial, Vector3? overridePos = null, Vector3? tangent = null)
    {
        Vector3 p = overridePos ?? transform.position;
        Vector3 t = (tangent ?? transform.forward).normalized;

        if (samples.Count > 0)
            lastS += Vector3.Distance(p, samples[^1].pos);

        if (samples.Count >= maxSamples) samples.RemoveAt(0);
        samples.Add(new Sample { pos = p, tangent = t, s = lastS });
    }

    // Get indices that span [sStart, sEnd] along the recorded curve.
    // Returns false if not enough data yet.
    public bool GetSpanByS(float sStart, float sEnd, out int iStart, out int iEnd)
    {
        iStart = iEnd = -1;
        if (samples.Count < 2 || sEnd <= 0f) return false;

        // iEnd: last index whose s <= sEnd
        iEnd = samples.Count - 1;
        while (iEnd > 0 && samples[iEnd].s > sEnd) iEnd--;

        // iStart: first index whose s >= sStart (but not after iEnd)
        iStart = iEnd;
        while (iStart > 0 && samples[iStart - 1].s >= sStart) iStart--;

        // Basic sanity
        if (iStart < 0 || iEnd <= iStart) return false;
        return true;
    }
}
