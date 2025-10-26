using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;
using System.Data.Common;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Car")]
    [SerializeField] private Car car;

    [Header("Snake")]
    [SerializeField] private GameObject snakeHead;

    [Header("TrackRoot")]
    [SerializeField] private GameObject trackRoot;
    [SerializeField] private Transform spawnPoint;

    [Header("Systems")]
    [SerializeField] private LevelTimer levelTimer;  // drag your LevelTimer here

    public GameObject PlayerInstance { get; private set; }
    public Transform SpawnPoint => spawnPoint;

    private bool gameOver = false;

    [Header("Camera")]
    [SerializeField] private GameObject mainCamera;

    [Header("Flow")]
    public string returnScene = "MainMenu";

    // Seconds the car has before it'll game over at < gameOverSpeed
    private float graceTime = 5f;
    private float gameOverSpeed = 10f;


    void OnEnable()  => GameEvents.PlayerCrashed += OnPlayerCrashed;
    void OnDisable() => GameEvents.PlayerCrashed -= OnPlayerCrashed;

    void Awake()
    {
        // if (levelTimer == null)
            // levelTimer = FindObjectOfType<LevelTimer>();
    }

    void Start()
    {
        gameOver = false;
        StartCoroutine(SpawnPlayerDelayed(3f));
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ReturnToMainMenu();
        }

        float time = levelTimer.GetTime();
        UpdateSpeeds(time);
        UpdateCarUITimer(time);

        if (time > graceTime) CarUnderGameOverSpeed();

        


    }

    public void SetSpawnPoint(Transform newSpawn)
    {
        // Create an empty object at the offset
        GameObject offsetSpawn = new GameObject("SpawnPoint_Offset");
        offsetSpawn.transform.position = newSpawn.position + Vector3.up * 10.5f;
        offsetSpawn.transform.rotation = newSpawn.rotation;
        offsetSpawn.transform.SetParent(newSpawn); // optional

        spawnPoint = offsetSpawn.transform;
    }

   public void SpawnPlayer()
    {
        Transform firstSegment = trackRoot.transform.GetChild(0);
        SetSpawnPoint(firstSegment);


        // Instantiate
        Destroy(mainCamera);
        PlayerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        // Get the Car component and call Go()
        car = PlayerInstance.GetComponent<Car>();
        if (car == null)
        {
            Debug.LogError("LevelManager: Spawned playerPrefab has no Car component.");
            return;
        }
        car.Go();

        // Optionally wire up other bits
        // WirePlayer(PlayerInstance);
        // MovePlayerToSpawn(PlayerInstance);
    }

    IEnumerator SpawnPlayerDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnPlayer();
        levelTimer.StartTimer();
    }

    void WirePlayer(GameObject player)
    {
        // // Subscribe to death signals
        // var health = player.GetComponent<CarHealth>();
        // if (health != null)
        // {
        //     health.OnDied += HandlePlayerDied;
        // }

        // // Optional: ensure OutOfBoundsKill points back to this manager
        // var oob = player.GetComponent<OutOfBoundsKill>();
        // if (oob != null) oob.Manager = this;
    }

    void MovePlayerToSpawn(GameObject player)
    {
        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        // Clear physics so it doesn’t launch
        if (player.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void HandlePlayerDied()
    {
        // Hand responsibility to the timer (submits score + returns to menu)
        if (levelTimer != null)
            levelTimer.OnPlayerDied();
        else
            Debug.LogWarning("LevelManager: LevelTimer missing when handling death.");
    }

    private void OnPlayerCrashed(GameObject player, Vector3 pos)
    {
        if (gameOver == true) return;
        // Debug.Log($"Game Over at {pos}");
        gameOver = true;
        levelTimer.OnPlayerDied();

        // Return to menu (or load a GameOver scene if you add one)
        SceneManager.LoadScene(returnScene, LoadSceneMode.Single);
    }

    private void OnPlayerStop()
    {
        if (gameOver == true) return;
        // Debug.Log($"Game Over at {pos}");
        gameOver = true;
        levelTimer.OnPlayerDied();

        // Return to menu (or load a GameOver scene if you add one)
        SceneManager.LoadScene(returnScene, LoadSceneMode.Single);
    }
    public void ReturnToMainMenu()
    {
        // Optional: stop timer, clean up, etc.
        if (levelTimer != null)
            levelTimer.OnPlayerDied();

        // Debug.Log("Returning to Main Menu...");
        SceneManager.LoadScene(returnScene, LoadSceneMode.Single);
    }

    private void UpdateSpeeds(float time)
    {
        if (car == null) return;
        SnakeHeadController shc = snakeHead.GetComponent<SnakeHeadController>();
        if (time <= 5f)
        {
            car.targetSpeedKmH = 30f;
            shc.forwardSpeed = 12f;
            return;
        }

        car.targetSpeedKmH = time + 25f;
        shc.forwardSpeed = time / 4f + 10.8f;
        
    }

    private void UpdateCarUITimer(float time)
    {
        if (car == null) return;
        car.UpdateTimer(time);
    }

    private void CarUnderGameOverSpeed()
    {
        if (car == null) return;
        if(car.currentSpeedKmH < gameOverSpeed)
        {
            OnPlayerStop();
        }
    }
}
