using System;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public event Action<Tower> OnUpgraded;

    [Header("Tower")]
    [SerializeField]
    private TowerSO towerSO;

    [Header("Upgrade System")]
    [SerializeField]
    private UpgradePathSO upgradePath;

    public int CurrentLevel { get; private set; } = 0;
    public int MaxLevel => upgradePath != null ? upgradePath.MaxLevel : 0;

    private void Start()
    {
        TowerManager.Instance.RegisterTower(this);
    }

    public TowerSO GetTowerSO() => towerSO;

    public TowerStats GetEffectiveStats()
    {
        TowerStats baseStats = towerSO.Stats;

        if (CurrentLevel == 0 || upgradePath == null)
            return baseStats;

        // Accumulate all upgrade tiers from level 1 to CurrentLevel
        float totalHealthBonus = 0f;
        float totalDamageMultiplier = 1f;
        float totalRangeMultiplier = 1f;
        float totalAttackSpeedMultiplier = 1f;

        for (int level = 1; level <= CurrentLevel; level++)
        {
            UpgradeTier tier = upgradePath.GetTier(level);
            if (tier == null)
                continue;

            totalHealthBonus += tier.HealthBonus;
            totalDamageMultiplier *= tier.DamageMultiplier;
            totalRangeMultiplier *= tier.RangeMultiplier;
            totalAttackSpeedMultiplier *= tier.AttackSpeedMultiplier;
        }

        return new TowerStats
        {
            Health = baseStats.Health + totalHealthBonus,
            Cost = baseStats.Cost,
            Damage = baseStats.Damage * totalDamageMultiplier,
            Range = baseStats.Range * totalRangeMultiplier,
            AttackInterval = baseStats.AttackInterval / totalAttackSpeedMultiplier,
            TargetingType = baseStats.TargetingType
        };
    }

    public bool CanUpgrade() => upgradePath != null && CurrentLevel < MaxLevel;

    public int GetUpgradeCost()
    {
        if (!CanUpgrade())
            return 0;

        UpgradeTier nextTier = upgradePath.GetTier(CurrentLevel + 1);
        return nextTier != null ? nextTier.UpgradeCost : 0;
    }

    public bool TryUpgrade()
    {
        if (!CanUpgrade())
            return false;

        int cost = GetUpgradeCost();

        if (!EconomyManager.Instance.CanAfford(cost))
            return false;

        EconomyManager.Instance.Spend(cost);
        CurrentLevel++;

        OnUpgraded?.Invoke(this);
        Debug.Log($"{gameObject.name} upgraded to level {CurrentLevel}.");
        return true;
    }

    private void OnDestroy()
    {
        if (TowerManager.Instance != null)
            TowerManager.Instance.UnregisterTower(this);
    }
}
