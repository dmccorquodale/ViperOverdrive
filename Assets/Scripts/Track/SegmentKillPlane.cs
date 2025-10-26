using UnityEngine;

public class SegmentKillPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameEvents.RaisePlayerCrashed(other.gameObject, transform.position);
        // if (other.CompareTag("Player"))
        // {
        // }
    }
}
