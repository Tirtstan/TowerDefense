using System;
using UnityEngine;

public class TowerHealth : MonoBehaviour, IDamagable, IHealable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    [Header("Debug")]
    [SerializeField]
    private bool preventDamage;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => towerSO.Stats.Health;

    private void Awake()
    {
        CurrentHealth = towerSO.Stats.Health;
    }

    public void TakeDamage(float amount)
    {
        if (preventDamage && amount > 0)
            return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, towerSO.Stats.Health);
        OnHealthChanged?.Invoke(this);

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(float amount) => TakeDamage(-amount);

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public TowerSO GetTowerSO() => towerSO;
}
