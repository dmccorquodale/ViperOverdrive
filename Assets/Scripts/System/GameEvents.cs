using System;
using UnityEngine;

public static class GameEvents
{
    // Signal raised when the player hits a kill plane
    public static event Action<GameObject, Vector3> PlayerCrashed;

    public static void RaisePlayerCrashed(GameObject player, Vector3 position)
    {
        PlayerCrashed?.Invoke(player, position);
    }
}
