
using UnityEngine;
using UnityEngine.AI;

public class WayPointSystem : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject[] zombies;

    [Header("Spawn Settings")]
    public Transform[] Spawnpoints;
    public Transform player;
    public float minPlayerDistance = 15f;
    public float avoidFrontAngle = 100f;

    [Header("Wave Settings")]
    public float spawnInterval = 5f;
    public float waveTimer;
    public int waveNumber = 1;
    public int ZombiePerWave = 11;

    private void Update()
    {
        if (waveNumber >= 10)
            return;

        waveTimer += Time.deltaTime;

        if (waveTimer >= spawnInterval)
        {
            waveTimer = 0f;
            StartNewWave();
        }
    }

    private void StartNewWave()
    {
        if (zombies == null || zombies.Length == 0)
        {
            Debug.LogError("No zombie prefabs assigned!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is not assigned!");
            return;
        }

        if (Spawnpoints == null || Spawnpoints.Length == 0)
        {
            Debug.LogError("No spawnpoints assigned!");
            return;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.SetCurrentWave(waveNumber);
        }

        Debug.Log("Starting Wave: " + waveNumber);

        for (int i = 0; i < ZombiePerWave; i++)
        {
            Transform spawnPoint = GetValidSpawnPoint();

            if (spawnPoint == null)
            {
                Debug.LogWarning("No valid spawnpoint found!");
                continue;
            }

            // Randomly select a zombie prefab.
            GameObject selectedZombie =
                zombies[Random.Range(0, zombies.Length)];

            if (selectedZombie == null)
            {
                Debug.LogWarning("Selected zombie prefab is missing!");
                continue;
            }

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                spawnPoint.position,
                out hit,
                5f,
                NavMesh.AllAreas))
            {
                Instantiate(
                    selectedZombie,
                    hit.position,
                    spawnPoint.rotation
                );
            }
            else
            {
                Debug.LogWarning(
                    "No NavMesh found near spawnpoint: " +
                    spawnPoint.name
                );
            }
        }

        // Increase the number of zombies for the next wave.
        ZombiePerWave += 4;
        waveNumber++;
    }

    private Transform GetValidSpawnPoint()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            Transform spawnPoint =
                Spawnpoints[Random.Range(0, Spawnpoints.Length)];

            if (spawnPoint == null)
                continue;

            float distance = Vector3.Distance(
                player.position,
                spawnPoint.position
            );

            // Ensure zombies spawn away from the player.
            if (distance < minPlayerDistance)
                continue;

            // Avoid spawning directly in front of the player.
            Vector3 direction =
                spawnPoint.position - player.position;

            direction.y = 0f;

            float angle = Vector3.Angle(
                player.forward,
                direction
            );

            if (angle < avoidFrontAngle / 2f)
                continue;

            return spawnPoint;
        }

        return null;
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("TimeBetweenWaves"))
        {
            spawnInterval =
                PlayerPrefs.GetFloat("TimeBetweenWaves");
        }

        if (PlayerPrefs.HasKey("ZombiePerWave"))
        {
            ZombiePerWave =
                PlayerPrefs.GetInt("ZombiePerWave");
        }
    }
}
