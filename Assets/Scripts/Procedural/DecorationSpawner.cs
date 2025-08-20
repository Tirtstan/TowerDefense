using System.Collections.Generic;
using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private MapGenerator mapGenerator;

    [Header("General Settings")]
    [SerializeField, Range(0f, 1f)]
    private float globalDensity = 0.1f;

    [SerializeField, Range(0.1f, 2f)]
    private float heightOffset = 0.25f;

    [Header("Noise Settings")]
    [SerializeField, Range(0.1f, 20f)]
    private float noiseScale = 5f;

    [SerializeField]
    private Vector2 noiseOffset;

    [SerializeField]
    private bool randomizeNoiseOffset = true;

    [Header("Scale Noise Settings")]
    [SerializeField, Range(0.1f, 20f)]
    private float scaleNoiseScale = 3f;

    [SerializeField]
    private Vector2 scaleNoiseOffset;

    [SerializeField]
    private bool randomizeScaleNoiseOffset = true;

    [SerializeField, Tooltip("How much the noise affects the scale (0 = pure noise-based, 1 = some randomness)")]
    [Range(0f, 1f)]
    private float scaleRandomness = 0.2f;

    [Header("Default Spawning Parent")]
    [SerializeField, Tooltip("Default parent for all decorations. If null, uses this transform")]
    private Transform defaultParent;

    [SerializeField, Tooltip("Create separate child GameObjects for each decoration type")]
    private bool createTypeGroups = false;

    [Header("Decoration Types")]
    [SerializeField]
    private DecorationConfig[] decorationConfigs;
    private readonly Dictionary<DecorationConfig, Transform> configParents = new();

    private void Awake()
    {
        if (mapGenerator == null)
            Debug.LogError("DecorationSpawner requires MapGenerator component on the same GameObject!");
    }

    public void SpawnDecorations()
    {
        if (decorationConfigs == null || decorationConfigs.Length == 0 || mapGenerator == null)
            return;

        if (randomizeNoiseOffset)
            noiseOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));

        if (randomizeScaleNoiseOffset)
            scaleNoiseOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));

        ClearExistingDecorations();
        SetupParentTransforms();

        int gridSize = mapGenerator.GetGridSize();
        float tileSize = mapGenerator.GetTileSize();

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                if (mapGenerator.GetTileType(x, z) == TileType.Ground && Random.value < globalDensity)
                    TrySpawnDecoration(x, z, tileSize);
            }
        }
    }

    private void SetupParentTransforms()
    {
        configParents.Clear();

        foreach (var config in decorationConfigs)
        {
            // Priority order: customParent > createChildParent > defaultParent > this.transform
            Transform parentToUse;
            if (config.customParent != null)
            {
                parentToUse = config.customParent;
            }
            else if (config.createChildParent || createTypeGroups)
            {
                string parentName = config.createChildParent
                    ? config.childParentName
                    : $"{config.prefabs[0].name} Group";
                var childParent = new GameObject(parentName);
                childParent.transform.SetParent(defaultParent != null ? defaultParent : transform);
                parentToUse = childParent.transform;
            }
            else if (defaultParent != null)
            {
                parentToUse = defaultParent;
            }
            else
            {
                parentToUse = transform;
            }

            configParents[config] = parentToUse;
        }
    }

    private void TrySpawnDecoration(int gridX, int gridZ, float tileSize)
    {
        float noiseX = (gridX + noiseOffset.x) / noiseScale;
        float noiseZ = (gridZ + noiseOffset.y) / noiseScale;
        float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

        DecorationConfig selectedConfig = SelectDecorationByNoise(noiseValue);
        if (selectedConfig == null || selectedConfig.prefabs.Length == 0)
            return;

        // Calculate noise-based scale
        float scaleNoiseX = (gridX + scaleNoiseOffset.x) / scaleNoiseScale;
        float scaleNoiseZ = (gridZ + scaleNoiseOffset.y) / scaleNoiseScale;
        float scaleNoiseValue = Mathf.PerlinNoise(scaleNoiseX, scaleNoiseZ);

        // Map noise value (0-1) to scale range
        float noiseBasedScale = Mathf.Lerp(selectedConfig.scaleRange.x, selectedConfig.scaleRange.y, scaleNoiseValue);

        // Optionally add some randomness
        float randomComponent = Random.Range(selectedConfig.scaleRange.x, selectedConfig.scaleRange.y);
        float finalScale = Mathf.Lerp(noiseBasedScale, randomComponent, scaleRandomness);

        Vector3 position =
            new(
                (gridX + Random.Range(-selectedConfig.positionVariance, selectedConfig.positionVariance)) * tileSize,
                heightOffset,
                (gridZ + Random.Range(-selectedConfig.positionVariance, selectedConfig.positionVariance)) * tileSize
            );

        float effectiveAvoidanceRadius = selectedConfig.pathAvoidanceRadius * finalScale;

        if (selectedConfig.avoidPaths && IsTooCloseToPath(position, effectiveAvoidanceRadius, tileSize))
            return;

        if (selectedConfig.avoidPaths && IsTooCloseToTower(position, effectiveAvoidanceRadius, tileSize))
            return;

        GameObject prefab = selectedConfig.prefabs[Random.Range(0, selectedConfig.prefabs.Length)];

        Quaternion rotation = selectedConfig.randomizeRotation
            ? Quaternion.Euler(0, Random.Range(0, 360), 0)
            : Quaternion.identity;

        Transform parentTransform = configParents.GetValueOrDefault(selectedConfig, transform);
        GameObject decoration = Instantiate(prefab, position, rotation, parentTransform);

        decoration.transform.localScale = Vector3.one * finalScale;
    }

    private DecorationConfig SelectDecorationByNoise(float noiseValue)
    {
        List<DecorationConfig> validConfigs = new();
        List<float> weights = new();

        foreach (var config in decorationConfigs)
        {
            if (noiseValue >= config.minNoiseValue && noiseValue <= config.maxNoiseValue)
            {
                validConfigs.Add(config);
                weights.Add(config.weight);
            }
        }

        if (validConfigs.Count == 0)
            return null;

        // Weighted random selection
        float totalWeight = 0f;
        foreach (float weight in weights)
            totalWeight += weight;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < validConfigs.Count; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
                return validConfigs[i];
        }

        return validConfigs[^1];
    }

    private bool IsTooCloseToPath(Vector3 worldPosition, float avoidanceRadius, float tileSize)
    {
        int gridSize = mapGenerator.GetGridSize();

        // Convert world position back to grid coordinates for checking
        int centerX = Mathf.RoundToInt(worldPosition.x / tileSize);
        int centerZ = Mathf.RoundToInt(worldPosition.z / tileSize);

        // Check in a radius around the position
        int checkRadius = Mathf.CeilToInt(avoidanceRadius);
        for (int x = centerX - checkRadius; x <= centerX + checkRadius; x++)
        {
            for (int z = centerZ - checkRadius; z <= centerZ + checkRadius; z++)
            {
                if (x >= 0 && x < gridSize && z >= 0 && z < gridSize)
                {
                    TileType tileType = mapGenerator.GetTileType(x, z);

                    if (IsPathTile(tileType))
                    {
                        Vector3 tileWorldPos = new(x * tileSize, 0, z * tileSize);
                        float distance = Vector3.Distance(worldPosition, tileWorldPos);

                        if (distance < avoidanceRadius)
                            return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsTooCloseToTower(Vector3 worldPosition, float avoidanceRadius, float tileSize)
    {
        int gridSize = mapGenerator.GetGridSize();

        // Convert world position back to grid coordinates for checking
        int centerX = Mathf.RoundToInt(worldPosition.x / tileSize);
        int centerZ = Mathf.RoundToInt(worldPosition.z / tileSize);

        // Check in a radius around the position
        int checkRadius = Mathf.CeilToInt(avoidanceRadius);

        for (int x = centerX - checkRadius; x <= centerX + checkRadius; x++)
        {
            for (int z = centerZ - checkRadius; z <= centerZ + checkRadius; z++)
            {
                if (x >= 0 && x < gridSize && z >= 0 && z < gridSize)
                {
                    TileType tileType = mapGenerator.GetTileType(x, z);

                    if (IsTowerTile(tileType))
                    {
                        Vector3 tileWorldPos = new Vector3(x * tileSize, 0, z * tileSize);
                        float distance = Vector3.Distance(worldPosition, tileWorldPos);

                        if (distance < avoidanceRadius)
                            return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsPathTile(TileType tileType)
    {
        return tileType == TileType.Path
            || tileType == TileType.Turn
            || tileType == TileType.TJunction
            || tileType == TileType.CrossJunction
            || tileType == TileType.CenterTile;
    }

    private bool IsTowerTile(TileType tileType) => tileType == TileType.CenterTile;

    private void ClearExistingDecorations()
    {
        Transform clearTarget = defaultParent != null ? defaultParent : transform;
        for (int i = clearTarget.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(clearTarget.GetChild(i).gameObject);
#else
            Destroy(clearTarget.GetChild(i).gameObject);
#endif
        }

        // Also clear any custom parents that were created
        foreach (var config in decorationConfigs)
        {
            if (config.customParent != null && config.customParent != clearTarget)
            {
                for (int i = config.customParent.childCount - 1; i >= 0; i--)
                {
#if UNITY_EDITOR
                    DestroyImmediate(config.customParent.GetChild(i).gameObject);
#else
                    Destroy(config.customParent.GetChild(i).gameObject);
#endif
                }
            }
        }

        configParents.Clear();
    }

    public Transform GetParentForConfig(int configIndex)
    {
        if (configIndex >= 0 && configIndex < decorationConfigs.Length)
        {
            DecorationConfig config = decorationConfigs[configIndex];
            return configParents.GetValueOrDefault(config, transform);
        }

        return transform;
    }

    public Dictionary<string, Transform> GetAllDecorationParents()
    {
        var result = new Dictionary<string, Transform>();
        for (int i = 0; i < decorationConfigs.Length; i++)
        {
            DecorationConfig config = decorationConfigs[i];
            string key = config.createChildParent ? config.childParentName : $"Config_{i}";
            result[key] = configParents.GetValueOrDefault(config, transform);
        }

        return result;
    }
}
