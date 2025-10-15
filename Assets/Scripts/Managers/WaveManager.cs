using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : Singleton<WaveManager>
{
    [Header("Components")]
    [SerializeField]
    private Spawner ufoMeleeASpawner;

    [SerializeField]
    private Spawner ufoMeleeBSpawner;

    [SerializeField]
    private Spawner ufoRangeASpawner;

    [Header("Wave Settings")]
    [SerializeField]
    [Range(1, 10)]
    private float timeBetweenWaves = 5f;

    [Header("Budget")]
    [SerializeField]
    private float minBudget = 10;
    private readonly List<Transform> spawnPoints = new();
    private Coroutine spawnCoroutine;
    private int currentWaveIndex = -1;
    private float waveBudget;

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

    private IEnumerator SpawnRoutine()
    {
        currentWaveIndex++;
        yield break;
    }

    private void OnGameEnd()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    public void RegisterSpawnPoint(Transform transform) => spawnPoints.Add(transform);

    public void UnregisterSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    private void OnDestroy()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameEnd -= OnGameEnd;
    }
}
