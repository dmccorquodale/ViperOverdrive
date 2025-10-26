using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Serializable]
    public class ScoreData
    {
        public List<float> topTimes = new(); // seconds survived; higher is better
    }

    public IReadOnlyList<float> TopTimes => _data.topTimes;
    private ScoreData _data = new();

    const int MaxEntries = 5;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Cursor.lockState = CursorLockMode.Confined;
    }

    public bool TrySubmitScore(float seconds)
    {
        if (seconds <= 0f) return false;
        _data.topTimes.Add(seconds);
        _data.topTimes = _data.topTimes
            .OrderByDescending(t => t)
            .Take(MaxEntries)
            .ToList();
        return true;
    }

    public void ResetScores()
    {
        _data.topTimes.Clear();
    }
}
