using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ParticlePoolController : MonoBehaviour
{
    [Header("Tower Death Particles")]
    [SerializeField]
    private GameObject towerParticleSystemPrefab;

    [SerializeField]
    private Vector3 towerParticleOffset = new(0, 0.2f, 0);

    [Header("Enemy Death Particles")]
    [SerializeField]
    private GameObject enemyParticleSystemPrefab;

    [SerializeField]
    private Vector3 enemyParticleOffset = new(0, 0.2f, 0);

    [Header("Pool Settings")]
    [SerializeField]
    [Range(5, 50)]
    private int towerPoolDefaultCapacity = 5;

    [SerializeField]
    [Range(5, 50)]
    private int towerPoolMaxSize = 30;

    [SerializeField]
    [Range(5, 50)]
    private int enemyPoolDefaultCapacity = 10;

    [SerializeField]
    [Range(5, 50)]
    private int enemyPoolMaxSize = 50;

    private ObjectPool<ParticleSystem> towerParticlePool;
    private ObjectPool<ParticleSystem> enemyParticlePool;
    private readonly Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> particlePoolOwnership = new();

    private void Awake()
    {
        InitializePools();
        EventBus.Instance.Subscribe<OnTowerDeath>(OnTowerDeath);
        EventBus.Instance.Subscribe<OnEnemyDeath>(OnEnemyDeath);
    }

    private void InitializePools()
    {
        InitializeTowerPool();
        InitializeEnemyPool();
    }

    private void InitializeTowerPool()
    {
        towerParticlePool = new ObjectPool<ParticleSystem>(
            createFunc: () => CreatePooledParticleSystem(towerParticleSystemPrefab),
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: towerPoolDefaultCapacity,
            maxSize: towerPoolMaxSize
        );
    }

    private void InitializeEnemyPool()
    {
        enemyParticlePool = new ObjectPool<ParticleSystem>(
            createFunc: () => CreatePooledParticleSystem(enemyParticleSystemPrefab),
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: enemyPoolDefaultCapacity,
            maxSize: enemyPoolMaxSize
        );
    }

    private ParticleSystem CreatePooledParticleSystem(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, transform);

        if (!go.TryGetComponent(out ParticleSystem particleSystem))
            particleSystem = go.AddComponent<ParticleSystem>();

        var main = particleSystem.main;
        main.playOnAwake = false;
        main.loop = false;

        if (!go.TryGetComponent(out PooledParticleSystem pooledComponent))
            pooledComponent = go.AddComponent<PooledParticleSystem>();

        pooledComponent.Initialize(this);
        return particleSystem;
    }

    private void OnGetFromPool(ParticleSystem particleSystem)
    {
        particleSystem.gameObject.SetActive(true);
    }

    private void OnReturnToPool(ParticleSystem particleSystem)
    {
        particleSystem.Stop();
        particleSystem.Clear();
        particleSystem.gameObject.SetActive(false);
        particlePoolOwnership.Remove(particleSystem);
    }

    private void OnDestroyPoolObject(ParticleSystem particleSystem)
    {
        if (particleSystem != null && particleSystem.gameObject != null)
            Destroy(particleSystem.gameObject);
    }

    public void ReturnToPool(ParticleSystem particleSystem)
    {
        if (particlePoolOwnership.TryGetValue(particleSystem, out ObjectPool<ParticleSystem> pool))
        {
            pool.Release(particleSystem);
        }
        else
        {
            // Fallback: try both pools if ownership is missing
            Debug.LogWarning(
                "ParticlePoolController: Particle system ownership not found, attempting fallback release."
            );
            if (towerParticlePool != null)
                towerParticlePool.Release(particleSystem);
            else
                enemyParticlePool?.Release(particleSystem);
        }
    }

    private ParticleSystem PlayParticleAtPosition(ObjectPool<ParticleSystem> pool, Vector3 position, Vector3? rotation)
    {
        ParticleSystem particleSystem = pool.Get();
        particlePoolOwnership[particleSystem] = pool;

        particleSystem.transform.position = position;

        if (rotation.HasValue)
            particleSystem.transform.rotation = Quaternion.Euler(rotation.Value);
        else
            particleSystem.transform.rotation = Quaternion.identity;

        particleSystem.Play();
        return particleSystem;
    }

    public ParticleSystem PlayTowerParticleAtPosition(Vector3 position, Vector3? rotation = null)
    {
        if (towerParticleSystemPrefab == null)
        {
            Debug.LogWarning("ParticlePoolController: Tower particle system prefab is not assigned!");
            return null;
        }

        return PlayParticleAtPosition(towerParticlePool, position + towerParticleOffset, rotation);
    }

    public ParticleSystem PlayEnemyParticleAtPosition(Vector3 position, Vector3? rotation = null)
    {
        if (enemyParticleSystemPrefab == null)
        {
            Debug.LogWarning("ParticlePoolController: Enemy particle system prefab is not assigned!");
            return null;
        }

        return PlayParticleAtPosition(enemyParticlePool, position + enemyParticleOffset, rotation);
    }

    private void OnTowerDeath(OnTowerDeath evt)
    {
        PlayTowerParticleAtPosition(evt.Position);
    }

    private void OnEnemyDeath(OnEnemyDeath evt)
    {
        PlayEnemyParticleAtPosition(evt.Position);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<OnTowerDeath>(OnTowerDeath);
        EventBus.Instance.Unsubscribe<OnEnemyDeath>(OnEnemyDeath);
    }
}
