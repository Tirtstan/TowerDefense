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
    public float MaxHealth => tower.GetTowerSO().Stats.Health;
    private TowerSO towerSO;

    private void Awake()
    {
        towerSO = tower.GetTowerSO();
        CurrentHealth = MaxHealth;
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

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public TowerSO GetTowerSO() => towerSO;

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
