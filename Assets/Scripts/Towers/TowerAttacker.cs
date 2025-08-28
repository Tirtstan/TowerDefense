using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttacker : MonoBehaviour, ITowerAttack
{
    public event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    public void Attack(IEnumerable<IDamagable> targets)
    {
        foreach (var target in targets)
            target.TakeDamage(towerSO.Stats.Damage);

        OnAttack?.Invoke();
    }
}
