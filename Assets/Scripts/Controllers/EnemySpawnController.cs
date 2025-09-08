using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : Singleton<EnemySpawnController>
{
    [Header("Components")]
    [SerializeField]
    private GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField]
    private float spawnInterval = 10f;

    [SerializeField]
    private Vector2Int spawnCountRange = new(1, 3);
    private readonly List<Transform> spawnPoints = new();
    private Coroutine spawnCoroutine;

    protected override void Awake()
    {
        base.Awake();
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameEnd += OnGameEnd;
    }

    private void OnGameStart()
    {
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    private void OnGameEnd()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            StartCoroutine(SpawnEnemies());
            yield return new WaitForSeconds(GetInterval());
        }
    }

    public IEnumerator SpawnEnemies()
    {
        if (spawnPoints.Count == 0)
            yield break;

        int count = Random.Range(spawnCountRange.x, spawnCountRange.y + 1);
        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            yield return null;
        }
    }

    private float GetInterval() // TODO: add more in depth balancing logic
    {
        return spawnInterval;
    }

    public void RegisterSpawnPoint(Transform transform) => spawnPoints.Add(transform);

    public void UnregisterSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    private void OnDestroy()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameEnd -= OnGameEnd;
    }
}
