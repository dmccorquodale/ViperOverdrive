using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // new input system
using System.Text;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string levelToLoad = "Level01";

    [Header("UI (optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI highscoresText; // assign in Inspector

    [Header("Input")]
    // Bind in code below or via Inspector
    public InputAction startAction;

    void OnEnable()
    {
        // Ensure bindings exist (you can also add these in the Inspector)
        if (startAction.bindings.Count == 0)
        {
            startAction.AddBinding("<Keyboard>/space");
            startAction.AddBinding("<Keyboard>/enter");
            startAction.AddBinding("<Gamepad>/buttonSouth"); // A / Cross
            startAction.AddBinding("<Gamepad>/start");
        }

        startAction.Enable();
        startAction.performed += OnStartPerformed;

        // Update highscores immediately and once next frame (covers init timing)
        UpdateHighscoresUI();
        StartCoroutine(RefreshNextFrame());
    }

    void OnDisable()
    {
        startAction.performed -= OnStartPerformed;
        startAction.Disable();
    }

    System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null;
        UpdateHighscoresUI();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        UpdateHighscoresUI();
    }

    private void OnStartPerformed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        SceneManager.LoadScene(levelToLoad, LoadSceneMode.Single);
    }

    // ----- Highscores -----
    void UpdateHighscoresUI()
    {
        if (!highscoresText) return;

        var gc = GameController.Instance;
        if (gc == null || gc.TopTimes == null || gc.TopTimes.Count == 0)
        {
            highscoresText.text = "Top Times\n— no scores yet —";
            return;
        }

        var sb = new StringBuilder("Top Times\n");
        for (int i = 0; i < gc.TopTimes.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {FormatTime(gc.TopTimes[i])}");
        }
        highscoresText.text = sb.ToString();
    }

    static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
