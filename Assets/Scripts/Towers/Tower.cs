using System;
using QFSW.QC;
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

        UpgradeTier tier = upgradePath.GetTier(CurrentLevel);
        if (tier == null)
            return baseStats;

        return new TowerStats
        {
            Health = baseStats.Health + tier.HealthBonus,
            Cost = baseStats.Cost,
            Damage = baseStats.Damage * tier.DamageMultiplier,
            Range = baseStats.Range * tier.RangeMultiplier,
            AttackInterval = baseStats.AttackInterval / tier.AttackSpeedMultiplier,
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
        Debug.Log($"{gameObject.name} upgraded to level {CurrentLevel + 1}.");
        return true;
    }

    private void OnDestroy()
    {
        if (TowerManager.Instance != null)
            TowerManager.Instance.UnregisterTower(this);
    }
}
