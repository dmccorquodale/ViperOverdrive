using UnityEngine;

[ExecuteAlways]
public class PathSamplerGizmos : MonoBehaviour
{
    public PathSampler sampler;
    [Range(1,50)] public int drawEveryNth = 4;     // decimate for clarity
    public float pointSize = 0.15f;
    public float tangentLen = 1.0f;
    public Color lineColor = new Color(0.2f, 1f, 0.6f, 1f);
    public Color pointColor = new Color(1f, 0.7f, 0.2f, 1f);
    public Color tangentColor = new Color(0.6f, 0.8f, 1f, 1f);
    public bool highlight3mSpan = true;

    void OnDrawGizmos()
    {
        // if (sampler == null) sampler = FindObjectOfType<PathSampler>();
        // Debug.Log($"On Draw Gizmo");

        if (sampler == null || sampler.samples.Count < 2)
        {

            return;
        }
        Debug.Log($"PathSamplerGizmo: samples={sampler.samples.Count}");

        // Draw polyline
        Gizmos.color = lineColor;
        for (int i = 1; i < sampler.samples.Count; i++)
        {
            Gizmos.DrawLine(sampler.samples[i - 1].pos, sampler.samples[i].pos);
        }

        // Draw points + tangents
        for (int i = 0; i < sampler.samples.Count; i += Mathf.Max(1, drawEveryNth))
        {
            var s = sampler.samples[i];
            Gizmos.color = pointColor;
            Gizmos.DrawSphere(s.pos, pointSize);

            Gizmos.color = tangentColor;
            Gizmos.DrawLine(s.pos, s.pos + s.tangent * tangentLen);
        }

        // Optional: visualize the latest ~3m span you’ll spawn segments on
        if (highlight3mSpan && sampler.TryGetSpan(3f, out int iStart, out int iEnd, out _))
        {
            Gizmos.color = Color.magenta;
            for (int i = iStart + 1; i <= iEnd; i++)
                Gizmos.DrawLine(sampler.samples[i - 1].pos, sampler.samples[i].pos);
        }
    }
}
