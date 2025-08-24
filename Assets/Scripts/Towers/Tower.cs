using System;
using UnityEngine;

public class Tower : MonoBehaviour, IDamagable, IHealable
{
    public static event Action<TowerSO> OnDestroyed;
    public static event Action<Tower> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    [Header("Debug")]
    [SerializeField]
    private bool preventDamage;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = towerSO.Health;
    }

    public float GetCurrentHealth() => currentHealth;

    public float GetHealthPercentage() => currentHealth / towerSO.Health;

    public void TakeDamage(float amount)
    {
        if (preventDamage)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, towerSO.Health);
        OnHealthChanged?.Invoke(this);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount) => TakeDamage(-amount);

    public TowerSO GetTowerSO() => towerSO;

    private void Die()
    {
        OnDestroyed?.Invoke(towerSO);
        Destroy(gameObject);
    }
}
