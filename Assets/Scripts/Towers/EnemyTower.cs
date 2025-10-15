using UnityEngine;

public class EnemyTower : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void Start()
    {
        WaveManager.Instance.RegisterSpawnPoint(spawnPoint);
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.UnregisterSpawnPoint(spawnPoint);
    }
}
