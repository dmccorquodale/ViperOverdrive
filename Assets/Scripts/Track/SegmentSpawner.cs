using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SegmentPool))]
public class SegmentSpawner : MonoBehaviour
{
    public PathSampler headSampler;       // assign SnakeHead's PathSampler
    public Transform trackParent;         // usually this GameObject
    public float segmentLength = 3f;
    public float maxAnglePerSegmentDeg = 8f;
    public int maxActiveSegments = 300;
    [Range(0f, 1f)] public float gravityUpBias = 0.85f;

    private SegmentPool pool;
    private readonly LinkedList<GameObject> active = new();
    private Vector3 transportedUp = Vector3.up;
    private Vector3 lastTangent = Vector3.forward;

    private float nextSpawnAtS = 0f;     // end-s of the next segment
    public int maxSegmentsPerFrame = 64; // safety cap

    void Awake()
    {
        pool = GetComponent<SegmentPool>();
    }

    void Start()
    {
        // First segment should end after segmentLength
        nextSpawnAtS = segmentLength;
    }

    void Update()
    {
        TrySpawnSegments();
        CullFarSegments();
    }

    void TrySpawnSegments()
    {
        float latestS = headSampler.LatestS;
        int spawnedThisFrame = 0;

        while (latestS >= nextSpawnAtS && spawnedThisFrame < maxSegmentsPerFrame)
        {
            float sEnd = nextSpawnAtS;
            float sStartNominal = sEnd - segmentLength;

            if (!headSampler.GetSpanByS(sStartNominal, sEnd, out int iStart, out int iEnd))
                break; // not enough samples yet

            // Curvature check: angle between end tangents
            var s = headSampler.samples;
            Vector3 t0 = s[Mathf.Max(iStart, 0)].tangent;
            Vector3 t1 = s[iEnd].tangent;
            float angle = Vector3.Angle(t0, t1);

            float thisSegLen = segmentLength;

            // If too curved, shorten this one proportionally, clamp to a minimum
            if (angle > maxAnglePerSegmentDeg)
            {
                float ratio = maxAnglePerSegmentDeg / Mathf.Max(angle, 0.01f);
                thisSegLen = Mathf.Clamp(segmentLength * ratio, 0.75f, segmentLength);
                sStartNominal = sEnd - thisSegLen;

                if (!headSampler.GetSpanByS(sStartNominal, sEnd, out iStart, out iEnd))
                    break; // still not enough data; wait a frame
            }

            // Compute midpoint along the arc (by s)
            Vector3 midPos = MidpointAlongS(s, sStartNominal, sEnd, iStart, iEnd);
            Vector3 midTan = (s[iStart].tangent + s[iEnd].tangent).normalized;

            // Parallel-transport up, then stabilize with gravity
            transportedUp = TransportFrame.TransportUp(transportedUp, lastTangent, midTan);
            lastTangent = midTan;
            Vector3 blendedUp = Vector3.Slerp(transportedUp, Vector3.up, gravityUpBias).normalized;
            Quaternion rot = Quaternion.LookRotation(midTan, blendedUp);

            // Spawn
            var seg = pool.Get();
            seg.transform.SetPositionAndRotation(midPos, rot);
            seg.transform.SetParent(trackParent, true);
            active.AddLast(seg);

            // Seam bridge
            SnapBridgeToPrevious(seg);

            // Advance the spawn cursor by the ACTUAL length we used
            nextSpawnAtS += thisSegLen;
            spawnedThisFrame++;

            // Recycle if needed
            if (active.Count > maxActiveSegments)
            {
                pool.Release(active.First.Value);
                active.RemoveFirst();
            }
        }

        // Safety notice if we hit the cap (diagnostic only)
        if (spawnedThisFrame >= maxSegmentsPerFrame)
            Debug.LogWarning("[SegmentSpawner] Hit per-frame spawn cap; consider raising maxSegmentsPerFrame.");
    }

    Vector3 MidpointAlongS(List<PathSampler.Sample> s, float sStart, float sEnd, int i0, int i1)
    {
        float midS = (sStart + sEnd) * 0.5f;

        // Find segment containing midS
        int i = i0 + 1;
        while (i <= i1 && s[i].s < midS) i++;
        i = Mathf.Clamp(i, i0 + 1, i1);

        float sA = s[i - 1].s;
        float sB = s[i].s;
        float t = Mathf.InverseLerp(sA, sB, midS);
        return Vector3.Lerp(s[i - 1].pos, s[i].pos, t);
    }

    void SnapBridgeToPrevious(GameObject current)
    {
        if (active.Count < 2) return;
        var prev = active.Last.Previous.Value;

        var aHead = current.transform.Find("BridgeAnchor_Head");
        var bTail = prev.transform.Find("BridgeAnchor_Tail");
        var bridge = current.transform.Find("BridgeStrip");

        if (aHead && bTail && bridge)
        {
            Vector3 p0 = bTail.position;
            Vector3 p1 = aHead.position;

            var tr = bridge as Transform;
            tr.gameObject.SetActive(true);
            tr.position = (p0 + p1) * 0.5f;

            Vector3 dir = (p1 - p0);
            tr.rotation = Quaternion.LookRotation(dir.normalized, current.transform.up);

            Vector3 s = tr.localScale;
            s.z = Mathf.Max(0.001f, dir.magnitude);
            tr.localScale = s;
        }
    }

    void CullFarSegments()
    {
        // Count-based culling already in place via maxActiveSegments.
        // You can switch to distance-based later.
    }
}
