using System;
using UnityEngine;

public class CenterTower : Singleton<CenterTower>
{
    [Header("Health")]
    [SerializeField]
    private TowerHealth towerHealth;

    protected override void Awake()
    {
        base.Awake();
    }

    public Vector3 GetPosition() => transform.position;
}
