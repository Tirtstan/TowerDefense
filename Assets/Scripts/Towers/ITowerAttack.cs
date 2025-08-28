using System;
using System.Collections.Generic;

public interface ITowerAttack
{
    public event Action OnAttack;
    public void Attack(IEnumerable<IDamagable> targets);
}
