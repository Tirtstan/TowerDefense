using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class UfoSpawner : Spawner
{
    public override event Action<EnemyHealth> OnSpawned;

    [Header("Spawning")]
    [SerializeField, Tooltip("The search radius to find a valid NavMesh position.")]
    private float navMeshSampleRadius = 2.0f;

    public override void SpawnEnemy(Vector3 position, Quaternion rotation)
    {
        EnemyHealth enemy = Pool.Get();
        if (enemy.TryGetComponent(out NavMeshAgent agent))
        {
            agent.enabled = false;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            {
                enemy.transform.SetPositionAndRotation(hit.position, rotation);
            }
            else
            {
                Debug.LogWarning(
                    $"Could not find a valid NavMesh position near {position} for enemy spawn. Spawning at original position.",
                    enemy.gameObject
                );
                enemy.transform.SetPositionAndRotation(position, rotation);
            }

            agent.enabled = true;
        }
        else
        {
            enemy.transform.SetPositionAndRotation(position, rotation);
        }

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
