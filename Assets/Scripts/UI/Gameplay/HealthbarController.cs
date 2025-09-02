using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class HealthbarController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private Transform parent;

    [SerializeField]
    private Healthbar healthbarPrefab;

    [Header("Configs")]
    [SerializeField]
    [Range(1f, 10f)]
    private float initialDisplayDuration = 3f;

    private ObjectPool<Healthbar> healthbarPool;
    private readonly Dictionary<IDamagable, Healthbar> activeHealthbars = new();
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        healthbarPool = new ObjectPool<Healthbar>(
            createFunc: () => Instantiate(healthbarPrefab, parent, false),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject)
        );

        EventBus.Instance.Subscribe<OnEnemyHealthChanged>(OnEnemyHealthChanged);
        EventBus.Instance.Subscribe<OnTowerHealthChanged>(OnTowerHealthChanged);
    }

    private void OnEnemyHealthChanged(OnEnemyHealthChanged evt) =>
        HandleHealthChanged(evt.EnemyHealth, evt.EnemyHealth.transform);

    private void OnTowerHealthChanged(OnTowerHealthChanged evt) =>
        HandleHealthChanged(evt.TowerHealth, evt.TowerHealth.transform);

    private void HandleHealthChanged(IDamagable damagable, Transform target)
    {
        if (damagable == null)
            return;

        if (activeHealthbars.TryGetValue(damagable, out Healthbar existingHealthbar))
        {
            existingHealthbar.ResetTimer();
        }
        else
        {
            var healthbar = healthbarPool.Get();
            healthbar.Init(this, target, damagable, initialDisplayDuration, mainCamera, canvas);
            activeHealthbars[damagable] = healthbar;
        }
    }

    public void ReleaseHealthbar(Healthbar healthbar, IDamagable damagable)
    {
        if (activeHealthbars.ContainsKey(damagable))
        {
            activeHealthbars.Remove(damagable);
            healthbarPool.Release(healthbar);
        }
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<OnEnemyHealthChanged>(OnEnemyHealthChanged);
        EventBus.Instance.Unsubscribe<OnTowerHealthChanged>(OnTowerHealthChanged);
    }
}
