using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("TrackRoot")]
    [SerializeField] private GameObject trackRoot;
    [SerializeField] private Transform spawnPoint;

    [Header("Systems")]
    [SerializeField] private LevelTimer levelTimer;  // drag your LevelTimer here

    public GameObject PlayerInstance { get; private set; }
    public Transform SpawnPoint => spawnPoint;

    void Awake()
    {
        if (levelTimer == null)
            levelTimer = FindObjectOfType<LevelTimer>();
    }

    void Start()
    {
        StartCoroutine(SpawnPlayerDelayed(0.5f));
    }

    public void SetSpawnPoint(Transform newSpawn)
    {
        // Create an empty object at the offset
        GameObject offsetSpawn = new GameObject("SpawnPoint_Offset");
        offsetSpawn.transform.position = newSpawn.position + Vector3.up * 8f;
        offsetSpawn.transform.rotation = newSpawn.rotation;
        offsetSpawn.transform.SetParent(newSpawn); // optional

        spawnPoint = offsetSpawn.transform;
    }

    public void SpawnPlayer()
    {
        Debug.Log("Spawning player");
        Transform firstSegment = trackRoot.transform.GetChild(0);
        SetSpawnPoint(firstSegment);

        // Reuse existing or instantiate new
        PlayerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        // if (PlayerInstance == null)
        // {
        //     // WirePlayer(PlayerInstance);
        // }
        // // else
        // // {
        // // }
        // 
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
