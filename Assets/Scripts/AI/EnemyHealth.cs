using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;
    public static event Action<EnemyHealth> OnHealthChangedStatic;

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
        OnHealthChangedStatic?.Invoke(this);

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
