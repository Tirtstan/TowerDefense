using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttacker : MonoBehaviour, IAttack
{
    public event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private EnemySO enemySO;

    public void Attack(IEnumerable<IDamagable> targets)
    {
        foreach (var item in targets)
            item.TakeDamage(enemySO.Damage);

        OnAttack?.Invoke();
    }
}
