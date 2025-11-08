using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public sealed class UfoSpawner : Spawner
{
    [Header("Spawning")]
    [SerializeField, Tooltip("The search radius to find a valid NavMesh position.")]
    private float navMeshSampleRadius = 2.0f;

    protected override Vector3 GetSpawnPosition(Vector3 requestedPosition)
    {
        if (NavMesh.SamplePosition(requestedPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            return hit.position;

        Debug.LogWarning(
            $"Could not find a valid NavMesh position near {requestedPosition}. Using requested position.",
            this
        );
        return requestedPosition;
    }

    public override void ClearAll()
    {
        var enemiesToClear = new List<Enemy>(activeEnemies);
        foreach (var enemy in enemiesToClear)
        {
            if (enemy == null)
                continue;

            if (enemy.TryGetComponent(out EnemyHealth enemyHealth))
                enemyHealth.TakeDamage(enemyHealth.MaxHealth);

            ReturnToPool(enemy);
        }

        activeEnemies.Clear();
    }
}
