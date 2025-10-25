using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    float _startTime;
    bool _running;

    [Header("Optional UI")]
    public TMPro.TextMeshProUGUI timerText; // assign if you want HUD time

    [Header("Flow")]
    public string returnScene = "MainMenu";

    void OnEnable()
    {
        _startTime = Time.time;
        _running = true;
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


    // Call this when the player dies.
    public void OnPlayerDied()
    {
        if (!_running) return;
        _running = false;

        float total = Time.time - _startTime;
        GameController.Instance?.TrySubmitScore(total);

        // Return to menu (or load a GameOver scene if you add one)
        SceneManager.LoadScene(returnScene, LoadSceneMode.Single);
    }
}
