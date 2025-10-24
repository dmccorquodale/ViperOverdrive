using UnityEngine;

[ExecuteAlways]
public class PathSamplerGizmos : MonoBehaviour
{
    [Header("Source")]
    public PathSampler sampler;

    [Header("Drawing Controls")]
    [Range(1, 50)] public int drawEveryNth = 4; // decimate for clarity
    [Min(0.01f)] public float pointSize = 0.12f;
    [Min(0.01f)] public float tangentLen = 0.9f;

    [Header("Colors")]
    public Color lineColor    = new Color(0.20f, 1.00f, 0.60f, 1f);
    public Color pointColor   = new Color(1.00f, 0.70f, 0.20f, 1f);
    public Color tangentColor = new Color(0.60f, 0.80f, 1.00f, 1f);
    public Color highlightColor = new Color(1.00f, 0.20f, 0.90f, 1f);

    [Header("Options")]
    public bool drawPolyline = true;
    public bool drawPointsAndTangents = true;
    public bool highlightLast3m = true;

    void OnDrawGizmos()
    {
        DrawGizmosInternal();
    }

    // If you prefer to only draw when this component is selected in the editor,
    // comment out OnDrawGizmos above and uncomment the method below.
    // void OnDrawGizmosSelected() => DrawGizmosInternal();

    void DrawGizmosInternal()
    {
        if (sampler == null || sampler.samples == null) return;
        int count = sampler.samples.Count;
        if (count < 2) return;

        // Polyline along all samples
        if (drawPolyline)
        {
            Gizmos.color = lineColor;
            for (int i = 1; i < count; i++)
            {
                Gizmos.DrawLine(sampler.samples[i - 1].pos, sampler.samples[i].pos);
            }
        }

        // Decimated points + tangents
        if (drawPointsAndTangents)
        {
            int step = Mathf.Max(1, drawEveryNth);
            for (int i = 0; i < count; i += step)
            {
                var s = sampler.samples[i];
                Gizmos.color = pointColor;
                Gizmos.DrawSphere(s.pos, pointSize);

                Gizmos.color = tangentColor;
                Gizmos.DrawLine(s.pos, s.pos + s.tangent.normalized * tangentLen);
            }
        }

        // Highlight the latest ~3 meters of arc length using the new API
        if (highlightLast3m)
        {
            float sEnd = sampler.LatestS;
            float sStart = Mathf.Max(0f, sEnd - 3f);

            if (sampler.GetSpanByS(sStart, sEnd, out int iStart, out int iEnd))
            {
                Gizmos.color = highlightColor;
                for (int i = iStart + 1; i <= iEnd; i++)
                {
                    Gizmos.DrawLine(sampler.samples[i - 1].pos, sampler.samples[i].pos);
                }
            }
        }
    }
}
