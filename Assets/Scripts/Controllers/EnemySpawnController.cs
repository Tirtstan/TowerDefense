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
    private float spawnInterval = 20f;
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
        StopCoroutine(spawnCoroutine);
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(GetInterval());
        }
    }

    public void SpawnEnemy()
    {
        if (spawnPoints.Count == 0)
            return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        Debug.Log($"Spawned {enemyPrefab.name} at {spawnPoint.position}");
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
