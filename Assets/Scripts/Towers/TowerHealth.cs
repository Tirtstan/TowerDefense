using System;
using UnityEngine;

public class TowerHealth : MonoBehaviour, IDamagable, IHealable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private Tower tower;

    [Header("Debug")]
    [SerializeField]
    private bool preventDamage;
    public Transform Target => transform;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => tower.GetEffectiveStats().Health;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
        tower.OnUpgraded += OnTowerUpgraded;
    }

    private void OnTowerUpgraded(Tower tower)
    {
        float healthPercent = CurrentHealth / MaxHealth;
        float newMaxHealth = tower.GetEffectiveStats().Health;
        Heal(newMaxHealth * healthPercent - CurrentHealth);
    }

    public void TakeDamage(float amount)
    {
        if (preventDamage && amount > 0)
            return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        OnHealthChanged?.Invoke(this);
        EventBus.Instance.Publish(new OnTowerHealthChanged(this));

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(float amount) => TakeDamage(-amount);

    public void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public TowerSO GetTowerSO() => tower.GetTowerSO();

    private void OnDestroy()
    {
        tower.OnUpgraded -= OnTowerUpgraded;
    }

    private void Reset()
    {
        if (tower == null)
            tower = GetComponent<Tower>();
    }
}

public struct OnTowerHealthChanged : IGameEvent
{
    public TowerHealth TowerHealth;

    public OnTowerHealthChanged(TowerHealth towerHealth) => TowerHealth = towerHealth;
}
