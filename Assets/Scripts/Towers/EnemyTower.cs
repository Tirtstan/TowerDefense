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
        EnemySpawnController.Instance.RegisterSpawnPoint(spawnPoint);
    }

    private void OnDestroy()
    {
        if (EnemySpawnController.Instance != null)
            EnemySpawnController.Instance.UnregisterSpawnPoint(spawnPoint);
    }
}
