using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyAttacker : EnemyAttack
{
    public override event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;

    public override void Attack(IEnumerable<IDamagable> targets)
    {
        foreach (var item in targets)
            item.TakeDamage(enemySO.Damage);

        OnAttack?.Invoke();
    }
}
