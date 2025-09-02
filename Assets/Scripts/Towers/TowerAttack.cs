using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerAttack : MonoBehaviour, IAttack
{
    public abstract event Action OnAttack;

    public abstract void Attack(IEnumerable<IDamagable> targets);
}
