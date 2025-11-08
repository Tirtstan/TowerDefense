using System.Collections.Generic;
using UnityEngine;

public enum EnemySpawnType
{
    MeleeBasic,
    MeleeTank,
    RangedBasic
}

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

    private const int REQUIRED_SPAWNER_TYPES = 3;

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

    public void SelectSpawners(IReadOnlyDictionary<EnemySpawnType, Spawner> availableSpawners)
    {
        selectedSpawners.Clear();
        spawnerUsageCount.Clear();
        spawnerMaxUsage.Clear();

        if (availableSpawners == null || availableSpawners.Count == 0)
        {
            Debug.LogWarning("No spawners provided for wave selection.");
            return;
        }

        if (availableSpawners.Count < REQUIRED_SPAWNER_TYPES)
        {
            Debug.LogWarning("Not enough spawners! Expected 3 (Melee A, Melee B, Range A)");
            foreach (var spawner in availableSpawners.Values)
            {
                AddSpawnerToSelection(spawner);
            }
            SetSpawnerLimits();
            return;
        }

        availableSpawners.TryGetValue(EnemySpawnType.MeleeBasic, out var meleeA);
        availableSpawners.TryGetValue(EnemySpawnType.MeleeTank, out var meleeB);
        availableSpawners.TryGetValue(EnemySpawnType.RangedBasic, out var rangeA);

        if (IsBossWave)
        {
            // Boss waves: All enemy types, prioritize tanks
            AddSpawnerToSelection(meleeA);
            AddSpawnerToSelection(meleeB);
            AddSpawnerToSelection(rangeA);
        }
        else if (WaveNumber == 0)
        {
            // First wave: Only basic melee
            AddSpawnerToSelection(meleeA);
        }
        else if (WaveNumber < 5)
        {
            // Early game (waves 1-4): Mostly basic, sometimes ranged
            AddSpawnerToSelection(meleeA);
            if (Random.value > 0.5f)
                AddSpawnerToSelection(rangeA);
        }
        else
        {
            // Mid-late game: All types can appear
            AddSpawnerToSelection(meleeA);

            // 60% chance to include ranged
            if (Random.value > 0.4f)
                AddSpawnerToSelection(rangeA);

            // 40% chance to include tanks
            if (Random.value > 0.6f)
                AddSpawnerToSelection(meleeB);
        }

        if (selectedSpawners.Count == 0)
        {
            Debug.LogWarning("No spawners selected after applying wave rules. Falling back to all available spawners.");
            foreach (var spawner in availableSpawners.Values)
            {
                AddSpawnerToSelection(spawner);
            }
        }

        SetSpawnerLimits();

        Debug.Log($"Wave {WaveNumber}: Selected {selectedSpawners.Count} spawner types. Budget: {Budget}");
    }

    private void SetSpawnerLimits()
    {
        foreach (var spawner in selectedSpawners)
        {
            InitializeSpawnerTracking(spawner);
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
            RemoveSpawnerIf(IsTankSpawner);
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

    public void CounterPlayerTowers(
        Dictionary<TowerSO, int> towerCounts,
        IReadOnlyDictionary<EnemySpawnType, Spawner> allSpawners
    )
    {
        if (towerCounts.Count == 0 || Random.value < 0.7f || allSpawners == null || allSpawners.Count == 0)
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
        allSpawners.TryGetValue(EnemySpawnType.MeleeBasic, out var meleeA);
        allSpawners.TryGetValue(EnemySpawnType.RangedBasic, out var rangeA);

        // Long-range towers -> Add fast melee enemies
        if (towerRange >= 7f)
        {
            if (AddSpawnerToSelection(meleeA))
                InitializeSpawnerTracking(meleeA);
        }
        // Short/Medium range towers -> Add ranged enemies
        else if (towerRange <= 6f)
        {
            if (AddSpawnerToSelection(rangeA))
                InitializeSpawnerTracking(rangeA);
        }
    }

    public bool CanSpawnAnyEnemy()
    {
        if (selectedSpawners.Count == 0)
            return false;

        // Check if ANY spawner can fit the remaining budget
        foreach (var spawner in selectedSpawners)
        {
            if (spawner.GetDifficultyCost() <= budgetRemaining)
                return true;
        }

        return false;
    }

    public Spawner GetNextSpawner()
    {
        // Safety check - this should never happen, but if it does, force wave end
        if (selectedSpawners.Count == 0)
        {
            Debug.LogError("No spawners selected! Forcing wave to end.");
            budgetRemaining = 0;
            return selectedSpawners.Count > 0 ? selectedSpawners[0] : null;
        }

        // If we can't afford ANY enemy, force the wave to end immediately
        if (!CanSpawnAnyEnemy())
        {
            Debug.Log($"Cannot afford any more enemies. Budget remaining: {budgetRemaining:F2}. Ending wave.");
            budgetRemaining = 0;
            // Return cheapest spawner anyway so we don't return null
            Spawner cheapest = selectedSpawners[0];
            foreach (var spawner in selectedSpawners)
            {
                if (spawner.GetDifficultyCost() < cheapest.GetDifficultyCost())
                    cheapest = spawner;
            }
            return cheapest;
        }

        // Filter spawners that fit remaining budget AND haven't reached their usage limit
        var availableSpawners = new List<Spawner>();
        foreach (var spawner in selectedSpawners)
        {
            float cost = spawner.GetDifficultyCost();
            int currentUsage = spawnerUsageCount.ContainsKey(spawner) ? spawnerUsageCount[spawner] : 0;
            int maxUsage = spawnerMaxUsage.ContainsKey(spawner) ? spawnerMaxUsage[spawner] : int.MaxValue;

            if (cost <= budgetRemaining && currentUsage < maxUsage)
                availableSpawners.Add(spawner);
        }

        // If no spawners fit the budget with usage limits, try ignoring limits
        if (availableSpawners.Count == 0)
        {
            // Find cheapest spawner that fits budget (ignore usage limits)
            Spawner cheapestSpawner = null;
            float cheapestCost = float.MaxValue;

            foreach (var spawner in selectedSpawners)
            {
                float cost = spawner.GetDifficultyCost();
                if (cost <= budgetRemaining && cost < cheapestCost)
                {
                    cheapestCost = cost;
                    cheapestSpawner = spawner;
                }
            }

            // This should always find something because CanSpawnAnyEnemy() returned true
            if (cheapestSpawner != null)
            {
                Debug.Log($"Spawner limits reached. Spawning cheapest available enemy (cost: {cheapestCost:F2})");
                EnsureUsageTracking(cheapestSpawner);
                spawnerUsageCount[cheapestSpawner]++;
                return cheapestSpawner;
            }

            // Fallback: This should never happen, but just in case
            Debug.LogWarning("Unexpected state: CanSpawnAnyEnemy was true but no spawner found. Using first spawner.");
            return selectedSpawners[0];
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

        EnsureUsageTracking(selectedSpawner);
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

    private bool AddSpawnerToSelection(Spawner spawner)
    {
        if (spawner == null)
            return false;

        if (selectedSpawners.Contains(spawner))
            return false;

        selectedSpawners.Add(spawner);
        return true;
    }

    private void InitializeSpawnerTracking(Spawner spawner)
    {
        if (spawner == null || spawnerUsageCount.ContainsKey(spawner))
            return;

        spawnerUsageCount[spawner] = 0;

        if (IsTankSpawner(spawner))
        {
            spawnerMaxUsage[spawner] = IsBossWave ? Random.Range(3, 6) : Random.Range(1, 4);
        }
        else
        {
            spawnerMaxUsage[spawner] = int.MaxValue;
        }
    }

    private void RemoveSpawnerIf(System.Predicate<Spawner> predicate)
    {
        for (int i = selectedSpawners.Count - 1; i >= 0; i--)
        {
            var spawner = selectedSpawners[i];
            if (predicate(spawner))
            {
                selectedSpawners.RemoveAt(i);
                spawnerUsageCount.Remove(spawner);
                spawnerMaxUsage.Remove(spawner);
            }
        }
    }

    private bool IsTankSpawner(Spawner spawner) => spawner != null && spawner.GetEnemySO().Health >= 300f;

    private void EnsureUsageTracking(Spawner spawner)
    {
        if (spawner == null)
            return;

        if (!spawnerUsageCount.ContainsKey(spawner))
            InitializeSpawnerTracking(spawner);
    }
}

#region Reference List
/*

Anthropic. 2025. Claude Sonnet (Version 4.5). [Large language model]. Available at: https://claude.ai/ [Accessed: 13 October 2025].

*/
#endregion
