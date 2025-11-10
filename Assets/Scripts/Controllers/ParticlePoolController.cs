using UnityEngine;
using UnityEngine.Pool;

public class ParticlePoolController : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField]
    private GameObject particleSystemPrefab;

    [SerializeField]
    [Range(5, 50)]
    private int poolDefaultCapacity = 5;

    [SerializeField]
    [Range(5, 50)]
    private int poolMaxSize = 30;
    private ObjectPool<ParticleSystem> particlePool;

    private void Awake()
    {
        InitializePool();
        EventBus.Instance.Subscribe<OnTowerDeath>(OnTowerDeath);
    }

    private void InitializePool()
    {
        particlePool = new ObjectPool<ParticleSystem>(
            createFunc: CreatePooledParticleSystem,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: poolDefaultCapacity,
            maxSize: poolMaxSize
        );
    }

    private ParticleSystem CreatePooledParticleSystem()
    {
        GameObject go = Instantiate(particleSystemPrefab, transform);

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
    }

    private void OnDestroyPoolObject(ParticleSystem particleSystem)
    {
        if (particleSystem != null && particleSystem.gameObject != null)
            Destroy(particleSystem.gameObject);
    }

    public void ReturnToPool(ParticleSystem particleSystem)
    {
        particlePool.Release(particleSystem);
    }

    public ParticleSystem PlayParticleAtPosition(Vector3 position, Vector3? rotation = null)
    {
        if (particleSystemPrefab == null)
        {
            Debug.LogWarning("ParticlePoolController: Particle system prefab is not assigned!");
            return null;
        }

        ParticleSystem particleSystem = particlePool.Get();
        particleSystem.transform.position = position;

        if (rotation.HasValue)
            particleSystem.transform.rotation = Quaternion.Euler(rotation.Value);
        else
            particleSystem.transform.rotation = Quaternion.identity;

        particleSystem.Play();
        return particleSystem;
    }

    private void OnTowerDeath(OnTowerDeath evt)
    {
        PlayParticleAtPosition(evt.Position);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<OnTowerDeath>(OnTowerDeath);
    }
}
