using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;

    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = enemySO.Health;
    }

    public float GetCurrentHealth() => currentHealth;

    public float GetHealthPercentage() => currentHealth / enemySO.Health;

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, enemySO.Health);
        OnHealthChanged?.Invoke(this);

        if (currentHealth <= 0)
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
