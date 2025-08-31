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
    private float currentHealth;

    private void Awake()
    {
        currentHealth = towerSO.Stats.Health;
    }

    public float GetCurrentHealth() => currentHealth;

    public float GetHealthPercentage() => currentHealth / towerSO.Stats.Health;

    public void TakeDamage(float amount)
    {
        if (preventDamage)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, towerSO.Stats.Health);
        OnHealthChanged?.Invoke(this);

        if (currentHealth <= 0)
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
