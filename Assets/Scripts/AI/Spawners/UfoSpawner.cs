using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class UfoSpawner : Spawner
{
    public override event Action<EnemyHealth> OnSpawned;

    public override void SpawnEnemy(Vector3 position, Quaternion rotation)
    {
        EnemyHealth enemy = Pool.Get();

        // Disable NavMeshAgent before repositioning to avoid warnings
        if (enemy.TryGetComponent(out NavMeshAgent agent))
            agent.enabled = false;

        enemy.transform.SetPositionAndRotation(position, rotation);

        // Re-enable NavMeshAgent after positioning
        if (agent != null)
            agent.enabled = true;

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
