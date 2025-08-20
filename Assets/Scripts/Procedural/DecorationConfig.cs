using UnityEngine;

[System.Serializable]
public class DecorationConfig
{
    [Header("Basic Settings")]
    public GameObject[] prefabs;
    public float weight = 1f;

    [Header("Noise Thresholds")]
    [Range(0f, 1f)]
    public float minNoiseValue = 0f;

    [Range(0f, 1f)]
    public float maxNoiseValue = 1f;

    [Header("Size & Scale")]
    public Vector2 scaleRange = new(0.8f, 1.2f);
    public float minSize = 0.5f;

    [Header("Positioning")]
    [Range(0f, 0.5f)]
    public float positionVariance = 0.3f;
    public bool randomizeRotation = true;

    [Header("Path Avoidance")]
    public float pathAvoidanceRadius = 1.5f;
    public bool avoidPaths = true;

    [Header("Spawning Parent")]
    [Tooltip("Optional parent transform for this decoration type. If null, uses the DecorationSpawner's transform")]
    public Transform customParent;

    [Tooltip("Create a new child GameObject as parent for this decoration type")]
    public bool createChildParent = false;

    [Tooltip("Name for the child parent GameObject (only used if createChildParent is true)")]
    public string childParentName = "Decoration Group";
}
