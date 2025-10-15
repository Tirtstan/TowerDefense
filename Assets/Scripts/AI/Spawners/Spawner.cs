using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public abstract class Spawner : MonoBehaviour
{
    public abstract event Action<EnemyHealth> OnSpawned;
    public virtual event Action<EnemyHealth> OnReleased;

    [Header("Enemy")]
    [SerializeField]
    protected EnemyHealth enemyPrefab;

    [Header("Pool Settings")]
    [SerializeField]
    private int initialPoolSize = 10;

    [SerializeField]
    private int maxPoolSize = 100;
    protected virtual ObjectPool<EnemyHealth> Pool { get; private set; }
    protected readonly HashSet<EnemyHealth> activeEnemies = new();

    protected virtual void Awake()
    {
        Pool = new ObjectPool<EnemyHealth>(
            createFunc: CreateItem,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPooledItem,
            collectionCheck: true,
            initialPoolSize,
            maxPoolSize
        );
    }

    protected virtual EnemyHealth CreateItem()
    {
        EnemyHealth enemy = Instantiate(enemyPrefab);
        enemy.gameObject.SetActive(false);

        // Disable NavMeshAgent on creation to avoid errors
        if (enemy.TryGetComponent(out NavMeshAgent agent))
            agent.enabled = false;

        return enemy;
    }

    protected virtual void OnGetFromPool(EnemyHealth enemy)
    {
        // Don't activate yet - let the spawner position it first
        // The spawner will handle NavMeshAgent enabling
        enemy.gameObject.SetActive(true);
        enemy.Spawner = this;
        activeEnemies.Add(enemy);
    }

    protected virtual void OnReturnToPool(EnemyHealth enemy)
    {
        // Disable NavMeshAgent before deactivating
        if (enemy.TryGetComponent(out NavMeshAgent agent))
            agent.enabled = false;

        enemy.gameObject.SetActive(false);
        activeEnemies.Remove(enemy);

        if (enemy.TryGetComponent(out IDamagable damagable))
            damagable.Heal(damagable.MaxHealth);
    }

    protected virtual void OnDestroyPooledItem(EnemyHealth enemy)
    {
        if (enemy == null)
            return;

        Destroy(enemy.gameObject);
    }

    public abstract void SpawnEnemy(Vector3 position, Quaternion rotation);

    public virtual void ReturnToPool(EnemyHealth enemyHealth)
    {
        OnReleased?.Invoke(enemyHealth);
        Pool.Release(enemyHealth);
    }

    public abstract void ClearAll();

    public EnemySO GetEnemySO() => enemyPrefab.GetEnemySO();

    public float GetDifficultyCost() => enemyPrefab.GetEnemySO().DifficultyRating;
}
