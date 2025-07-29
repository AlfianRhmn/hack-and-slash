using UnityEngine;
using System.Collections; 
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int enemiesPerWave = 3; 
    public float timeBetweenWaves = 5f; 
    public float spawnDelayPerEnemy = 0.5f; 

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private List<GameObject> currentEnemies = new List<GameObject>(); 
    private int currentWave = 0; 
    private bool waveInProgress = false; 
    void Start()
    {
        // Pastikan ada prefab musuh dan spawn points
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab is not assigned in EnemySpawner!");
            enabled = false; // Nonaktifkan skrip jika tidak ada prefab
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn Points are not assigned in EnemySpawner!");
            enabled = false; // Nonaktifkan skrip jika tidak ada spawn points
            return;
        }

        // Mulai gelombang pertama
        StartNextWave();
    }

    void Update()
    {
        // Jika ada musuh yang mati, hapus dari daftar
        // Loop mundur untuk menghindari masalah saat menghapus elemen dari list
        for (int i = currentEnemies.Count - 1; i >= 0; i--)
        {
            if (currentEnemies[i] == null || currentEnemies[i].GetComponent<Balmond>().HP <= 0) 
            {
                currentEnemies.RemoveAt(i);
            }
        }

        // Jika semua musuh dari gelombang saat ini sudah mati dan tidak ada gelombang baru yang sedang diproses
        if (currentEnemies.Count == 0 && waveInProgress)
        {
            Debug.Log($"Wave {currentWave} cleared! Preparing for next wave...");
            waveInProgress = false; // Tandai gelombang selesai
            StartCoroutine(WaitForNextWave()); // Tunggu sebelum memulai gelombang baru
        }
    }

    void StartNextWave()
    {
        currentWave++;
        Debug.Log($"Starting Wave {currentWave}!");
        waveInProgress = true;
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            // Pastikan masih ada spawn points yang tersedia
            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points available to spawn more enemies!");
                break;
            }

            // Pilih spawn point secara acak
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiate musuh
            GameObject newEnemy = Instantiate(enemyPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            currentEnemies.Add(newEnemy);

            // Jika musuh adalah Balmond dan PlayerManager memiliki Instance, set targetnya
            Balmond balmondScript = newEnemy.GetComponent<Balmond>();
            if (balmondScript != null && PlayerManager.Instance != null) // Asumsi PlayerManager sudah ada dan Singleton bekerja
            {
                // Kamu harus memiliki referensi ke objek Player di PlayerManager atau di sini
                // Contoh: balmondScript.target = PlayerManager.Instance.playerTransform;
                // Untuk contoh ini, kita bisa mencari objek dengan tag "Player"
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    balmondScript.target = playerObject.transform;
                }
                else
                {
                    Debug.LogWarning("Player object with 'Player' tag not found! Balmond will not have a target.");
                }
            }

            yield return new WaitForSeconds(spawnDelayPerEnemy); // Jeda sebelum spawn musuh berikutnya
        }
    }

    IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }

    // Fungsi ini bisa dipanggil dari luar jika Anda ingin mengatur ulang spawner
    public void ResetSpawner()
    {
        // Hancurkan semua musuh yang tersisa
        foreach (GameObject enemy in currentEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        currentEnemies.Clear();
        currentWave = 0;
        waveInProgress = false;
        StopAllCoroutines(); // Hentikan semua coroutine yang berjalan
        StartNextWave(); // Mulai dari gelombang pertama lagi
    }
}