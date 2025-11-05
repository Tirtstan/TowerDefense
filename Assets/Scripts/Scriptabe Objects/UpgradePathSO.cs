using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade Path", menuName = "Scriptable Objects/Upgrade Path")]
public class UpgradePathSO : ScriptableObject
{
    [Header("Upgrade Tiers")]
    [Tooltip("Array of upgrade tiers. Index 0 = Level 1, Index 1 = Level 2, etc.")]
    public UpgradeTier[] Tiers = new UpgradeTier[2];

    public int MaxLevel => Tiers.Length;

    public UpgradeTier GetTier(int level)
    {
        if (level <= 0 || level > Tiers.Length)
            return null;

        return Tiers[level - 1];
    }
}

[System.Serializable]
public class UpgradeTier
{
    [Header("Cost")]
    public int UpgradeCost = 100;

    [Header("Multipliers")]
    [Tooltip("Multiplies base damage.")]
    public float DamageMultiplier = 1f;

    [Tooltip("Multiplies base range.")]
    public float RangeMultiplier = 1f;

    [Tooltip("Multiplies attack speed (higher = faster attacks, lower interval).")]
    public float AttackSpeedMultiplier = 1f;

    [Header("Bonuses")]
    [Tooltip("Adds flat health bonus.")]
    public float HealthBonus = 0f;
}
