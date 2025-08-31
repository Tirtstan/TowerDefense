using System;

public interface IDamagable
{
    public event Action OnDeath;
    public event Action<IDamagable> OnHealthChanged;
    public void TakeDamage(float amount);
}
