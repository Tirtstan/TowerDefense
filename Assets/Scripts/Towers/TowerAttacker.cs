using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttacker : TowerAttack
{
    public override event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private Tower tower;

    public override void Attack(IEnumerable<IDamagable> targets)
    {
        foreach (var target in targets)
            target.TakeDamage(tower.GetTowerSO().Stats.Damage);

        OnAttack?.Invoke();
    }

    private void Reset()
    {
        if (tower == null)
            tower = GetComponent<Tower>();
    }
}
