using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamagable
{
    public static event Action<EnemySO> OnDeath;
    public static event Action<Enemy> OnHealthChanged;

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
        OnDeath?.Invoke(enemySO);
        Destroy(gameObject);
    }

    public EnemySO GetEnemySO() => enemySO;
}
