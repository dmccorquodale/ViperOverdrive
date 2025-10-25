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

    // Builds a rotation with no roll: forward = fwd, up is as close to world up as possible.
    // Falls back to fallbackUp when forward ~ parallel to world up.
    public static Quaternion NoRollRotation(Vector3 fwd, Vector3 fallbackUp)
    {
        fwd = fwd.normalized;
        Vector3 upRef = Vector3.up;

        // Project world-up onto plane perpendicular to forward
        Vector3 upProj = upRef - fwd * Vector3.Dot(upRef, fwd);
        float mag = upProj.magnitude;

        if (mag < 1e-3f)
        {
            // We're near-parallel to world-up; use a safe fallback (e.g., transported up)
            upProj = fallbackUp - fwd * Vector3.Dot(fallbackUp, fwd);
            mag = upProj.magnitude;

            if (mag < 1e-3f)
            {
                // As a last resort, pick any perpendicular
                upProj = Vector3.Cross(fwd, Vector3.right);
                if (upProj.sqrMagnitude < 1e-6f)
                    upProj = Vector3.Cross(fwd, Vector3.forward);
            }
        }

        upProj /= mag;
        return Quaternion.LookRotation(fwd, upProj);
    }
}
