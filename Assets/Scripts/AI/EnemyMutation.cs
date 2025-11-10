using System;
using UnityEngine;

[Serializable]
public enum EnemyMutationType
{
    None,

    [Tooltip("Increases speed, decreases health")]
    Swift,

    [Tooltip("Increases health, decreases speed")]
    Tough,

    [Tooltip("Increases range and damage (ranged only), decreases health")]
    Range,

    [Tooltip("Decreases health, damage, and difficulty rating")]
    Swarm
}

[Serializable]
public struct EnemyMutation
{
    public EnemyMutationType Type;
    public float HealthMultiplier;
    public float DamageMultiplier;
    public float SpeedMultiplier;
    public float VisionRangeMultiplier;
    public float AttackRangeMultiplier;
    public float AttackIntervalMultiplier;
    public float DifficultyRatingMultiplier;

    public static EnemyMutation CreateSwift()
    {
        return new EnemyMutation
        {
            Type = EnemyMutationType.Swift,
            HealthMultiplier = 0.7f,
            DamageMultiplier = 1f,
            SpeedMultiplier = 1.5f,
            VisionRangeMultiplier = 1f,
            AttackRangeMultiplier = 1f,
            AttackIntervalMultiplier = 0.7f,
            DifficultyRatingMultiplier = 0.9f
        };
    }

    public static EnemyMutation CreateTough()
    {
        return new EnemyMutation
        {
            Type = EnemyMutationType.Tough,
            HealthMultiplier = 1.8f,
            DamageMultiplier = 1f,
            SpeedMultiplier = 0.7f,
            VisionRangeMultiplier = 1f,
            AttackRangeMultiplier = 1f,
            AttackIntervalMultiplier = 1f,
            DifficultyRatingMultiplier = 1.3f
        };
    }

    public static EnemyMutation CreateRange()
    {
        return new EnemyMutation
        {
            Type = EnemyMutationType.Range,
            HealthMultiplier = 0.8f,
            DamageMultiplier = 1.4f,
            SpeedMultiplier = 1f,
            VisionRangeMultiplier = 1.3f,
            AttackRangeMultiplier = 1.5f,
            AttackIntervalMultiplier = 1f,
            DifficultyRatingMultiplier = 1.1f
        };
    }

    public static EnemyMutation CreateSwarm()
    {
        return new EnemyMutation
        {
            Type = EnemyMutationType.Swarm,
            HealthMultiplier = 0.5f,
            DamageMultiplier = 0.7f,
            SpeedMultiplier = 1f,
            VisionRangeMultiplier = 1f,
            AttackRangeMultiplier = 1f,
            AttackIntervalMultiplier = 1f,
            DifficultyRatingMultiplier = 0.6f
        };
    }

    public static EnemyMutation CreateNone()
    {
        return new EnemyMutation
        {
            Type = EnemyMutationType.None,
            HealthMultiplier = 1f,
            DamageMultiplier = 1f,
            SpeedMultiplier = 1f,
            VisionRangeMultiplier = 1f,
            AttackRangeMultiplier = 1f,
            AttackIntervalMultiplier = 1f,
            DifficultyRatingMultiplier = 1f
        };
    }
}
