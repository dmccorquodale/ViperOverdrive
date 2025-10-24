using System.Collections.Generic;
// using System.Diagnostics;   // ← remove this unless you really need it
using UnityEngine;

public class PathSampler : MonoBehaviour
{
    [Header("Sampling")]
    public float sampleSpacing = 0.2f; // meters between path samples
    public int maxSamples = 4096;      // ring buffer

    public struct Sample { public Vector3 pos; public Vector3 tangent; }

    public readonly List<Sample> samples = new();
    private Vector3 lastPos;

    void Start()
    {
        lastPos = transform.position;
        AddSample(initial: true);
    }

    void Update()
    {
        if (Time.frameCount % 30 == 0)
            UnityEngine.Debug.Log($"PathSampler: samples={samples.Count}");

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
        Vector3 t = tangent ?? transform.forward;

        if (samples.Count >= maxSamples) samples.RemoveAt(0);
        samples.Add(new Sample { pos = p, tangent = t.normalized });
    }

    public bool TryGetSpan(float length, out int iStart, out int iEnd, out float actualLength)
    {
        iEnd = samples.Count - 1;
        iStart = iEnd;
        actualLength = 0f;
        if (iEnd <= 0) return false;

        while (iStart > 0 && actualLength < length)
        {
            float d = Vector3.Distance(samples[iStart].pos, samples[iStart - 1].pos);
            actualLength += d;
            iStart--;
        }
        return actualLength > 0.01f;
    }

    void LateUpdate()
    {
        // Draw in Game view (requires Game-view Gizmos enabled)
        if (samples.Count < 2) return;

        float lengthShown = 0f;
        for (int i = samples.Count - 1; i > 0; i--)
        {
            var p0 = samples[i - 1].pos;
            var p1 = samples[i].pos;

            UnityEngine.Debug.DrawLine(p0, p1, Color.cyan, 0f, false);
            lengthShown += Vector3.Distance(p0, p1);
            if (lengthShown > 10f) break; // increase if you want more
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw in Scene view (Editor)
        if (samples == null || samples.Count < 2) return;
        Gizmos.color = Color.green;
        for (int i = 1; i < samples.Count; i++)
            Gizmos.DrawLine(samples[i - 1].pos, samples[i].pos);
    }
#endif
}
