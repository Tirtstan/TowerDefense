using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private Enemy enemy;

    [Header("Debug")]
    [SerializeField]
    private bool preventDamage;
    public Transform Target => transform;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => enemy != null ? enemy.GetEffectiveStats().Health : 0f;
    private float previousMaxHealth;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();

        if (enemy != null)
        {
            CurrentHealth = enemy.GetEffectiveStats().Health;
            enemy.OnMutationApplied += OnMutationApplied;
        }

        previousMaxHealth = MaxHealth;
    }

    private void OnEnable()
    {
        // Ensure health is set correctly when enabled (in case mutation was applied before activation)
        if (enemy != null && CurrentHealth <= 0f)
            CurrentHealth = enemy.GetEffectiveStats().Health;
    }

    private void OnMutationApplied(Enemy mutatedEnemy)
    {
        float newMaxHealth = mutatedEnemy.GetEffectiveStats().Health;
        float healthIncrease = newMaxHealth - previousMaxHealth;
        if (healthIncrease > 0)
        {
            Heal(healthIncrease);
            previousMaxHealth = newMaxHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        if (preventDamage && amount > 0)
            return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(this);
        EventBus.Instance.Publish(new OnEnemyHealthChanged(this));

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(float amount) => TakeDamage(-amount);

    public void Die()
    {
        OnDeath?.Invoke();

        if (enemy != null && enemy.Spawner != null)
            enemy.Spawner.ReturnToPool(enemy);
        else
            Destroy(gameObject);
    }

    public EnemySO GetEnemySO() => enemy != null ? enemy.GetEnemySO() : null;
}

public struct OnEnemyHealthChanged : IGameEvent
{
    public EnemyHealth EnemyHealth;

    public OnEnemyHealthChanged(EnemyHealth enemyHealth) => EnemyHealth = enemyHealth;
}
