using System;
using System.Collections.Generic;

public interface IAttack
{
    public event Action OnAttack;
    public void Attack(IEnumerable<IDamagable> targets);
}
