using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyHealthbarController : MonoBehaviour
{
    [Header("Components")]
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
    private readonly List<IDamagable> toRemove = new();

    private void Awake()
    {
        healthbarPool = new ObjectPool<Healthbar>(
            createFunc: () => Instantiate(healthbarPrefab, parent, false),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject)
        );

        EnemyHealth.OnHealthChangedStatic += OnEnemyHealthChanged;
    }

    private void Update()
    {
        HeartbeatHealthbars();
    }

    private void OnEnemyHealthChanged(EnemyHealth health)
    {
        IDamagable damagable = health;
        if (damagable == null)
            return;

        if (activeHealthbars.TryGetValue(damagable, out Healthbar existingHealthbar))
        {
            existingHealthbar.ResetTimer();
        }
        else
        {
            var healthbar = healthbarPool.Get();
            healthbar.Init(this, health.transform, damagable, initialDisplayDuration);
            activeHealthbars[damagable] = healthbar;
        }
    }

    private void HeartbeatHealthbars()
    {
        toRemove.Clear();
        foreach (var kvp in activeHealthbars)
        {
            var damagable = kvp.Key;
            var healthbar = kvp.Value;

            if (healthbar.TryUpdateTimer(Time.deltaTime) || damagable.CurrentHealth <= 0f)
                toRemove.Add(damagable);
        }

        foreach (var damagable in toRemove)
            RemoveHealthbar(damagable);
    }

    private void RemoveHealthbar(IDamagable damagable)
    {
        if (activeHealthbars.TryGetValue(damagable, out var healthbar))
        {
            healthbarPool.Release(healthbar);
            activeHealthbars.Remove(damagable);
        }
    }

    public void ReleaseHealthbar(Healthbar healthbar)
    {
        IDamagable toRemove = null;
        foreach (var healthbarPair in activeHealthbars)
        {
            if (healthbarPair.Value == healthbar)
            {
                toRemove = healthbarPair.Key;
                break;
            }
        }

        if (toRemove != null)
            activeHealthbars.Remove(toRemove);

        healthbarPool.Release(healthbar);
    }

    private void OnDestroy()
    {
        EnemyHealth.OnHealthChangedStatic -= OnEnemyHealthChanged;
    }
}
