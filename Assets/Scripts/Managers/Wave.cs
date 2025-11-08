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
    private readonly int earlyGameThreshold;

    private readonly List<Spawner> selectedSpawners = new();
    private readonly Dictionary<Spawner, int> spawnerUsageCount = new();
    private readonly Dictionary<Spawner, int> spawnerMaxUsage = new();
    private float budgetRemaining;

    private const int MUTATION_START_WAVE = 5;
    private const float MAX_MUTATION_CHANCE = 0.35f;

    public Wave(
        int waveNumber,
        float baseBudget,
        float budgetIncreasePerWave,
        int bossWaveInterval,
        float bossWaveBudgetMultiplier,
        float baseSpawnInterval,
        int baseEnemiesToSpawn,
        int earlyGameThreshold
    )
    {
        WaveNumber = waveNumber;
        this.baseSpawnInterval = baseSpawnInterval;
        this.baseEnemiesToSpawn = baseEnemiesToSpawn;
        this.earlyGameThreshold = earlyGameThreshold;

        IsBossWave = waveNumber % bossWaveInterval == 0;

        // Calculate budget
        Budget = baseBudget + waveNumber * budgetIncreasePerWave;
        if (IsBossWave)
            Budget *= bossWaveBudgetMultiplier;

        budgetRemaining = Budget;

        switch (WaveNumber)
        {
            case 0:
                // First wave: Very slow, single enemies
                SpawnInterval = baseSpawnInterval * 3f;
                EnemiesToSpawnAtOnce = 1;
                break;
            case var n when n <= earlyGameThreshold:
                // Early waves: Slower spawns, fewer at once
                SpawnInterval = baseSpawnInterval * 2f;
                EnemiesToSpawnAtOnce = 1;
                break;
            case var n when n < 5:
                // Transition: Gradually increase pace
                SpawnInterval = baseSpawnInterval * 1.5f;
                EnemiesToSpawnAtOnce = 1;
                break;
            default:
                // Normal pacing
                SpawnInterval = baseSpawnInterval;
                EnemiesToSpawnAtOnce = baseEnemiesToSpawn;
                break;
        }
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

        availableSpawners.TryGetValue(EnemySpawnType.MeleeBasic, out var meleeA);
        availableSpawners.TryGetValue(EnemySpawnType.MeleeTank, out var meleeB);
        availableSpawners.TryGetValue(EnemySpawnType.RangedBasic, out var rangeA);

        if (IsBossWave)
        {
            // Boss waves: All enemy types, but no tanks before early game ends
            AddSpawnerToSelection(meleeA);
            if (WaveNumber > earlyGameThreshold)
                AddSpawnerToSelection(meleeB);
            AddSpawnerToSelection(rangeA);
        }
        else if (WaveNumber == 0)
        {
            // First wave: Only basic melee
            AddSpawnerToSelection(meleeA);
        }
        else if (WaveNumber <= earlyGameThreshold)
        {
            // Early game: Mostly basic, sometimes ranged
            // NO TANKS in early game
            AddSpawnerToSelection(meleeA);
            if (Random.value > 0.5f)
                AddSpawnerToSelection(rangeA);
        }
        else if (WaveNumber < 5)
        {
            // Transition (waves after early game): Basic, ranged, and tanks can appear
            AddSpawnerToSelection(meleeA);
            if (Random.value > 0.5f)
                AddSpawnerToSelection(rangeA);
            // Lower chance for tanks in transition waves
            if (Random.value > 0.7f)
                AddSpawnerToSelection(meleeB);
        }
        else
        {
            // Mid-late game (wave 5+): All types can appear
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
                // Exclude tanks in early game even in fallback
                if (WaveNumber <= earlyGameThreshold && IsTankSpawner(spawner))
                    continue;
                AddSpawnerToSelection(spawner);
            }
        }

        SetSpawnerLimits();

        Debug.Log($"Wave {WaveNumber}: Selected {selectedSpawners.Count} spawner types. Budget: {Budget}");
    }

    private void SetSpawnerLimits()
    {
        foreach (var spawner in selectedSpawners)
            InitializeSpawnerTracking(spawner);
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
        // Normal performance - but still respect early game pacing
        else
        {
            // Early game: Keep slower pacing even if player is doing well
            if (isEarlyGame)
            {
                SpawnInterval = baseSpawnInterval * 1.2f;
                EnemiesToSpawnAtOnce = Mathf.Min(baseEnemiesToSpawn, 1);
            }
            else
            {
                SpawnInterval = baseSpawnInterval;
                EnemiesToSpawnAtOnce = baseEnemiesToSpawn;
            }
        }
    }

    public void CounterPlayerTowers(
        Dictionary<TowerSO, int> towerCounts,
        IReadOnlyDictionary<EnemySpawnType, Spawner> allSpawners
    )
    {
        // Don't counter towers in early game
        if (WaveNumber <= earlyGameThreshold)
            return;

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
        allSpawners.TryGetValue(EnemySpawnType.MeleeBasic, out Spawner meleeA);
        allSpawners.TryGetValue(EnemySpawnType.RangedBasic, out Spawner rangeA);

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

        // Check if ANY spawner can fit the remaining budget (accounting for mutations)
        foreach (var spawner in selectedSpawners)
        {
            float baseCost = spawner.GetDifficultyCost();

            // Check base cost
            if (baseCost <= budgetRemaining)
                return true;

            // Check with cheapest mutation (Swarm)
            float swarmCost = baseCost * EnemyMutation.CreateSwarm().DifficultyRatingMultiplier;
            if (swarmCost <= budgetRemaining)
                return true;

            // Check with most expensive mutation (Tough) to ensure we can afford worst case
            float toughCost = baseCost * EnemyMutation.CreateTough().DifficultyRatingMultiplier;
            if (toughCost <= budgetRemaining)
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
        // Account for potential mutations by checking with the most expensive mutation
        var availableSpawners = new List<Spawner>();
        foreach (var spawner in selectedSpawners)
        {
            float baseCost = spawner.GetDifficultyCost();
            // Check if we can afford even with the most expensive mutation (Tough = 1.3x)
            float maxPossibleCost = baseCost * EnemyMutation.CreateTough().DifficultyRatingMultiplier;

            int currentUsage = spawnerUsageCount.ContainsKey(spawner) ? spawnerUsageCount[spawner] : 0;
            int maxUsage = spawnerMaxUsage.ContainsKey(spawner) ? spawnerMaxUsage[spawner] : int.MaxValue;

            if (maxPossibleCost <= budgetRemaining && currentUsage < maxUsage)
                availableSpawners.Add(spawner);
        }

        // If no spawners fit the budget with usage limits, try ignoring limits
        if (availableSpawners.Count == 0)
        {
            // Find cheapest spawner that fits budget even with worst mutation (ignore usage limits)
            Spawner cheapestSpawner = null;
            float cheapestMaxCost = float.MaxValue;

            foreach (var spawner in selectedSpawners)
            {
                float baseCost = spawner.GetDifficultyCost();
                float maxPossibleCost = baseCost * EnemyMutation.CreateTough().DifficultyRatingMultiplier;

                if (maxPossibleCost <= budgetRemaining && maxPossibleCost < cheapestMaxCost)
                {
                    cheapestMaxCost = maxPossibleCost;
                    cheapestSpawner = spawner;
                }
            }

            if (cheapestSpawner != null)
            {
                Debug.Log(
                    $"Spawner limits reached. Spawning cheapest available enemy (max cost: {cheapestMaxCost:F2})"
                );
                EnsureUsageTracking(cheapestSpawner);
                spawnerUsageCount[cheapestSpawner]++;
                return cheapestSpawner;
            }

            Debug.LogWarning($"Cannot find affordable spawner. Budget: {budgetRemaining:F2}. Ending wave.");
            budgetRemaining = 0;
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

    public bool TrySpendBudget(float cost, EnemyMutation mutation)
    {
        float adjustedCost = cost * mutation.DifficultyRatingMultiplier;
        if (budgetRemaining >= adjustedCost)
        {
            budgetRemaining -= adjustedCost;
            return true;
        }
        return false;
    }

    public float GetRemainingBudget() => budgetRemaining;

    public bool HasBudgetRemaining() => budgetRemaining > 0;

    public EnemyMutation GetMutationForSpawner(
        Spawner spawner,
        Dictionary<TowerSO, int> towerCounts,
        float playerHealthPercent,
        int activeEnemyCount,
        float budgetRemainingPercent
    )
    {
        // No mutations before wave 5
        if (WaveNumber < MUTATION_START_WAVE)
            return EnemyMutation.CreateNone();

        // Calculate mutation chance based on multiple factors
        float mutationChance = CalculateMutationChance(playerHealthPercent, activeEnemyCount, budgetRemainingPercent);

        // Roll for mutation
        if (Random.value > mutationChance)
            return EnemyMutation.CreateNone();

        // Determine mutation based on game state and strategy
        EnemySO enemySO = spawner.GetEnemySO();
        bool isRangedEnemy = enemySO.AttackRange > 2f;
        bool isTankEnemy = enemySO.Health >= 300f;

        // Strategic mutations based on player's tower setup
        if (towerCounts != null && towerCounts.Count > 0)
        {
            EnemyMutation strategicMutation = GetStrategicMutation(
                towerCounts,
                enemySO,
                isRangedEnemy,
                playerHealthPercent,
                budgetRemainingPercent
            );
            if (strategicMutation.Type != EnemyMutationType.None)
                return strategicMutation;
        }

        // Context-based mutations
        EnemyMutation contextMutation = GetContextualMutation(
            isRangedEnemy,
            isTankEnemy,
            playerHealthPercent,
            budgetRemainingPercent,
            activeEnemyCount
        );
        if (contextMutation.Type != EnemyMutationType.None)
            return contextMutation;

        // Fallback: Random selection weighted by wave progression
        return GetRandomMutation(isRangedEnemy, isTankEnemy);
    }

    private float CalculateMutationChance(float playerHealthPercent, int activeEnemyCount, float budgetRemainingPercent)
    {
        // Base chance increases with wave number
        float baseChance = Mathf.Clamp01((WaveNumber - MUTATION_START_WAVE) / 15f) * MAX_MUTATION_CHANCE;

        // Increase chance if player is doing well (above 75% health)
        if (playerHealthPercent > 0.75f)
            baseChance *= 1.3f;

        // Increase chance if there are many active enemies (pressure situation)
        if (activeEnemyCount > 10)
            baseChance *= 1.2f;

        // Increase chance if budget is running low (desperation spawns)
        if (budgetRemainingPercent < 0.3f)
            baseChance *= 1.15f;

        // Decrease chance if player is struggling (below 50% health)
        if (playerHealthPercent < 0.5f)
            baseChance *= 0.7f;

        // Boss waves have higher mutation chance
        if (IsBossWave)
            baseChance *= 1.5f;

        return Mathf.Clamp01(baseChance);
    }

    private EnemyMutation GetStrategicMutation(
        Dictionary<TowerSO, int> towerCounts,
        EnemySO enemySO,
        bool isRangedEnemy,
        float playerHealthPercent,
        float budgetRemainingPercent
    )
    {
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
            return EnemyMutation.CreateNone();

        float towerRange = mostCommonTower.Stats.Range;

        // Long-range towers (Catapult & Ballista) -> Swift enemies to rush past
        if (towerRange >= 7f && !isRangedEnemy)
        {
            // Higher chance if player is doing well
            if (playerHealthPercent > 0.6f && Random.value < 0.7f)
                return EnemyMutation.CreateSwift();
        }

        // Many short-range towers -> Range mutation on ranged enemies
        if (towerRange <= 4f && isRangedEnemy && maxCount >= 3)
        {
            if (Random.value < 0.6f)
                return EnemyMutation.CreateRange();
        }

        // Player has many towers -> Swarm mutation to overwhelm
        int totalTowers = 0;
        foreach (var count in towerCounts.Values)
            totalTowers += count;

        if (totalTowers >= 5 && budgetRemainingPercent > 0.5f)
        {
            if (Random.value < 0.5f)
                return EnemyMutation.CreateSwarm();
        }

        return EnemyMutation.CreateNone();
    }

    private EnemyMutation GetContextualMutation(
        bool isRangedEnemy,
        bool isTankEnemy,
        float playerHealthPercent,
        float budgetRemainingPercent,
        int activeEnemyCount
    )
    {
        // Low budget but need to spend -> Swarm mutation (cheaper)
        if (budgetRemainingPercent < 0.4f && Random.value < 0.6f)
            return EnemyMutation.CreateSwarm();

        // Many enemies already active -> Tough mutation for survivability
        if (activeEnemyCount > 8 && !isTankEnemy && Random.value < 0.4f)
            return EnemyMutation.CreateTough();

        // Player struggling -> Less aggressive mutations
        if (playerHealthPercent < 0.4f)
        {
            // Prefer Swarm (weaker) over Tough (stronger)
            if (Random.value < 0.5f)
                return EnemyMutation.CreateSwarm();
            return EnemyMutation.CreateNone();
        }

        // Player doing well -> More challenging mutations
        if (playerHealthPercent > 0.8f)
        {
            if (isRangedEnemy && Random.value < 0.5f)
                return EnemyMutation.CreateRange();
            if (!isTankEnemy && Random.value < 0.4f)
                return EnemyMutation.CreateTough();
        }

        return EnemyMutation.CreateNone();
    }

    private EnemyMutation GetRandomMutation(bool isRangedEnemy, bool isTankEnemy)
    {
        float rand = Random.value;

        // Ranged enemies have higher chance for Range mutation
        if (isRangedEnemy && rand < 0.35f)
            return EnemyMutation.CreateRange();

        // Weighted random selection
        if (rand < 0.25f)
            return EnemyMutation.CreateTough();
        else if (rand < 0.45f)
            return EnemyMutation.CreateSwift();
        else if (rand < 0.65f)
            return EnemyMutation.CreateSwarm();
        else if (isRangedEnemy && rand < 0.8f)
            return EnemyMutation.CreateRange();

        return EnemyMutation.CreateNone();
    }

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
