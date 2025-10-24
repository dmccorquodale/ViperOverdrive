using UnityEngine;

public static class TransportFrame
{
    // Parallel transport the 'up' vector from (t0) to (t1) to avoid twist.
    public static Vector3 TransportUp(Vector3 upPrev, Vector3 t0, Vector3 t1)
    {
        t0.Normalize(); t1.Normalize();
        Vector3 v = Vector3.Cross(t0, t1);
        float s = v.magnitude;
        float c = Vector3.Dot(t0, t1);
        if (s < 1e-6f) return upPrev; // almost no change

        // Rodrigues rotation formula: rotate upPrev around axis v by angle atan2(s,c)
        float angle = Mathf.Atan2(s, c);
        Vector3 axis = v / s;
        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis) * upPrev;
    }
}
