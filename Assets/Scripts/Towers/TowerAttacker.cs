using System;
using UnityEngine;

public class TowerAttacker : MonoBehaviour, ITowerAttack
{
    public event Action OnAttack;

    [Header("Components")]
    [SerializeField]
    private TowerSO towerSO;

    public void Attack(IDamagable target)
    {
        target.TakeDamage(towerSO.Damage);
        OnAttack?.Invoke();
    }
}
