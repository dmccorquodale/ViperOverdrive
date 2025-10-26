using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Car")]
    [SerializeField] private Car Car;

    [Header("TrackRoot")]
    [SerializeField] private GameObject trackRoot;
    [SerializeField] private Transform spawnPoint;

    [Header("Systems")]
    [SerializeField] private LevelTimer levelTimer;  // drag your LevelTimer here

    public GameObject PlayerInstance { get; private set; }
    public Transform SpawnPoint => spawnPoint;

    [Header("Camera")]
    [SerializeField] private GameObject mainCamera;

    void Awake()
    {
        // if (levelTimer == null)
            // levelTimer = FindObjectOfType<LevelTimer>();
    }

    void Start()
    {
        StartCoroutine(SpawnPlayerDelayed(3f));
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
        Car = PlayerInstance.GetComponent<Car>();
        if (Car == null)
        {
            Debug.LogError("LevelManager: Spawned playerPrefab has no Car component.");
            return;
        }
        Car.Go();

        // Optionally wire up other bits
        // WirePlayer(PlayerInstance);
        // MovePlayerToSpawn(PlayerInstance);
    }

    IEnumerator SpawnPlayerDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnPlayer();
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
}
