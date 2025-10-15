using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Anthropic, 2025
public class WaveManager : Singleton<WaveManager>
{
    public static event Action<Wave> OnWaveStarted;
    public static event Action<Wave> OnWaveCompleted;

    [Header("Components")]
    [SerializeField]
    private Spawner[] enemySpawners;

    [Header("Wave Settings")]
    [SerializeField]
    private float timeBetweenWaves = 5f;

    [SerializeField, Tooltip("The interval between each spawn check within a wave.")]
    private float baseSpawnInterval = 1f;

    [SerializeField, Tooltip("Base number of enemies to spawn at once.")]
    private int baseEnemiesToSpawn = 1;

    [Header("Budget")]
    [SerializeField]
    private float minBudget = 10;

    [SerializeField, Tooltip("Budget increases by this amount each wave.")]
    private float budgetIncreasePerWave = 5f;

    [Header("Boss Waves")]
    [SerializeField, Tooltip("A boss wave will occur every N waves.")]
    private int bossWaveInterval = 5;

    [SerializeField, Tooltip("Budget multiplier for boss waves.")]
    [Range(1f, 3f)]
    private float bossWaveBudgetMultiplier = 1.5f;

    [Header("Rewards")]
    [SerializeField]
    private int waveCompletionReward = 40;

    [SerializeField, Tooltip("Currency reward for completing a boss wave.")]
    private int bossWaveReward = 150;

    [Header("Balance")]
    [SerializeField, Tooltip("Wave number considered 'early game'.")]
    private int earlyGameThreshold = 2;

    private readonly List<Transform> spawnPoints = new();
    private readonly List<EnemyHealth> activeEnemies = new();
    private Coroutine spawnCoroutine;
    private int currentWaveIndex = -1;
    private Wave currentWave;

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameEnd += OnGameEnd;

        foreach (var spawner in enemySpawners)
        {
            spawner.OnSpawned += AddActiveEnemy;
            spawner.OnReleased += RemoveActiveEnemy;
        }
    }

    public int GetCurrentWaveIndex() => currentWaveIndex;

    private void OnGameStart()
    {
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        var wavesInterval = new WaitForSeconds(timeBetweenWaves);
        while (true)
        {
            currentWaveIndex++;
            currentWave = new Wave(
                currentWaveIndex,
                minBudget,
                budgetIncreasePerWave,
                bossWaveInterval,
                bossWaveBudgetMultiplier,
                baseSpawnInterval,
                baseEnemiesToSpawn
            );

            OnWaveStarted?.Invoke(currentWave);
            currentWave.SelectSpawners(enemySpawners);
            Debug.Log($"Starting Wave {currentWaveIndex + 1}");

            // adjust difficulty based on player performance
            float playerHealthPercent = GetPlayerHealthPercentage();
            bool isEarlyGame = currentWaveIndex <= earlyGameThreshold;
            currentWave.AdjustDifficultyForPlayerPerformance(playerHealthPercent, isEarlyGame);

            // counter player's tower strategy
            currentWave.CounterPlayerTowers(GetPlayerTowerCounts(), enemySpawners);

            yield return StartCoroutine(WaveExecutionRoutine());
            OnWaveCompleted?.Invoke(currentWave);

            int rewardAmount = currentWave.IsBossWave ? bossWaveReward : waveCompletionReward;
            EconomyManager.Instance.AddCurrency(rewardAmount);

            yield return wavesInterval;
        }
    }

    private IEnumerator WaveExecutionRoutine()
    {
        var interval = new WaitForSeconds(currentWave.SpawnInterval);
        while (currentWave.HasBudgetRemaining() || activeEnemies.Count > 0)
        {
            if (currentWave.HasBudgetRemaining() && spawnPoints.Count > 0)
            {
                for (int i = 0; i < currentWave.EnemiesToSpawnAtOnce; i++)
                {
                    if (!currentWave.HasBudgetRemaining())
                        break;

                    Spawner spawnerToUse = currentWave.GetNextSpawner();
                    if (spawnerToUse != null)
                    {
                        float cost = spawnerToUse.GetDifficultyCost();
                        if (currentWave.TrySpendBudget(cost))
                        {
                            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                            spawnerToUse.SpawnEnemy(spawnPoint.position, spawnPoint.rotation);
                        }
                    }
                }
            }

            yield return interval;
        }
    }

    private float GetPlayerHealthPercentage()
    {
        if (CenterTower.Instance != null)
        {
            if (CenterTower.Instance.TryGetComponent(out IDamagable damageable))
                return damageable.GetHealthPercentage();
        }

        return 1f;
    }

    private Dictionary<TowerSO, int> GetPlayerTowerCounts() => TowerManager.Instance.GetTowersPlaced();

    private void OnGameEnd()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    public void RegisterSpawnPoint(Transform transform) => spawnPoints.Add(transform);

    public void UnregisterSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    private void AddActiveEnemy(EnemyHealth enemy) => activeEnemies.Add(enemy);

    private void RemoveActiveEnemy(EnemyHealth enemy) => activeEnemies.Remove(enemy);

    private void OnDestroy()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameEnd -= OnGameEnd;

        foreach (var spawner in enemySpawners)
        {
            spawner.OnSpawned -= AddActiveEnemy;
            spawner.OnReleased -= RemoveActiveEnemy;
        }
    }
}

#region Reference List
/*

Anthropic. 2025. Claude Sonnet (Version 4.5). [Large language model]. Available at: https://claude.ai/ [Accessed: 13 October 2025].

*/
#endregion
