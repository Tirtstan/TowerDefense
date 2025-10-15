using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UfoSpawner : Spawner
{
    public override event Action<EnemyHealth> OnSpawned;

    public override void SpawnEnemy(Vector3 position)
    {
        EnemyHealth enemy = Pool.Get();
        enemy.transform.SetPositionAndRotation(position, Quaternion.identity);
        OnSpawned?.Invoke(enemy);
    }

    public override void ClearAll()
    {
        var enemiesToClear = new List<EnemyHealth>(activeEnemies);
        foreach (var enemy in enemiesToClear)
        {
            if (enemy == null)
                continue;

            if (enemy.TryGetComponent(out IDamagable damagable))
            {
                damagable.TakeDamage(damagable.MaxHealth);
            }
            else
            {
                ReturnToPool(enemy);
            }
        }

        activeEnemies.Clear();
    }
}
