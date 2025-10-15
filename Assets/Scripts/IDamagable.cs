using System;
using UnityEngine;

public interface IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;
    public Transform Target { get; }
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public void TakeDamage(float amount);
    public void Heal(float amount);
    public void Die();
    public float GetHealthPercentage() => CurrentHealth / MaxHealth;
}
