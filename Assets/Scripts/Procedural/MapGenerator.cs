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

    [
        SerializeField,
        Range(0f, 0.5f),
        Tooltip("Small penalty for staying near edges (higher = slight center preference)")
    ]
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

    [SerializeField, Tooltip("Special tile placed under enemy towers (like the center tile)")]
    private GameObject enemyTowerTilePrefab;

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
    private readonly HashSet<Vector2Int> pathTiles = new(); // Tracks all carved path tiles for lane width

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
        CreatePaths(); // exactly 3 lanes: widths 1, 2, 3
        AddCenterHub(); // surround center and integrate lanes
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
        pathTiles.Clear();

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
        // Build exactly three lanes with widths 1, 2, and 3 (in that order), if enough towers exist.
        int[] widths = { 1, 2, 3 };
        int lanesToBuild = Mathf.Min(3, towerPositions.Count);

        for (int i = 0; i < lanesToBuild; i++)
        {
            Vector2Int towerPos = towerPositions[i];
            List<Vector2Int> path = FindPathWithRetries(towerPos, startPoint);
            if (path != null && path.Count > 0)
            {
                paths.Add(path);
                CarvePathWithWidth(path, widths[i]);

                // NEW: surround the enemy tower with a hub matching this lane's width
                AddEnemyHub(towerPos, widths[i]);
            }
            else
            {
                Debug.LogWarning($"Could not find path from tower at {towerPos} to center at {startPoint}");
            }
        }

        if (towerPositions.Count < 3)
            Debug.LogWarning("Fewer than 3 towers placed; could not create all three lane widths (1,2,3).");
        if (towerPositions.Count > 3)
            Debug.LogWarning("More than 3 towers placed; only the first three were connected (widths 1,2,3).");
    }

    // Carve a widened lane along the path, excluding endpoints (tower and center)
    private void CarvePathWithWidth(List<Vector2Int> path, int width)
    {
        if (path == null || path.Count < 2)
            return;

        width = Mathf.Clamp(width, 1, 3);

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int current = path[i];
            Vector2Int prevDir = (path[i] - path[i - 1]);
            Vector2Int nextDir = (path[i + 1] - path[i]);

            // Normalize to cardinal unit vectors
            prevDir = new Vector2Int(Mathf.Clamp(prevDir.x, -1, 1), Mathf.Clamp(prevDir.y, -1, 1));
            nextDir = new Vector2Int(Mathf.Clamp(nextDir.x, -1, 1), Mathf.Clamp(nextDir.y, -1, 1));

            // Union of offsets for both segment directions (fills corners cleanly)
            foreach (var o in GetPerpOffsets(prevDir, width))
                MarkPathTile(current + o);

            foreach (var o in GetPerpOffsets(nextDir, width))
                MarkPathTile(current + o);
        }
    }

    // Add a small center hub so all lanes meet cleanly and center is surrounded
    private void AddCenterHub()
    {
        // Use max width (3) so any incoming widened lane integrates cleanly
        int centerWidth = 3;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var d in dirs)
        {
            foreach (var o in GetPerpOffsets(d, centerWidth))
                MarkPathTile(startPoint + d + o);
        }

        // Diagonals to fully surround the center tile
        Vector2Int[] diags = { new(+1, +1), new(-1, +1), new(+1, -1), new(-1, -1) };
        foreach (var dd in diags)
            MarkPathTile(startPoint + dd);
    }

    // NEW: surround a given enemy tower with a hub of the given width
    private void AddEnemyHub(Vector2Int towerPos, int width)
    {
        width = Mathf.Clamp(width, 1, 3);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var d in dirs)
        {
            foreach (var o in GetPerpOffsets(d, width))
                MarkPathTile(towerPos + d + o);
        }

        // Diagonals to fully surround the tower tile
        Vector2Int[] diags = { new(+1, +1), new(-1, +1), new(+1, -1), new(-1, -1) };
        foreach (var dd in diags)
            MarkPathTile(towerPos + dd);
    }

    private IEnumerable<Vector2Int> GetPerpOffsets(Vector2Int dir, int width)
    {
        // Determine perpendicular axis: vertical dirs -> x offsets; horizontal dirs -> y offsets
        Vector2Int perp = (dir == Vector2Int.up || dir == Vector2Int.down) ? Vector2Int.right : Vector2Int.up;

        width = Mathf.Clamp(width, 1, 3);

        if (width == 1)
        {
            yield return Vector2Int.zero;
            yield break;
        }

        if (width == 2)
        {
            yield return Vector2Int.zero;
            yield return perp; // +perp side
            yield break;
        }

        // width == 3
        yield return -perp;
        yield return Vector2Int.zero;
        yield return perp;
    }

    private List<Vector2Int> FindPathWithRetries(Vector2Int start, Vector2Int end)
    {
        for (int attempt = 0; attempt < maxPathfindingAttempts; attempt++)
        {
            List<Vector2Int> path = FindPath(start, end, attempt);
            if (path != null)
                return path;
        }

        return null;
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, int attempt = 0)
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
                if (closedSet.Contains(neighbor) || !IsValidTileForPath(neighbor, end))
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

                if (straightCount >= 6)
                {
                    moveCost += straightLinePenalty;
                }

                // Spread out from already carved lanes, but still allow approaching center
                if (IsNearExistingPath(neighbor) && !IsAdjacentToCenter(neighbor))
                {
                    moveCost += pathAvoidancePenalty;
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
        return FindFallbackPath(start, end);
    }

    private List<Vector2Int> FindFallbackPath(Vector2Int start, Vector2Int end)
    {
        Vector2Int bestConnectionPoint;
        float bestDistance = float.MaxValue;
        List<Vector2Int> bestPathToConnection = null;

        foreach (var pathTile in occupiedTiles)
        {
            if (Vector2Int.Distance(pathTile, end) <= fallbackConnectionRadius)
            {
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

    private bool IsValidTileForPath(Vector2Int position, Vector2Int destination)
    {
        if (!IsInBounds(position))
            return false;

        if (position == destination)
            return true;

        if (towerTiles.Contains(position))
            return false;

        // Only traverse ground during path search
        return grid[position.x, position.y] == TileType.Ground;
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
            position + Vector2Int.left,
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

    private void InstantiateTiles()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector2Int pos = new(x, z);
                Vector3 position = new(x * tileSize, 0, z * tileSize);

                if (pos == startPoint)
                {
                    grid[x, z] = TileType.CenterTile;
                    Instantiate(centerTowerTilePrefab, position, Quaternion.identity, mapParent);
                    continue;
                }

                if (towerTiles.Contains(pos))
                {
                    // Keep grid classification simple; visuals come from prefab
                    if (enemyTowerTilePrefab != null)
                        Instantiate(enemyTowerTilePrefab, position, Quaternion.identity, mapParent);
                    else
                        Instantiate(groundTilePrefab, position, Quaternion.identity, mapParent);

                    continue;
                }

                if (!pathTiles.Contains(pos))
                {
                    grid[x, z] = TileType.Ground;
                    Instantiate(groundTilePrefab, position, Quaternion.identity, mapParent);
                    continue;
                }

                // Determine connections (treat center & enemy tower tiles as connectable)
                bool up = IsPathOrCenter(pos + Vector2Int.up);
                bool right = IsPathOrCenter(pos + Vector2Int.right);
                bool down = IsPathOrCenter(pos + Vector2Int.down);
                bool left = IsPathOrCenter(pos + Vector2Int.left);

                int connCount = (up ? 1 : 0) + (right ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0);

                if (connCount == 4)
                {
                    grid[x, z] = TileType.CrossJunction;
                    int rotSteps = Random.Range(0, 4); // 0,1,2,3
                    Quaternion rot = Quaternion.Euler(0f, rotSteps * 90f, 0f);
                    Instantiate(pathCrossJunctionPrefab, position, rot, mapParent);
                }
                else if (connCount == 3)
                {
                    grid[x, z] = TileType.TJunction;

                    // Missing side determines rotation (matches prior GetTJunctionRotation semantics)
                    if (!down)
                        Instantiate(pathTJunctionPrefab, position, Quaternion.Euler(0, 0, 0), mapParent);
                    else if (!up)
                        Instantiate(pathTJunctionPrefab, position, Quaternion.Euler(0, 180, 0), mapParent);
                    else if (!left)
                        Instantiate(pathTJunctionPrefab, position, Quaternion.Euler(0, 90, 0), mapParent);
                    else // !right
                        Instantiate(pathTJunctionPrefab, position, Quaternion.Euler(0, 270, 0), mapParent);
                }
                else if (connCount == 2)
                {
                    // Straight vs Turn
                    if ((up && down) || (left && right))
                    {
                        grid[x, z] = TileType.Path;
                        Quaternion rot = (left && right) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                        Instantiate(pathStraightPrefab, position, rot, mapParent);
                    }
                    else
                    {
                        grid[x, z] = TileType.Turn;
                        // Map adjacent pairs to rotation
                        if (up && right)
                            Instantiate(pathTurnPrefab, position, Quaternion.Euler(0, 90, 0), mapParent);
                        else if (right && down)
                            Instantiate(pathTurnPrefab, position, Quaternion.Euler(0, 180, 0), mapParent);
                        else if (down && left)
                            Instantiate(pathTurnPrefab, position, Quaternion.Euler(0, 270, 0), mapParent);
                        else // left && up
                            Instantiate(pathTurnPrefab, position, Quaternion.Euler(0, 0, 0), mapParent);
                    }
                }
                else if (connCount == 1)
                {
                    // End-cap: use straight with appropriate orientation
                    grid[x, z] = TileType.Path;
                    Quaternion rot = (left || right) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                    Instantiate(pathStraightPrefab, position, rot, mapParent);
                }
                else
                {
                    // Isolated (shouldn't happen often) - place ground fallback
                    grid[x, z] = TileType.Ground;
                    Instantiate(groundTilePrefab, position, Quaternion.identity, mapParent);
                }
            }
        }
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
        pathTiles.Clear();
    }

    private void MarkPathTile(Vector2Int p)
    {
        if (!IsInBounds(p))
            return;
        if (towerTiles.Contains(p))
            return;
        if (p == startPoint)
            return;

        pathTiles.Add(p);
        occupiedTiles.Add(p);
    }

    // Helper: treat center and enemy tower base tiles as connectable for visuals
    private bool IsPathOrCenter(Vector2Int p)
    {
        if (!IsInBounds(p))
            return false;
        return pathTiles.Contains(p) || p == startPoint || towerTiles.Contains(p);
    }
}

#region Reference List
/*

Anthropic. 2025. Claude Sonnet (Version 4). [Large language model]. Available at: https://claude.ai/ [Accessed: 15 August 2025].

*/
#endregion
