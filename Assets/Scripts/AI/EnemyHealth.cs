using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable, ISpawnable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;

    [Header("Debug")]
    [SerializeField]
    private bool preventDamage;
    public Transform Target => transform;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => enemySO.Health;
    public Spawner Spawner { get; set; }

    private void Awake()
    {
        CurrentHealth = enemySO.Health;
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
        if (EconomyManager.Instance != null) // TODO: separate this into different class
            EconomyManager.Instance.AddCurrency(enemySO.CurrencyDropAmount);

        OnDeath?.Invoke();

        if (Spawner != null)
            Spawner.ReturnToPool(this);
        else
            Destroy(gameObject);
    }

    public EnemySO GetEnemySO() => enemySO;
}

public struct OnEnemyHealthChanged : IGameEvent
{
    public EnemyHealth EnemyHealth;

    public OnEnemyHealthChanged(EnemyHealth enemyHealth) => EnemyHealth = enemyHealth;
}
