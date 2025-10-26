using UnityEngine;

public class AudioManager : MonoBehaviour
{
    void Awake()
    {
        var all = FindObjectsByType<AudioManager>(FindObjectsSortMode.None);

        if (all.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
