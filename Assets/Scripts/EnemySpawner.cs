using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave")]
    public EnemyWave[] wave;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    public GameObject playerObject;

    [Header("UI")]
    public GameObject waveDisplay;
    public TextMeshProUGUI waveDisplayText;
    public Slider bossHealthBar;
    public TextMeshProUGUI bossName;
    public TextMeshProUGUI bossHealthText;

    private List<GameObject> currentEnemies = new List<GameObject>(); 
    private int currentWave = 0; 
    private bool waveInProgress = false;
    private EnemyBehaviour currentBoss;
    float currentVelocity;
    string divider = " / ";
    void Start()
    {
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
            if (currentEnemies[i] == null || currentEnemies[i].GetComponent<EnemyBehaviour>().currentHP <= 0) 
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

        if (currentBoss != null)
        {
            bossHealthBar.maxValue = currentBoss.maxHP;
            bossHealthBar.value = Mathf.SmoothDamp(bossHealthBar.value, currentBoss.currentHP, ref currentVelocity, 0.1f);
            if (bossName.text != currentBoss.enemyName)
            {
                bossName.text = currentBoss.enemyName;
            }
            bossHealthText.text = Mathf.RoundToInt(bossHealthBar.value) + divider + currentBoss.maxHP;
        }
    }

    void StartNextWave()
    {
        if (currentWave < wave.Length)
        {
            waveDisplayText.text = "Wave " + (currentWave + 1);
            waveDisplay.SetActive(true);
            StartCoroutine(waveDisplayCooldown());
            currentWave++;
            Debug.Log($"Starting Wave {currentWave}!");
            waveInProgress = true;
            StartCoroutine(SpawnWave());
        }
    }

    IEnumerator waveDisplayCooldown()
    {
        yield return new WaitForSeconds(3.9f);
        waveDisplay.SetActive(false);
        if (currentBoss != null)
        {
            bossHealthBar.maxValue = currentBoss.maxHP;
            bossHealthBar.value = currentBoss.currentHP;
            bossHealthBar.gameObject.SetActive(true);
        }
    }

    public void BossDeath()
    {
        bossHealthBar.GetComponent<Animator>().Play("HideSlider");
        StartCoroutine(WaitUntilSlider());
    }

    IEnumerator WaitUntilSlider()
    {
        yield return new WaitForSeconds(1f);
        bossHealthBar.gameObject.SetActive(false);
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < wave[currentWave - 1].enemy.Length; i++)
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
            GameObject newEnemy = Instantiate(wave[currentWave - 1].enemy[i], randomSpawnPoint.position, randomSpawnPoint.rotation);
            currentEnemies.Add(newEnemy);

            // Jika musuh adalah Balmond dan PlayerManager memiliki Instance, set targetnya
            EnemyBehaviour enemy = newEnemy.GetComponent<EnemyBehaviour>();
            if (enemy != null && PlayerManager.Instance != null) // Asumsi PlayerManager sudah ada dan Singleton bekerja
            {
                if (playerObject != null)
                {
                    enemy.target = playerObject.transform;
                }
                else
                {
                    Debug.LogWarning("Player object with 'Player' tag not found! Balmond will not have a target.");
                }
            }
            if (enemy.isBoss && wave[currentWave - 1].bossWave)
            {
                enemy.source = this;
                currentBoss = enemy;
                if (!waveDisplay.activeSelf)
                {
                    bossHealthBar.maxValue = currentBoss.maxHP;
                    bossHealthBar.value = currentBoss.currentHP;
                    bossHealthBar.gameObject.SetActive(true);
                }
            }

            yield return new WaitForSeconds(wave[currentWave - 1].spawnDelayPerEnemy); // Jeda sebelum spawn musuh berikutnya
        }
    }

    IEnumerator WaitForNextWave()
    {
        yield return new WaitForSeconds(wave[currentWave - 1].timeBetweenWaves);
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

[System.Serializable]
public class EnemyWave
{
    public GameObject[] enemy;
    public float timeBetweenWaves = 5f;
    public float spawnDelayPerEnemy = 0.5f;
    public bool bossWave; // add boss health bar to scene
}