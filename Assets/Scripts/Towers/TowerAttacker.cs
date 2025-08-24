using System;
using UnityEngine;

public class TowerAttacker : MonoBehaviour, ITowerAttack
{
    public event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    public void Attack(IDamagable[] targets)
    {
        foreach (var target in targets)
            target.TakeDamage(towerSO.Damage);

        OnAttack?.Invoke();
    }
}
