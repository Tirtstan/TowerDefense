using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyAttacker : EnemyAttack
{
    public override event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private Enemy enemy;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();
    }

    public override void Attack(IEnumerable<IDamagable> targets)
    {
        if (enemy == null)
            return;

        float damage = enemy.GetEffectiveStats().Damage;
        foreach (var item in targets)
            item.TakeDamage(damage);

        OnAttack?.Invoke();
    }

    private void Reset()
    {
        if (enemy == null)
            enemy = GetComponent<Enemy>();
    }
}
