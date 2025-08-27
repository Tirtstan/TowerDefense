using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField]
    private float spawnInterval = 5f;

    [SerializeField]
    private GameObject enemyPrefab;
    private WaitForSeconds waitInterval;

    private void Awake()
    {
        waitInterval = new WaitForSeconds(spawnInterval);
    }

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            yield return waitInterval;
        }
    }
}
