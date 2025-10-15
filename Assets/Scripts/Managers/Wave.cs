using System.Collections.Generic;
using UnityEngine;

// Anthropic, 2025
public class Wave
{
    public int WaveNumber { get; private set; }
    public float Budget { get; private set; }
    public bool IsBossWave { get; private set; }
    public float SpawnInterval { get; private set; }
    public int EnemiesToSpawnAtOnce { get; private set; }

    private readonly float baseSpawnInterval;
    private readonly int baseEnemiesToSpawn;

    private readonly List<Spawner> selectedSpawners = new();
    private readonly Dictionary<Spawner, int> spawnerUsageCount = new();
    private readonly Dictionary<Spawner, int> spawnerMaxUsage = new();
    private float budgetRemaining;

    // Enemy indices
    private const int MELEE_A_INDEX = 0; // Basic enemy
    private const int MELEE_B_INDEX = 1; // Tank/Boss enemy
    private const int RANGE_A_INDEX = 2; // Ranged enemy

    public Wave(
        int waveNumber,
        float baseBudget,
        float budgetIncreasePerWave,
        int bossWaveInterval,
        float bossWaveBudgetMultiplier,
        float baseSpawnInterval,
        int baseEnemiesToSpawn
    )
    {
        WaveNumber = waveNumber;
        this.baseSpawnInterval = baseSpawnInterval;
        this.baseEnemiesToSpawn = baseEnemiesToSpawn;

        IsBossWave = waveNumber % bossWaveInterval == 0;

        // Calculate budget
        Budget = baseBudget + waveNumber * budgetIncreasePerWave;
        if (IsBossWave)
            Budget *= bossWaveBudgetMultiplier;

        budgetRemaining = Budget;

        SpawnInterval = baseSpawnInterval;
        EnemiesToSpawnAtOnce = baseEnemiesToSpawn;
    }

    public void SelectSpawners(Spawner[] availableSpawners)
    {
        selectedSpawners.Clear();
        spawnerUsageCount.Clear();
        spawnerMaxUsage.Clear();

        if (availableSpawners.Length < 3)
        {
            Debug.LogWarning("Not enough spawners! Expected 3 (Melee A, Melee B, Range A)");
            selectedSpawners.AddRange(availableSpawners);
            SetSpawnerLimits();
            return;
        }

        Spawner meleeA = availableSpawners[MELEE_A_INDEX];
        Spawner meleeB = availableSpawners[MELEE_B_INDEX];
        Spawner rangeA = availableSpawners[RANGE_A_INDEX];

        if (IsBossWave)
        {
            // Boss waves: All enemy types, prioritize tanks
            selectedSpawners.Add(meleeA);
            selectedSpawners.Add(meleeB);
            selectedSpawners.Add(rangeA);
        }
        else if (WaveNumber == 0)
        {
            // First wave: Only basic melee
            selectedSpawners.Add(meleeA);
        }
        else if (WaveNumber < 5)
        {
            // Early game (waves 1-4): Mostly basic, sometimes ranged
            selectedSpawners.Add(meleeA);
            if (Random.value > 0.5f)
                selectedSpawners.Add(rangeA);
        }
        else
        {
            // Mid-late game: All types can appear
            selectedSpawners.Add(meleeA);

            // 60% chance to include ranged
            if (Random.value > 0.4f)
                selectedSpawners.Add(rangeA);

            // 40% chance to include tanks
            if (Random.value > 0.6f)
                selectedSpawners.Add(meleeB);
        }

        SetSpawnerLimits();

        Debug.Log($"Wave {WaveNumber}: Selected {selectedSpawners.Count} spawner types. Budget: {Budget}");
    }

    private void SetSpawnerLimits()
    {
        foreach (var spawner in selectedSpawners)
        {
            spawnerUsageCount[spawner] = 0;

            // Check if this is the tank enemy (index 1)
            if (spawner.GetEnemySO().Health >= 300f)
            {
                if (IsBossWave)
                {
                    // Boss waves: 3-5 tanks
                    spawnerMaxUsage[spawner] = Random.Range(3, 6);
                }
                else
                {
                    // Normal waves: 1-3 tanks max
                    spawnerMaxUsage[spawner] = Random.Range(1, 4);
                }
            }
            else
            {
                // No limit for basic and ranged enemies
                spawnerMaxUsage[spawner] = int.MaxValue;
            }
        }
    }

    public void AdjustDifficultyForPlayerPerformance(float playerHealthPercent, bool isEarlyGame)
    {
        // Player struggling (below 50% health)
        if (playerHealthPercent < 0.5f)
        {
            SpawnInterval = baseSpawnInterval * 1.5f; // Slower spawns
            EnemiesToSpawnAtOnce = Mathf.Max(1, baseEnemiesToSpawn - 1);

            // Remove tanks if player is struggling
            selectedSpawners.RemoveAll(s => s.GetEnemySO().Health >= 300f);
        }
        // Player doing well (above 75% health) and past early game
        else if (playerHealthPercent > 0.75f && !isEarlyGame)
        {
            // Randomly increase difficulty
            if (Random.value > 0.5f)
            {
                SpawnInterval = baseSpawnInterval * 0.8f; // Slightly faster spawns
            }
            else
            {
                EnemiesToSpawnAtOnce = baseEnemiesToSpawn + 1;
            }
        }
        // Normal performance
        else
        {
            SpawnInterval = baseSpawnInterval;
            EnemiesToSpawnAtOnce = baseEnemiesToSpawn;
        }
    }

    public void CounterPlayerTowers(Dictionary<TowerSO, int> towerCounts, Spawner[] allSpawners)
    {
        if (towerCounts.Count == 0 || Random.value < 0.7f || allSpawners.Length < 3)
            return;

        // Find most common tower type
        TowerSO mostCommonTower = null;
        int maxCount = 0;

        foreach (var kvp in towerCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostCommonTower = kvp.Key;
            }
        }

        if (mostCommonTower == null)
            return;

        float towerRange = mostCommonTower.Stats.Range;
        Spawner meleeA = allSpawners[MELEE_A_INDEX];
        Spawner rangeA = allSpawners[RANGE_A_INDEX];

        // Long-range towers -> Add fast melee enemies
        if (towerRange >= 7f)
        {
            if (!selectedSpawners.Contains(meleeA))
                selectedSpawners.Add(meleeA);
        }
        // Short/Medium range towers -> Add ranged enemies
        else if (towerRange <= 6f)
        {
            if (!selectedSpawners.Contains(rangeA))
            {
                selectedSpawners.Add(rangeA);
                spawnerUsageCount[rangeA] = 0;
                spawnerMaxUsage[rangeA] = int.MaxValue;
            }
        }
    }

    public Spawner GetNextSpawner()
    {
        if (selectedSpawners.Count == 0)
        {
            Debug.LogError("No spawners selected!");
            return null;
        }

        // Filter spawners that fit remaining budget AND haven't reached their usage limit
        var availableSpawners = new List<Spawner>();

        foreach (var spawner in selectedSpawners)
        {
            float cost = spawner.GetDifficultyCost();
            int currentUsage = spawnerUsageCount.ContainsKey(spawner) ? spawnerUsageCount[spawner] : 0;
            int maxUsage = spawnerMaxUsage.ContainsKey(spawner) ? spawnerMaxUsage[spawner] : int.MaxValue;

            if (cost <= budgetRemaining && currentUsage < maxUsage)
            {
                availableSpawners.Add(spawner);
            }
        }

        // If no spawners fit the budget, find the cheapest spawner that hasn't reached usage limit
        if (availableSpawners.Count == 0)
        {
            Spawner cheapestSpawner = null;
            float cheapestCost = float.MaxValue;

            foreach (var spawner in selectedSpawners)
            {
                int currentUsage = spawnerUsageCount.ContainsKey(spawner) ? spawnerUsageCount[spawner] : 0;
                int maxUsage = spawnerMaxUsage.ContainsKey(spawner) ? spawnerMaxUsage[spawner] : int.MaxValue;
                float cost = spawner.GetDifficultyCost();

                if (currentUsage < maxUsage && cost < cheapestCost)
                {
                    cheapestCost = cost;
                    cheapestSpawner = spawner;
                }
            }

            if (cheapestSpawner != null)
            {
                Debug.Log($"Budget too low ({budgetRemaining}). Spawning cheapest enemy (cost: {cheapestCost})");
                spawnerUsageCount[cheapestSpawner]++;
                return cheapestSpawner;
            }

            Debug.Log($"No available spawners. Budget remaining: {budgetRemaining}");
            return null;
        }

        // Weight selection towards basic enemies (70% basic, 30% others)
        Spawner selectedSpawner;
        if (availableSpawners.Count > 1 && Random.value > 0.3f)
        {
            // Try to select basic melee enemy
            var basicSpawner = availableSpawners.Find(s => s.GetEnemySO().Health < 100f);
            selectedSpawner =
                basicSpawner != null ? basicSpawner : availableSpawners[Random.Range(0, availableSpawners.Count)];
        }
        else
        {
            selectedSpawner = availableSpawners[Random.Range(0, availableSpawners.Count)];
        }

        spawnerUsageCount[selectedSpawner]++;
        return selectedSpawner;
    }

    public bool TrySpendBudget(float cost)
    {
        if (budgetRemaining >= cost)
        {
            budgetRemaining -= cost;
            return true;
        }
        return false;
    }

    public float GetRemainingBudget() => budgetRemaining;

    public bool HasBudgetRemaining() => budgetRemaining > 0;
}

#region Reference List
/*

Anthropic. 2025. Claude Sonnet (Version 4.5). [Large language model]. Available at: https://claude.ai/ [Accessed: 13 October 2025].

*/
#endregion
