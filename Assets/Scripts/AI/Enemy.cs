using System;
using UnityEngine;

public class Enemy : MonoBehaviour, ISpawnable
{
    public event Action<Enemy> OnMutationApplied;

    [Header("Enemy")]
    [SerializeField]
    private EnemySO enemySO;

    private EnemyMutation currentMutation = EnemyMutation.CreateNone();
    public Spawner Spawner { get; set; }

    public EnemySO GetEnemySO() => enemySO;

    public EnemyStats GetEffectiveStats()
    {
        if (enemySO == null)
            return default;

        var baseStats = new EnemyStats
        {
            Health = enemySO.Health,
            Damage = enemySO.Damage,
            Speed = enemySO.Speed,
            VisionRange = enemySO.VisionRange,
            AttackRange = enemySO.AttackRange,
            AttackInterval = enemySO.AttackInterval,
            DifficultyRating = enemySO.DifficultyRating
        };

        if (currentMutation.Type == EnemyMutationType.None)
            return baseStats;

        return new EnemyStats
        {
            Health = baseStats.Health * currentMutation.HealthMultiplier,
            Damage = baseStats.Damage * currentMutation.DamageMultiplier,
            Speed = baseStats.Speed * currentMutation.SpeedMultiplier,
            VisionRange = baseStats.VisionRange * currentMutation.VisionRangeMultiplier,
            AttackRange = baseStats.AttackRange * currentMutation.AttackRangeMultiplier,
            AttackInterval = baseStats.AttackInterval * currentMutation.AttackIntervalMultiplier,
            DifficultyRating = baseStats.DifficultyRating * currentMutation.DifficultyRatingMultiplier
        };
    }

    public void ApplyMutation(EnemyMutation mutation)
    {
        currentMutation = mutation;
        OnMutationApplied?.Invoke(this);

        gameObject.name = $"{enemySO.Name} ({mutation.Type})";
    }

    public EnemyMutation GetCurrentMutation() => currentMutation;

    public bool HasMutation() => currentMutation.Type != EnemyMutationType.None;
}
