using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    float _startTime;
    bool _running;

    [Header("Optional UI")]
    public TMPro.TextMeshProUGUI timerText; // assign if you want HUD time

    [Header("Flow")]
    public string returnScene = "MainMenu";

    // Call this manually when ready (e.g., after player spawns)
    public void StartTimer()
    {
        _startTime = Time.time;
        _running = true;
    }

    // Optionally allow pausing/stopping
    public void StopTimer()
    {
        _running = false;
    }

    void Update()
    {
        if (!_running) return;

        float elapsed = Time.time - _startTime;
        if (timerText)
        {
            int hours = Mathf.FloorToInt(elapsed / 3600f);
            int minutes = Mathf.FloorToInt((elapsed % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            int milliseconds = Mathf.FloorToInt((elapsed * 1000f) % 1000f);
            timerText.text = $"{hours:00}:{minutes:00}:{seconds:00}:{milliseconds:000}";
        }
    }

    public void OnPlayerDied()
    {
        if (!_running) return;

        _running = false;
        float total = Time.time - _startTime;
        GameController.Instance?.TrySubmitScore(total);
    }
}
