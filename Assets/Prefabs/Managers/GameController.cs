using System;
using System.IO;
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

    string SavePath => Path.Combine(Application.persistentDataPath, "highscores.json");
    const int MaxEntries = 5;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadScores();
    }

    public bool TrySubmitScore(float seconds)
    {
        if (seconds <= 0f) return false;
        _data.topTimes.Add(seconds);
        _data.topTimes = _data.topTimes
            .OrderByDescending(t => t)
            .Take(MaxEntries)
            .ToList();
        SaveScores();
        return true;
    }

    public void ResetScores()
    {
        _data.topTimes.Clear();
        SaveScores();
    }

    void LoadScores()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                _data = JsonUtility.FromJson<ScoreData>(json) ?? new ScoreData();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load highscores: {e.Message}");
            _data = new ScoreData();
        }
    }

    void SaveScores()
    {
        try
        {
            var json = JsonUtility.ToJson(_data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to save highscores: {e.Message}");
        }
    }
}
