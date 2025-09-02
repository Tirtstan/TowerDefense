using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;

    [Header("Debug")]
    [SerializeField]
    private bool preventDamage;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => enemySO.Health;

    private void Awake()
    {
        CurrentHealth = enemySO.Health;
    }

    public void TakeDamage(float amount)
    {
        if (preventDamage)
            return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(this);
        EventBus.Instance.Publish(new OnEnemyHealthChanged(this));

        if (CurrentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddCurrency(enemySO.SoulAmount);

        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public EnemySO GetEnemySO() => enemySO;
}

public struct OnEnemyHealthChanged : IGameEvent
{
    public EnemyHealth EnemyHealth;

    public OnEnemyHealthChanged(EnemyHealth enemyHealth) => EnemyHealth = enemyHealth;
}
