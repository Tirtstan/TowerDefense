using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public abstract class Spawner : MonoBehaviour
{
    public virtual event Action<Enemy> OnSpawned;
    public virtual event Action<Enemy> OnReleased;

    [Header("Enemy")]
    [SerializeField]
    protected Enemy enemyPrefab;

    [Header("Pool Settings")]
    [SerializeField]
    private int initialPoolSize = 10;

    [SerializeField]
    private int maxPoolSize = 100;
    protected virtual ObjectPool<Enemy> Pool { get; private set; }
    protected readonly HashSet<Enemy> activeEnemies = new();

    protected virtual void Awake()
    {
        Pool = new ObjectPool<Enemy>(
            createFunc: CreateItem,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPooledItem,
            collectionCheck: true,
            initialPoolSize,
            maxPoolSize
        );
    }

    protected virtual Enemy CreateItem()
    {
        Enemy enemy = Instantiate(enemyPrefab, Vector3.right * 18f, Quaternion.identity);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    protected virtual void OnGetFromPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemy.Spawner = this;
        activeEnemies.Add(enemy);
    }

    protected virtual void OnReturnToPool(Enemy enemy)
    {
        if (enemy.TryGetComponent(out NavMeshAgent agent))
            agent.enabled = false;

        enemy.gameObject.SetActive(false);
        activeEnemies.Remove(enemy);

        // Reset mutation when returning to pool
        enemy.ApplyMutation(EnemyMutation.CreateNone());

        if (enemy.TryGetComponent(out EnemyHealth enemyHealth))
        {
            if (enemyHealth.TryGetComponent(out IDamagable damagable))
                damagable.Heal(damagable.MaxHealth);
        }
    }

    protected virtual void OnDestroyPooledItem(Enemy enemy)
    {
        if (enemy == null)
            return;

        Destroy(enemy.gameObject);
    }

    public virtual void SpawnEnemy(Vector3 position, Quaternion rotation, EnemyMutation mutation)
    {
        Enemy enemy = Pool.Get();

        enemy.ApplyMutation(mutation);

        Vector3 finalPosition = GetSpawnPosition(position);
        enemy.transform.SetPositionAndRotation(finalPosition, rotation);

        if (enemy.TryGetComponent(out NavMeshAgent agent))
            agent.enabled = true;

        OnSpawned?.Invoke(enemy);
    }

    protected virtual Vector3 GetSpawnPosition(Vector3 requestedPosition)
    {
        return requestedPosition;
    }

    public virtual void ReturnToPool(Enemy enemy)
    {
        OnReleased?.Invoke(enemy);
        Pool.Release(enemy);
    }

    public abstract void ClearAll();

    public EnemySO GetEnemySO() => enemyPrefab.GetEnemySO();

    public float GetDifficultyCost() => enemyPrefab.GetEnemySO().DifficultyRating;

    public virtual float GetDifficultyCost(EnemyMutation mutation)
    {
        float baseCost = GetDifficultyCost();
        return baseCost * mutation.DifficultyRatingMultiplier;
    }
}
