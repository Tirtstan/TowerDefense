using System;

public interface ITowerAttack
{
    public event Action OnAttack;
    public void Attack(IDamagable[] targets);
}
