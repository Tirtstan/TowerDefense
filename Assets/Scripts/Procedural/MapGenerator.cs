using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// Anthropic, 2025
public class MapGenerator : MonoBehaviour
{
    public static event Action OnMapGenerated;

    [Header("Map Settings")]
    [SerializeField]
    private Transform mapParent;

    [SerializeField, Tooltip("Size of the grid (width and height in tiles)")]
    private int gridSize = 27;

    [SerializeField, Tooltip("Number of enemy towers to place around the map edges")]
    private int numEndPoints = 3;

    [SerializeField, Tooltip("Size of each tile in Unity units")]
    private float tileSize = 1f;

    [SerializeField]
    [Range(2, 10)]
    [Tooltip("Minimum number of paths that must connect directly to the center (prevents single bottleneck)")]
    private int minDirectPaths = 2;

    [Header("Tower Spacing")]
    [SerializeField, Range(3, 15), Tooltip("Minimum distance between towers to ensure good spacing")]
    private int minTowerDistance = 5;

    [SerializeField, Tooltip("Use the configured min distance instead of adaptive distance based on grid size")]
    private bool useGridBasedDistance = true;

    [SerializeField, Range(0.1f, 2f), Tooltip("How high above the tiles towers should be positioned")]
    private float towerHeightOffset = 0.25f;

    [Header("Pathfinding Settings")]
    [SerializeField, Range(1f, 20f), Tooltip("Penalty for paths going near existing paths (higher = more spread out)")]
    private float pathAvoidancePenalty = 5f;

    [SerializeField, Range(10f, 200f), Tooltip("High penalty to prevent merging paths at direct connection points")]
    private float directPathMergePenalty = 100f;

    [SerializeField]
    [Range(1f, 15f)]
    [Tooltip("Penalty for direct paths avoiding existing paths (higher = more isolated direct paths)")]
    private float directPathAvoidancePenalty = 10f;

    [SerializeField]
    [Range(0f, 0.5f)]
    [Tooltip("Small penalty for staying near edges (higher = slight center preference)")]
    private float edgePenalty = 0.2f;

    [SerializeField, Range(0f, 0.3f), Tooltip("Small penalty for very long straight lines (minimal turning)")]
    private float straightLinePenalty = 0.1f;

    [SerializeField, Range(2, 8), Tooltip("Distance from center where fallback connection is allowed")]
    private int fallbackConnectionRadius = 4;

    [Header("Tile Prefabs")]
    [SerializeField, Tooltip("Basic ground tile prefab")]
    private GameObject groundTilePrefab;

    [SerializeField, Tooltip("Straight path tile prefab")]
    private GameObject pathStraightPrefab;

    [SerializeField, Tooltip("Corner/turn path tile prefab")]
    private GameObject pathTurnPrefab;

    [SerializeField, Tooltip("T-junction path tile prefab (3 connections)")]
    private GameObject pathTJunctionPrefab;

    [SerializeField, Tooltip("Cross junction path tile prefab (4 connections)")]
    private GameObject pathCrossJunctionPrefab;

    [SerializeField, Tooltip("Special tile for the center where player base will be placed")]
    private GameObject centerTowerTilePrefab;

    [Header("Tower Prefabs")]
    [SerializeField, Tooltip("Player base prefab (spawned at center)")]
    private GameObject playerBasePrefab;

    [SerializeField, Tooltip("Enemy tower prefab (spawned at map edges)")]
    private GameObject enemyTowerPrefab;

    [Header("Generation Options")]
    [SerializeField, Tooltip("Allow towers to spawn in the corner positions of the map")]
    private bool allowCornerTowers = true;

    [SerializeField, Tooltip("Prioritize placing towers at the center of each edge before other positions")]
    private bool preferEdgeCenters = false;

    [SerializeField, Tooltip("Number of attempts to retry pathfinding if initial attempt fails")]
    private int maxPathfindingAttempts = 10;

    private TileType[,] grid;
    private Vector2Int startPoint;
    private readonly List<Vector2Int> towerPositions = new();
    private readonly List<List<Vector2Int>> paths = new();
    private readonly HashSet<Vector2Int> occupiedTiles = new();
    private readonly HashSet<Vector2Int> towerTiles = new();
    private readonly Dictionary<Vector2Int, int> pathConnectionCount = new();
    private readonly Dictionary<Vector2Int, List<Vector2Int>> junctionConnections = new();
    private readonly HashSet<Vector2Int> directPathEndpoints = new();

    private void Awake()
    {
        if (mapParent == null)
            mapParent = transform;
    }

    public int GetGridSize() => gridSize;

    public float GetTileSize() => tileSize;

    public TileType GetTileType(int x, int z) => grid[x, z];

    public void GenerateMap()
    {
        ClearExistingMap();
        InitializeGrid();
        PlaceStartPoint();
        PlaceTowerPositions();
        CreatePaths();
        IdentifyJunctions();
        InstantiateTiles();
        SpawnTowers();

        if (TryGetComponent(out DecorationSpawner decorationSpawner))
            decorationSpawner.SpawnDecorations();

        OnMapGenerated?.Invoke();
    }

    private void InitializeGrid()
    {
        grid = new TileType[gridSize, gridSize];
        occupiedTiles.Clear();
        towerTiles.Clear();
        towerPositions.Clear();
        paths.Clear();
        pathConnectionCount.Clear();
        junctionConnections.Clear();
        directPathEndpoints.Clear();

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
                grid[x, z] = TileType.Ground;
        }
    }

    private void PlaceStartPoint()
    {
        startPoint = new Vector2Int(gridSize / 2, gridSize / 2);
        grid[startPoint.x, startPoint.y] = TileType.CenterTile;
        occupiedTiles.Add(startPoint);
    }

    private void PlaceTowerPositions()
    {
        List<Vector2Int> edgePositions = GetEdgePositions();
        edgePositions = edgePositions.OrderBy(p => Random.value).ToList();

        int actualMinDistance = useGridBasedDistance ? minTowerDistance : Mathf.Max(3, gridSize / 8);

        for (int i = 0; i < numEndPoints && edgePositions.Count > 0; i++)
        {
            Vector2Int bestPosition = Vector2Int.zero;
            bool foundValidPosition = false;

            // try to find a position that's far enough from existing towers
            for (int j = 0; j < edgePositions.Count; j++)
            {
                Vector2Int candidate = edgePositions[j];

                if (IsValidTowerPosition(candidate, actualMinDistance))
                {
                    bestPosition = candidate;
                    foundValidPosition = true;
                    edgePositions.RemoveAt(j);
                    break;
                }
            }

            if (foundValidPosition)
            {
                towerPositions.Add(bestPosition);
                towerTiles.Add(bestPosition);
            }
            else if (towerPositions.Count == 0)
            {
                // force place at least one tower if none found
                bestPosition = edgePositions[0];
                towerPositions.Add(bestPosition);
                towerTiles.Add(bestPosition);
                edgePositions.RemoveAt(0);
            }
            else
            {
                Debug.LogWarning($"Could not place tower {i + 1} with minimum distance {actualMinDistance}");
            }
        }
    }

    private List<Vector2Int> GetEdgePositions()
    {
        List<Vector2Int> edgePositions = new();

        if (preferEdgeCenters)
        {
            // add edge center positions first
            edgePositions.Add(new Vector2Int(gridSize / 2, 0)); // Bottom center
            edgePositions.Add(new Vector2Int(gridSize / 2, gridSize - 1)); // Top center
            edgePositions.Add(new Vector2Int(0, gridSize / 2)); // Left center
            edgePositions.Add(new Vector2Int(gridSize - 1, gridSize / 2)); // Right center
        }

        // add all edge positions
        for (int x = 0; x < gridSize; x++)
        {
            if (!allowCornerTowers && (x == 0 || x == gridSize - 1))
                continue;

            edgePositions.Add(new Vector2Int(x, 0)); // Bottom edge
            edgePositions.Add(new Vector2Int(x, gridSize - 1)); // Top edge
        }

        for (int y = 1; y < gridSize - 1; y++)
        {
            if (!allowCornerTowers && (y == 0 || y == gridSize - 1))
                continue;

            edgePositions.Add(new Vector2Int(0, y)); // Left edge
            edgePositions.Add(new Vector2Int(gridSize - 1, y)); // Right edge
        }

        return edgePositions.Distinct().ToList();
    }

    private bool IsValidTowerPosition(Vector2Int candidate, int minDistance)
    {
        foreach (var existingTower in towerPositions)
        {
            if (Vector2Int.Distance(candidate, existingTower) < minDistance)
                return false;
        }

        // Additional check: don't place too close to center
        if (Vector2Int.Distance(candidate, startPoint) < minDistance / 2)
            return false;

        return true;
    }

    private void CreatePaths()
    {
        int actualMinDirectPaths = Mathf.Min(minDirectPaths, towerPositions.Count);
        List<Vector2Int> directTowers = towerPositions.Take(actualMinDirectPaths).ToList();
        List<Vector2Int> remainingTowers = towerPositions.Skip(actualMinDirectPaths).ToList();

        // Create direct paths first
        foreach (var towerPos in directTowers)
        {
            List<Vector2Int> path = FindPathWithRetries(towerPos, startPoint, true);
            if (path != null && path.Count > 0)
            {
                paths.Add(path);
                ApplyPathToGrid(path);
                TrackPathConnections(path);

                if (path.Count >= 2)
                    directPathEndpoints.Add(path[^2]);

                for (int i = 1; i < path.Count - 1; i++)
                    occupiedTiles.Add(path[i]);
            }
            else
            {
                Debug.LogWarning($"Could not find direct path from tower at {towerPos} to center at {startPoint}");
            }
        }

        // Create remaining paths
        foreach (var towerPos in remainingTowers)
        {
            List<Vector2Int> path = FindPathWithRetries(towerPos, startPoint, false);
            if (path != null && path.Count > 0)
            {
                paths.Add(path);
                ApplyPathToGrid(path);
                TrackPathConnections(path);

                for (int i = 1; i < path.Count - 1; i++)
                    occupiedTiles.Add(path[i]);
            }
            else
            {
                Debug.LogWarning($"Could not find path from tower at {towerPos} to center at {startPoint}");
            }
        }
    }

    private List<Vector2Int> FindPathWithRetries(Vector2Int start, Vector2Int end, bool forceDirect)
    {
        for (int attempt = 0; attempt < maxPathfindingAttempts; attempt++)
        {
            List<Vector2Int> path = FindPath(start, end, forceDirect, attempt);
            if (path != null)
                return path;
        }

        return null;
    }

    private void TrackPathConnections(List<Vector2Int> path)
    {
        foreach (var position in path)
        {
            if (pathConnectionCount.ContainsKey(position))
            {
                pathConnectionCount[position]++;
            }
            else
            {
                pathConnectionCount[position] = 1;
            }
        }

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int current = path[i];

            if (!junctionConnections.ContainsKey(current))
                junctionConnections[current] = new List<Vector2Int>();

            if (i > 0)
            {
                Vector2Int direction = path[i - 1] - current;
                if (!junctionConnections[current].Contains(direction))
                    junctionConnections[current].Add(direction);
            }

            if (i < path.Count - 1)
            {
                Vector2Int direction = path[i + 1] - current;
                if (!junctionConnections[current].Contains(direction))
                    junctionConnections[current].Add(direction);
            }
        }
    }

    private void IdentifyJunctions()
    {
        foreach (var kvp in pathConnectionCount)
        {
            Vector2Int position = kvp.Key;
            int connectionCount = kvp.Value;

            if (connectionCount > 1 && position != startPoint)
            {
                int uniqueDirections = junctionConnections.ContainsKey(position)
                    ? junctionConnections[position].Count
                    : 0;

                if (uniqueDirections >= 4)
                {
                    grid[position.x, position.y] = TileType.CrossJunction;
                }
                else if (uniqueDirections == 3)
                {
                    grid[position.x, position.y] = TileType.TJunction;
                }
            }
        }
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, bool forceDirect = false, int attempt = 0)
    {
        var openSet = new List<Vector2Int>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = ManhattanDistance(start, end);

        while (openSet.Count > 0)
        {
            Vector2Int current = FindNodeWithLowestFScore(openSet, fScore);

            if (current == end)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor) || !IsValidTileForPath(neighbor, end, forceDirect))
                    continue;

                float moveCost = 1f + (attempt * 0.1f);

                // Very mild edge penalty - only for tiles right at the edge
                if (neighbor.x == 0 || neighbor.y == 0 || neighbor.x == gridSize - 1 || neighbor.y == gridSize - 1)
                {
                    moveCost += edgePenalty;
                }

                // Only apply straight line penalty after many consecutive straight moves
                int straightCount = 0;
                Vector2Int tempCurrent = current;
                Vector2Int direction = neighbor - current;

                while (cameFrom.ContainsKey(tempCurrent))
                {
                    Vector2Int prevDirection = tempCurrent - cameFrom[tempCurrent];
                    if (prevDirection == direction)
                    {
                        straightCount++;
                        tempCurrent = cameFrom[tempCurrent];
                    }
                    else
                    {
                        break;
                    }
                }

                // Only penalize after 6+ straight moves
                if (straightCount >= 6)
                {
                    moveCost += straightLinePenalty;
                }

                if (forceDirect)
                {
                    if (IsNearExistingPath(neighbor) && !IsAdjacentToCenter(neighbor))
                        moveCost += directPathAvoidancePenalty;
                }
                else
                {
                    if (directPathEndpoints.Contains(neighbor))
                    {
                        moveCost += directPathMergePenalty;
                    }
                    else if (IsNearExistingPath(neighbor) && !IsAdjacentToCenter(neighbor))
                    {
                        moveCost += pathAvoidancePenalty;
                    }
                }

                float tentativeGScore = gScore[current] + moveCost;

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);
                else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = tentativeGScore + ManhattanDistance(neighbor, end);
            }
        }

        // If normal pathfinding failed, try fallback connection to existing path
        if (!forceDirect)
        {
            return FindFallbackPath(start, end);
        }

        return null;
    }

    private List<Vector2Int> FindFallbackPath(Vector2Int start, Vector2Int end)
    {
        // Find the closest existing path tile within fallback radius of center
        Vector2Int bestConnectionPoint = Vector2Int.zero;
        float bestDistance = float.MaxValue;
        List<Vector2Int> bestPathToConnection = null;

        foreach (var pathTile in occupiedTiles)
        {
            // Only consider path tiles close to center
            if (Vector2Int.Distance(pathTile, end) <= fallbackConnectionRadius)
            {
                // Try to find a path from start to this existing path tile
                var pathToExisting = FindSimplePath(start, pathTile);
                if (pathToExisting != null)
                {
                    float totalDistance = pathToExisting.Count + Vector2Int.Distance(pathTile, end);
                    if (totalDistance < bestDistance)
                    {
                        bestDistance = totalDistance;
                        bestConnectionPoint = pathTile;
                        bestPathToConnection = pathToExisting;
                    }
                }
            }
        }

        return bestPathToConnection;
    }

    private List<Vector2Int> FindSimplePath(Vector2Int start, Vector2Int end)
    {
        var openSet = new List<Vector2Int>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = ManhattanDistance(start, end);

        while (openSet.Count > 0)
        {
            Vector2Int current = FindNodeWithLowestFScore(openSet, fScore);

            if (current == end)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                // More lenient validation for fallback paths
                if (!IsInBounds(neighbor) || towerTiles.Contains(neighbor))
                    continue;

                // Allow connecting to existing paths
                if (neighbor == end || grid[neighbor.x, neighbor.y] == TileType.Ground)
                {
                    float tentativeGScore = gScore[current] + 1f;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + ManhattanDistance(neighbor, end);
                }
            }
        }

        return null;
    }

    private bool IsValidTileForPath(Vector2Int position, Vector2Int destination, bool forceDirect = false)
    {
        if (!IsInBounds(position))
            return false;

        if (position == destination)
            return true;

        if (towerTiles.Contains(position))
            return false;

        TileType type = grid[position.x, position.y];

        if (type == TileType.Ground)
            return true;

        if (forceDirect)
        {
            return false;
        }
        else
        {
            if (
                (
                    type == TileType.Path
                    || type == TileType.Turn
                    || type == TileType.TJunction
                    || type == TileType.CrossJunction
                ) && IsAdjacentToCenter(position)
            )
                return true;
        }

        return false;
    }

    private bool IsAdjacentToCenter(Vector2Int position) => Vector2Int.Distance(position, startPoint) <= 1.5f;

    private bool IsNearExistingPath(Vector2Int position)
    {
        var neighbors = GetNeighbors(position);
        foreach (var neighbor in neighbors)
        {
            if (occupiedTiles.Contains(neighbor))
                return true;
        }

        return false;
    }

    private Vector2Int FindNodeWithLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int lowest = openSet[0];
        float lowestScore = fScore[lowest];

        for (int i = 1; i < openSet.Count; i++)
        {
            if (fScore[openSet[i]] < lowestScore)
            {
                lowest = openSet[i];
                lowestScore = fScore[lowest];
            }
        }

        return lowest;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<Vector2Int>
        {
            position + Vector2Int.up,
            position + Vector2Int.right,
            position + Vector2Int.down,
            position + Vector2Int.left
        };

        neighbors.RemoveAll(n => !IsInBounds(n));
        return neighbors;
    }

    private bool IsInBounds(Vector2Int position) =>
        position.x >= 0 && position.x < gridSize && position.y >= 0 && position.y < gridSize;

    private float ManhattanDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    private void ApplyPathToGrid(List<Vector2Int> path)
    {
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int prev = path[i - 1];
            Vector2Int current = path[i];
            Vector2Int next = path[i + 1];

            if (grid[current.x, current.y] == TileType.Ground)
            {
                Vector2Int dirFromPrev = current - prev;
                Vector2Int dirToNext = next - current;

                if (dirFromPrev != dirToNext)
                    grid[current.x, current.y] = TileType.Turn;
                else
                    grid[current.x, current.y] = TileType.Path;
            }
        }
    }

    private void InstantiateTiles()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = new(x * tileSize, 0, z * tileSize);

                switch (grid[x, z])
                {
                    case TileType.Ground:
                        Instantiate(groundTilePrefab, position, Quaternion.identity, mapParent);
                        break;

                    case TileType.Path:
                        Quaternion pathRotation = GetPathRotation(new Vector2Int(x, z));
                        Instantiate(pathStraightPrefab, position, pathRotation, mapParent);
                        break;

                    case TileType.Turn:
                        Quaternion turnRotation = GetTurnRotation(new Vector2Int(x, z));
                        Instantiate(pathTurnPrefab, position, turnRotation, mapParent);
                        break;

                    case TileType.TJunction:
                        Quaternion tJunctionRotation = GetTJunctionRotation(new Vector2Int(x, z));
                        Instantiate(pathTJunctionPrefab, position, tJunctionRotation, mapParent);
                        break;

                    case TileType.CrossJunction:
                        Instantiate(pathCrossJunctionPrefab, position, Quaternion.identity, mapParent);
                        break;

                    case TileType.CenterTile:
                        Instantiate(centerTowerTilePrefab, position, Quaternion.identity, mapParent);
                        break;
                }
            }
        }
    }

    private Quaternion GetTJunctionRotation(Vector2Int position)
    {
        if (!junctionConnections.ContainsKey(position))
            return Quaternion.Euler(0, 180, 0);

        var connections = junctionConnections[position];

        bool hasUp = connections.Contains(Vector2Int.up);
        bool hasDown = connections.Contains(Vector2Int.down);
        bool hasLeft = connections.Contains(Vector2Int.left);
        bool hasRight = connections.Contains(Vector2Int.right);

        if (hasUp && hasLeft && hasRight && !hasDown)
        {
            return Quaternion.Euler(0, 0, 0);
        }
        else if (hasDown && hasLeft && hasRight && !hasUp)
        {
            return Quaternion.Euler(0, 180, 0);
        }
        else if (hasLeft && hasUp && hasDown && !hasRight)
        {
            return Quaternion.Euler(0, 270, 0);
        }
        else if (hasRight && hasUp && hasDown && !hasLeft)
        {
            return Quaternion.Euler(0, 90, 0);
        }

        // Fallback analysis
        Dictionary<Vector2Int, int> directionUsage = new();

        foreach (var path in paths)
        {
            int index = path.IndexOf(position);
            if (index >= 0)
            {
                if (index > 0)
                {
                    Vector2Int dir = path[index - 1] - position;
                    directionUsage[dir] = directionUsage.GetValueOrDefault(dir, 0) + 1;
                }
                if (index < path.Count - 1)
                {
                    Vector2Int dir = path[index + 1] - position;
                    directionUsage[dir] = directionUsage.GetValueOrDefault(dir, 0) + 1;
                }
            }
        }

        Vector2Int stemDirection = Vector2Int.zero;
        foreach (var kvp in directionUsage)
        {
            if (kvp.Value == 1)
            {
                stemDirection = kvp.Key;
                break;
            }
        }

        if (stemDirection == Vector2Int.up)
            return Quaternion.Euler(0, 180, 0);
        if (stemDirection == Vector2Int.right)
            return Quaternion.Euler(0, 270, 0);
        if (stemDirection == Vector2Int.down)
            return Quaternion.Euler(0, 0, 0);
        if (stemDirection == Vector2Int.left)
            return Quaternion.Euler(0, 90, 0);

        return Quaternion.Euler(0, 180, 0);
    }

    private void SpawnTowers()
    {
        Vector3 centerPosition = new(startPoint.x * tileSize, towerHeightOffset, startPoint.y * tileSize);
        Instantiate(playerBasePrefab, centerPosition, Quaternion.identity, mapParent);

        foreach (var towerPos in towerPositions)
        {
            Vector3 position = new(towerPos.x * tileSize, towerHeightOffset, towerPos.y * tileSize);
            Quaternion towerRotation = GetEnemyTowerRotation(towerPos);
            Instantiate(enemyTowerPrefab, position, towerRotation, mapParent);
        }
    }

    private Quaternion GetPathRotation(Vector2Int position)
    {
        foreach (var path in paths)
        {
            int index = path.IndexOf(position);
            if (index > 0 && index < path.Count - 1)
            {
                Vector2Int dir = path[index + 1] - path[index - 1];

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                    return Quaternion.Euler(0, 90, 0);
                else
                    return Quaternion.identity;
            }
        }

        return Quaternion.identity;
    }

    private Quaternion GetTurnRotation(Vector2Int position)
    {
        foreach (var path in paths)
        {
            int index = path.IndexOf(position);
            if (index > 0 && index < path.Count - 1)
            {
                Vector2Int prevPos = path[index - 1];
                Vector2Int nextPos = path[index + 1];

                if (
                    prevPos.x < position.x && nextPos.y > position.y
                    || prevPos.y > position.y && nextPos.x < position.x
                )
                    return Quaternion.Euler(0, 0, 0);
                if (
                    prevPos.x > position.x && nextPos.y > position.y
                    || prevPos.y > position.y && nextPos.x > position.x
                )
                    return Quaternion.Euler(0, 90, 0);
                if (
                    prevPos.x > position.x && nextPos.y < position.y
                    || prevPos.y < position.y && nextPos.x > position.x
                )
                    return Quaternion.Euler(0, 180, 0);
                if (
                    prevPos.x < position.x && nextPos.y < position.y
                    || prevPos.y < position.y && nextPos.x < position.x
                )
                    return Quaternion.Euler(0, 270, 0);
            }
        }

        return Quaternion.identity;
    }

    private Quaternion GetEnemyTowerRotation(Vector2Int position)
    {
        foreach (var path in paths)
        {
            if (path[0] == position && path.Count > 1)
            {
                Vector2Int direction = path[1] - path[0];

                if (direction == Vector2Int.up)
                    return Quaternion.Euler(0, 0, 0);
                if (direction == Vector2Int.right)
                    return Quaternion.Euler(0, 90, 0);
                if (direction == Vector2Int.down)
                    return Quaternion.Euler(0, 180, 0);
                if (direction == Vector2Int.left)
                    return Quaternion.Euler(0, 270, 0);
            }
        }

        return Quaternion.identity;
    }

    private void ClearExistingMap()
    {
        for (int i = mapParent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(mapParent.GetChild(i).gameObject);
#else
            Destroy(mapParent.GetChild(i).gameObject);
#endif
        }

        paths.Clear();
        towerPositions.Clear();
        occupiedTiles.Clear();
        towerTiles.Clear();
        pathConnectionCount.Clear();
        junctionConnections.Clear();
        directPathEndpoints.Clear();
    }
}

#region Reference List
/*

Anthropic. 2025. Claude Sonnet (Version 4). [Large language model]. Available at: https://claude.ai/ [Accessed: 15 August 2025].

*/
#endregion
