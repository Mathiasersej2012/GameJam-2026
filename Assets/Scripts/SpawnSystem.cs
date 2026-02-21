using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public bool isFlying;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public List<SpawnEntry> enemies = new List<SpawnEntry>();
        [Min(0f)] public float spawnInterval = 1f;
        [Min(0f)] public float waveDuration = 5f;
    }

    [Header("Spawn Points")]
    [SerializeField] private List<Transform> groundSpawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> flyingSpawnPoints = new List<Transform>();

    [Header("Waves")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private bool startOnAwake = true;

    private Coroutine spawnRoutine;

    private void Start()
    {
        if (startOnAwake)
        {
            spawnRoutine = StartCoroutine(SpawnWaves());
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnWaves());
        }
    }

    private IEnumerator SpawnWaves()
    {
        if (waves == null || waves.Count == 0)
        {
            yield break;
        }

        for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
        {
            Wave wave = waves[waveIndex];
            if (wave == null)
            {
                continue;
            }

            float waveStartTime = Time.time;
            List<SpawnEntry> enemiesInWave = wave.enemies;

            if (enemiesInWave != null)
            {
                for (int i = 0; i < enemiesInWave.Count; i++)
                {
                    SpawnEntry entry = enemiesInWave[i];

                    if (entry != null && entry.prefab != null)
                    {
                        Transform spawnPoint = GetRandomSpawnPoint(entry.isFlying);
                        if (spawnPoint != null)
                        {
                            Instantiate(entry.prefab, spawnPoint.position, spawnPoint.rotation);
                        }
                    }

                    if (i < enemiesInWave.Count - 1 && wave.spawnInterval > 0f)
                    {
                        yield return new WaitForSeconds(wave.spawnInterval);
                    }
                }
            }

            float elapsed = Time.time - waveStartTime;
            float remainingWaveTime = wave.waveDuration - elapsed;
            if (remainingWaveTime > 0f)
            {
                yield return new WaitForSeconds(remainingWaveTime);
            }
        }

        spawnRoutine = null;
    }

    private Transform GetRandomSpawnPoint(bool isFlying)
    {
        List<Transform> spawnPoints = isFlying ? flyingSpawnPoints : groundSpawnPoints;
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex];
    }
}
