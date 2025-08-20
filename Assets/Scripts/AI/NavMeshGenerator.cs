using Unity.AI.Navigation;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshGenerator : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    private void OnEnable()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        MapGenerator.OnMapGenerated += OnMapGenerated;
    }

    private void OnMapGenerated()
    {
        Debug.Log("Building Nav Mesh...");
        navMeshSurface.BuildNavMesh();
    }

    private void OnDisable()
    {
        MapGenerator.OnMapGenerated -= OnMapGenerated;
    }
}
