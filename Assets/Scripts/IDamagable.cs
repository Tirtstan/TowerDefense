using System;

public interface IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public void TakeDamage(float amount);
    public float GetHealthPercentage() => CurrentHealth / MaxHealth;
}
