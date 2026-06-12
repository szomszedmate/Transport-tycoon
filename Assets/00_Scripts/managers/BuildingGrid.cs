using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using static UnityEditor.FilePathAttribute;
#endif

public class BuildingGrid : MonoBehaviour
{
    public delegate void LocationsRegisteredEventHandler(object sender, LocationsRegisteredEventArgs e);
    public event LocationsRegisteredEventHandler LocationsRegistered;

    [Header("Grid Size")]
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;

    [Header("Prefabs")]
    [SerializeField] private TreeVisual treeVisualPrefab;
    [SerializeField] private GameObject waterTilePrefab;

    [Header("Containers")]
    [SerializeField] private Transform treeContainer;
    [SerializeField] private Transform waterContainer;

    [Header("Settings")]
    [SerializeField] private int maxTreeAttempts = 1000;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private bool skipTerrainCheck = false;

    public Water water;
    private BuildingGridCell[,] grid;
    private List<ILocation> locations;
    private List<Road> roads;
    private TreeVisual[,] treeVisuals;
    private GameObject[,] waterVisuals;
    private const float occupancyCheckRadius = BuildingSystem.CellSize * 0.76f;
    public BuildingGridCell[,] Grid { get => grid; private set => grid = value; }
    public int Width { get => width; set => width = value; }
    public int Height { get => height; set => height = value; }
    public List<ILocation> Locations { get => locations; set => locations = value; }

    private void Awake()
    {
        Grid = new BuildingGridCell[Width, Height];
        treeVisuals = new TreeVisual[Width, Height];
        waterVisuals = new GameObject[Width, Height];
    }

    private void Start()
    {
        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                Grid[i, j] = new();
            }
        }

        RegisterExistingObjects();
        InitializeWater();
        InitializeTrees();
        RefreshAllWaterVisuals();
        RefreshAllTreeVisuals();
    }

    public bool IsRoad(Vector3 worldPos)
    {
        (int x, int y) = WorldToGridPosition(worldPos);
        return (Grid[x, y].IsRoad());
    }

    public Road GetRoad(Vector3 worldPos)
    {
        (int x, int y) = WorldToGridPosition(worldPos);
        if (Grid[x, y].IsRoad())
        {
            return Grid[x, y].Road;
        }
        return null;
    }
 
    public ILocation GetLocationAt(Vector3 worldPos)
    {
        (int x, int y) = WorldToGridPosition(worldPos);
        for (int i = -1; i <= 1; i++) // check tiles around it
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;
                if (newX < 0 || newX >= Width || newY < 0 || newY >= Height) continue; // continue if out of bounds
                if (Grid[newX, newY].IsLocation())
                {
                    return Grid[newX, newY].Location;
                }
            }
        }
        return null;
    }

    private void RegisterExistingObjects()
    {
        Locations = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ILocation>().ToList();
        // init buildings to grid
        foreach (var location in Locations)
        {
            foreach (Vector3 pos in location.GetAllBuildingPositions())
            {
                (int x, int y) = WorldToGridPosition(pos);

                if (x >= 0 && x < Width && y >= 0 && y < Height)
                {
                    Grid[x, y].RegLocation(location);
                }
            }
        }

        // init base map roads to grid
        roads = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<Road>().ToList();
        foreach (var road in roads)
        {
            road.ForceSetup();

            Vector3 pos = ((MonoBehaviour)road).transform.position;
            (int x, int y) = WorldToGridPosition(pos);

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Grid[x, y].SetRoad(road);
            }
        }
        LocationsRegistered?.Invoke(this, new LocationsRegisteredEventArgs { RegisteredLocations = Locations, RegisteredRoads  = roads});
    }

    public void SetRoad(Road road, Vector3 roadPosition)
    {
        (int x, int y) = WorldToGridPosition(roadPosition);
        if (!IsInsideGrid(x, y)) return;

        bool isWater = Grid[x, y].IsWater();
        bool isBridge = road != null && road.IsBridge;

        if (isWater && !isBridge) return;
        if (!isWater && isBridge) return;

        Grid[x, y].SetRoad(road);
    }

    public void SetBusStop(BusStop busStop, Vector3 busStopPosition)
    {
        (int x, int y) = WorldToGridPosition(busStopPosition);
        Grid[x, y].SetBusStop(busStop);
    }
    public void SetVehicle(IVehicle vehicle, Vector3 vehiclePosition)
    {
        (int x, int y) = WorldToGridPosition(vehiclePosition);
        if (!IsInsideGrid(x, y)) return;
        if (Grid[x, y].IsWater()) return;

        Grid[x, y].SetVehicle(vehicle);
    }

    public bool IsCityRoad(Vector3 roadPosition)
    {
        (int x, int y) = WorldToGridPosition(roadPosition);
        return Grid[x, y].Cell_IsCityRoad();
    }

    public void RemRoad(Vector3 roadPosition)
    {
        (int x, int y) = WorldToGridPosition(roadPosition);
        Grid[x, y].Cell_RemRoad();
    }
    private bool IsInsideGrid(int col, int row)
    {
        return col >= 0 && col < Width && row >= 0 && row < Height;
    }
    public (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / BuildingSystem.CellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / BuildingSystem.CellSize);
        return (x, y);
    }

    private Vector3 GridToWorldCenterPosition(int col, int row)
    {
        float x = transform.position.x + col * BuildingSystem.CellSize + BuildingSystem.CellSize / 2f;
        float z = transform.position.z + row * BuildingSystem.CellSize + BuildingSystem.CellSize / 2f;
        return new Vector3(x, 100f, z);
    }

    public int GetWidth()
    {
        return Width;
    }

    public int GetHeight()
    {
        return Height;
    }

    private List<(int, int)> GetNeighbors(int col, int row)
    {
        List<(int, int)> neighbors = new();

        int[,] directions = new int[,]
        {
            { -1, 0 },
            { 1, 0 },
            { 0, -1 },
            { 0, 1 }
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int newCol = col + directions[i, 0];
            int newRow = row + directions[i, 1];

            if (IsInsideGrid(newCol, newRow))
            {
                neighbors.Add((newCol, newRow));
            }
        }

        return neighbors;

    }

    public bool CanBuildBus(Vector3 busPosition)
    {
        (int x, int y) = WorldToGridPosition(busPosition);
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        if (Grid[x, y].IsRoad() && Grid[x, y].Road.Road_HasBusStop()) return true;
        return false;
    }

    public bool CanBuildRoad(Vector3 position)
    {
        (int x, int y) = WorldToGridPosition(position);
        if (!IsInsideGrid(x, y)) return false;

        if (Grid[x, y].IsWater()) return false;
        if (!Grid[x, y].IsEmpty()) return false;

        foreach (ILocation location in locations)
        {
            foreach (Vector3 pos in location.GetAllBuildingPositions())
            {
                if (Vector3.Distance(pos, position) < occupancyCheckRadius)
                {
                    return false;
                }
            }
        }

        return true;
    }
    public StopType GetStopType(Vector3 busStopPosition)
    {
        (int x, int y) = WorldToGridPosition(busStopPosition);
        for (int i = -1; i <= 1; i++) // check tiles around it
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;
                if (newX < 0 || newX >= Width || newY < 0 || newY >= Height) continue; // continue if out of bounds
                if (Grid[newX, newY].IsLocation())
                {

                    return Grid[newX, newY].GetStopType();
                }

            }
        }
        return StopType.None;
    }

    public bool CanBuildBusStop(Vector3 busStopPosition) // checks for city/industry nearby
    {
        (int x, int y) = WorldToGridPosition(busStopPosition);
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false; // out of bounds
        if (!Grid[x, y].IsRoad() || Grid[x, y].IsCorner()) return false; // false if not road

        for (int i = -1; i <= 1; i++) // check tiles around it
        {
            for (int j = -1; j <= 1; j++)
            {
                int newX = x + i;
                int newY = y + j;
                if (newX < 0 || newX >= Width || newY < 0 || newY >= Height) continue; // continue if out of bounds
                if (Grid[newX, newY].IsLocation())
                {
                    return true;
                }

            }
        }
        return false;
    }

    public bool CanBuildBridge(Vector3 position, RoadData roadData, float rotation)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return false;

        if (!Grid[col, row].IsWater()) return false;

        if (!Grid[col, row].IsEmpty()) return false;

        bool horizontal = IsHorizontalRotation(rotation);

        if (HasCrossingBridge(col, row, horizontal))
            return false;

        if (!IsBridgeSpanClear(col, row, horizontal, roadData))
            return false;

        int dCol1 = horizontal ? -1 : 0;
        int dRow1 = horizontal ? 0 : -1;

        int dCol2 = horizontal ? 1 : 0;
        int dRow2 = horizontal ? 0 : 1;

        int n1Col = col + dCol1;
        int n1Row = row + dRow1;
        int n2Col = col + dCol2;
        int n2Row = row + dRow2;

        if (IsDifferentBridgeType(n1Col, n1Row, roadData)) return false;
        if (IsDifferentBridgeType(n2Col, n2Row, roadData)) return false;

        bool side1Anchor = IsLandCell(n1Col, n1Row) || IsSameBridgeType(n1Col, n1Row, roadData);
        bool side2Anchor = IsLandCell(n2Col, n2Row) || IsSameBridgeType(n2Col, n2Row, roadData);

        if (!side1Anchor && !side2Anchor)
            return false;

        bool reachesLand1 = TraceBridgeSide(col, row, dCol1, dRow1, roadData, horizontal, out int span1);
        bool reachesLand2 = TraceBridgeSide(col, row, dCol2, dRow2, roadData, horizontal, out int span2);

        if (!reachesLand1 && !reachesLand2)
            return false;

        int totalSpan = span1 + span2 + 1; // +1 for the current new element

        return totalSpan <= roadData.MaxBridgeLength;
    }

    public bool CanBuild(Vector3 buildingPosition)
    {
        return CanBuildRoad(buildingPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (BuildingSystem.CellSize <= 0 || Width <= 0 || Height <= 0) return;
        Vector3 origin = transform.position;
        for (int y = 0; y < Height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * BuildingSystem.CellSize);
            Vector3 end = origin + new Vector3(Width * BuildingSystem.CellSize, 0.01f, y * BuildingSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x < Width; x++)
        {
            Vector3 start = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, Height * BuildingSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
    }

    #region TreeMethods
    private void InitializeTrees()
    {
        int startingCount = 5;

        int row;
        int col;
        int amount;

        for (int i = 0; i < startingCount; i++)
        {
            int attempts = 0;
            do
            {
                row = RandomNumberGenerator.GetInt32(0, Height);
                col = RandomNumberGenerator.GetInt32(0, Width);
                attempts++;
                if (attempts > maxTreeAttempts)
                {
                    Debug.LogWarning("Couldnt find position for tree!");
                    break;
                }
            } while (!CanTreeGrowHere(col, row));

            amount = RandomNumberGenerator.GetInt32(1, 4);
            SetTreeCount(GridToWorldCenterPosition(col, row), amount);
        }
    }


    public List<(int, int)> GetSpreadableNeighbors(int col, int row)
    {
        List<(int, int)> neighbors = GetNeighbors(col, row);
        List<(int, int)> validNeighbors = new();

        foreach (var coo in neighbors)
        {
            if (CanTreeGrowHere(coo.Item1, coo.Item2))
            {
                validNeighbors.Add(coo);
            }
        }
        return validNeighbors;
    }

    private bool CanTreeGrowHere(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return false;
        if (!Grid[col, row].IsEmpty() || Grid[col, row].HasTrees() || Grid[col, row].IsWater()) return false;

        if (skipTerrainCheck) return true;

        Vector3 worldPos = GridToWorldCenterPosition(col, row);
        if (!Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
        {
            return false;
        }

        return true;
    }

    public int GetTreeCount(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return 0;

        return Grid[col, row].GetTreeCount();
    }

    public bool HasTrees(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return false;

        return Grid[col, row].HasTrees();
    }

    public bool CanSpreadTrees(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return false;

        return Grid[col, row].CanSpreadTrees();
    }

    public void SetTreeCount(Vector3 position, int treeCount)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;
        if (Grid[col, row].IsWater()) return;

        Grid[col, row].SetTreeCount(treeCount);
        RefreshTreeVisual(col, row);
    }

    public void IncreaseTreeCount(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;
        if (grid[col, row].IsWater()) return;

        Grid[col, row].IncreaseTreeCount();
        RefreshTreeVisual(col, row);
    }

    public void ClearTrees(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;

        Grid[col, row].ClearTrees();
        RefreshTreeVisual(col, row);
    }

    //same but with grid positions
    public int GetTreeCount(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return 0;

        return Grid[col, row].GetTreeCount();
    }

    public bool CanSpreadTrees(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return false;
        }

        return Grid[col, row].CanSpreadTrees();
    }

    public void SetTreeCount(int col, int row, int treeCount)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        if (Grid[col, row].IsWater())
        {
            return;
        }

        Grid[col, row].SetTreeCount(treeCount);
        RefreshTreeVisual(col, row);
    }



    public void IncreaseTreeCount(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        if (Grid[col, row].IsWater())
        {
            return;
        }

        Grid[col, row].IncreaseTreeCount();
        RefreshTreeVisual(col, row);
    }
    //visualization
    private void RefreshTreeVisual(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        int treeCount = Grid[col, row].GetTreeCount();

        if (treeVisuals[col, row] == null)
        {
            if (treeCount <= 0)
            {
                return;
            }

            Vector3 position = GridToWorldCenterPosition(col, row);
            Vector3 surfacePoint = FindSurfaceAt(position);
            TreeVisual visual = Instantiate(treeVisualPrefab, surfacePoint, Quaternion.identity, treeContainer);
            treeVisuals[col, row] = visual;
        }

        if (treeCount <= 0)
        {
            Destroy(treeVisuals[col, row].gameObject);
            treeVisuals[col, row] = null;
            return;
        }

        treeVisuals[col, row].SetTreeCount(treeCount);
    }

    private void RefreshAllTreeVisuals()
    {
        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Height; row++)
            {
                RefreshTreeVisual(col, row);
            }
        }
    }

    #endregion
    #region TerrainMethods

    public Vector3 FindSurfaceAt(Vector3 worldPos)
    {
        if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit))
        {
            return hit.point;
        }
        Debug.LogWarning("Couldnt find surface at: " + worldPos);
        return worldPos;
    }

    public float GetTerrainHeightAtGrid(int col, int row)
    {
        Vector3 worldPos = GridToWorldCenterPosition(col, row);
        // Ray starts at 100f and goes downward (same as in GridToWorldCenterPosition)
        if (Physics.Raycast(new Vector3(worldPos.x, 100f, worldPos.z), Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point.y;
        }
        return 0f; // Default if no raycast hit
    }

    public float CalculateBridgeHeight(Vector3 currentPos, float rotation, RoadData roadData)
    {
        (int col, int row) = WorldToGridPosition(currentPos);
        bool horizontal = IsHorizontalRotation(rotation);

        int dCol = horizontal ? 1 : 0;
        int dRow = horizontal ? 0 : 1;

        int startCol = col;
        int startRow = row;
        while (IsInsideGrid(startCol - dCol, startRow - dRow) && IsWater(startCol - dCol, startRow - dRow))
        {
            startCol -= dCol;
            startRow -= dRow;
        }

        int endCol = col;
        int endRow = row;
        while (IsInsideGrid(endCol + dCol, endRow + dRow) && IsWater(endCol + dCol, endRow + dRow))
        {
            endCol += dCol;
            endRow += dRow;
        }

        float heightA = GetTerrainHeightAtGrid(startCol - dCol, startRow - dRow);
        float heightB = GetTerrainHeightAtGrid(endCol + dCol, endRow + dRow);

        float totalDist = Vector3.Distance(GridToWorldCenterPosition(startCol - dCol, startRow - dRow),
                                           GridToWorldCenterPosition(endCol + dCol, endRow + dRow));
        float currentDist = Vector3.Distance(GridToWorldCenterPosition(startCol - dCol, startRow - dRow),
                                             GridToWorldCenterPosition(col, row));

        float t = currentDist / totalDist;

        float floatHeight = 2.0f;
        float baseHeight = Mathf.Lerp(heightA, heightB, t);
        return baseHeight + floatHeight;
    }

    private void InitializeWater()
    {
        List<GameObject> waterTiles = GameObject.FindGameObjectsWithTag("WaterTile").ToList();
        foreach (GameObject waterTile in waterTiles)
        {
            (int x, int z) = WorldToGridPosition(waterTile.transform.position);
            if (IsInsideGrid(x, z))
            {
                Grid[x, z].SetTerrainType(TerrainType.Water);
                Grid[x, z].ClearTrees();

                waterVisuals[x, z] = waterTile;
            }
        }
    }

    public void SetWater(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return;

        Grid[col, row].SetTerrainType(TerrainType.Water);
        Grid[col, row].ClearTrees();
        RefreshTreeVisual(col, row);
        RefreshWaterVisual(col, row);
    }

    public void SetLand(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return;

        Grid[col, row].SetTerrainType(TerrainType.Land);
        RefreshWaterVisual(col, row);
    }

    public bool IsWater(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return false;
        return Grid[col, row].IsWater();
    }

    public bool IsWater(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return false;

        return Grid[col, row].IsWater();
    }

    public TerrainType GetTerrainType(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return TerrainType.Land;
        return Grid[col, row].GetTerrainType();
    }

    private void RefreshWaterVisual(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return;

        bool isWater = Grid[col, row].IsWater();

        if (isWater)
        {
            if (waterVisuals[col, row] == null)
            {
                Vector3 pos = GridToWorldCenterPosition(col, row);
                waterVisuals[col, row] = Instantiate(waterTilePrefab, pos, Quaternion.identity, waterContainer);
            }
        }
        else
        {
            if (waterVisuals[col, row] != null)
            {
                Destroy(waterVisuals[col, row]);
                waterVisuals[col, row] = null;
            }
        }
    }

    private void RefreshAllWaterVisuals()
    {
        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Height; row++)
            {
                RefreshWaterVisual(col, row);
            }
        }
    }

    public void SetSkipTerrainCheck(bool skip)
    {
        skipTerrainCheck = skip;
    }
    #endregion

    #region Bridge
    private bool IsHorizontalRotation(float rotation)
    {

        float normalized = rotation % 180f;
        if (normalized < 0) normalized += 180f;

        return Mathf.Approximately(normalized, 0f);
    }

    private bool IsRoadHorizontal(Road road)
    {
        if (road == null || road.Model == null) return false;

        float rotation = road.Model.Rotation % 180f;
        if (rotation < 0) rotation += 180f;

        return Mathf.Approximately(rotation, 0f);
    }

    private bool HasCrossingBridge(int col, int row, bool horizontal)
    {
        if (!IsInsideGrid(col, row)) return false;

        Road road = Grid[col, row].Road;
        if (road == null || !road.IsBridge) return false;

        bool roadHorizontal = IsRoadHorizontal(road);
        return roadHorizontal != horizontal;
    }

    private bool TraceBridgeSide(int startCol, int startRow, int dCol, int dRow, RoadData roadData, bool horizontal, out int spanCount)
    {
        spanCount = 0;

        int col = startCol + dCol;
        int row = startRow + dRow;

        while (IsInsideGrid(col, row))
        {
            if (IsLandCell(col, row))
            {
                return true;
            }

            if (!Grid[col, row].IsWater())
            {
                return false;
            }

            if (HasCrossingBridge(col, row, horizontal))
            {
                return false;
            }

            Road road = Grid[col, row].Road;
            if (road != null)
            {
                if (!road.IsBridge) return false;
                if (road.data != roadData) return false;
            }

            spanCount++;
            col += dCol;
            row += dRow;
        }

        return false;
    }

    private bool IsSameBridgeType(int col, int row, RoadData roadData)
    {

        if (!IsInsideGrid(col, row)) return false;

        Road road = Grid[col, row].Road;
        if (road == null) return false;
        if (!road.IsBridge) return false;

        return road.data == roadData;
    }

    private bool IsLandCell(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return false;
        return !Grid[col, row].IsWater();
    }

    private bool IsDifferentBridgeType(int col, int row, RoadData roadData)
    {
        if (!IsInsideGrid(col, row)) return false;

        Road road = Grid[col, row].Road;
        if (road == null) return false;
        if (!road.IsBridge) return false;

        return road.data != roadData;
    }

    private bool IsBridgeSpanClear(int startCol, int startRow, bool horizontal, RoadData roadData)
    {
        int dCol = horizontal ? 1 : 0;
        int dRow = horizontal ? 0 : 1;

        int col = startCol;
        int row = startRow;

        while (IsInsideGrid(col - dCol, row - dRow) && Grid[col - dCol, row - dRow].IsWater())
        {
            col -= dCol;
            row -= dRow;
        }

        while (IsInsideGrid(col, row) && Grid[col, row].IsWater())
        {
            Road road = Grid[col, row].Road;
            if (road != null)
            {
                if (!road.IsBridge) return false;
                if (road.data != roadData) return false;

                bool roadHorizontal = IsRoadHorizontal(road);
                if (roadHorizontal != horizontal) return false;
            }

            col += dCol;
            row += dRow;
        }

        return true;
    }
    #endregion




    public Direction? GetRelativeDirection(Road firstRoad, Road secondRoad)
    {
        float firstX = firstRoad.transform.position.x;
        float firstZ = firstRoad.transform.position.z;
        float nextX = secondRoad.transform.position.x;
        float nextZ = secondRoad.transform.position.z;
        if (firstZ < nextZ && firstX == nextX) // next road is north of it and 1 tile away
        {
            return Direction.N;
        }

        if (nextX < firstX && firstZ == nextZ) // next road is west of it and 1 tile away
        {
            return Direction.W;
        }
        if (nextZ < firstZ && firstX == nextX) // next road is south of it and 1 tile away
        {
            return Direction.S;
        }

        if (firstX < nextX && nextX - firstX <= 10 && firstZ == nextZ) // next road is east of it and 1 tile away
        {
            return Direction.E;
        }
        return null;
    }

    public Direction? GetRelativePreviewDirection(RoadPreview firstRoad, Road secondRoad)
    {
        float firstX = firstRoad.transform.position.x;
        float firstZ = firstRoad.transform.position.z;
        float nextX = secondRoad.transform.position.x;
        float nextZ = secondRoad.transform.position.z;
        if (firstZ < nextZ && firstX == nextX) // next road is north of it and 1 tile away
        {
            return Direction.N;
        }

        if (nextX < firstX && firstZ == nextZ) // next road is west of it and 1 tile away
        {
            return Direction.W;
        }
        if (nextZ < firstZ && firstX == nextX) // next road is south of it and 1 tile away
        {
            return Direction.S;
        }

        if (firstX < nextX && nextX - firstX <= 10 && firstZ == nextZ) // next road is east of it and 1 tile away
        {
            return Direction.E;
        }
        return null;
    }

    public bool IsPreviewConnectedTo(RoadPreview firstRoad, Road nextRoad, Direction vehicleDirection)
    {
        bool nextToEachOther = false;
        bool roadsMatching = false;
        float firstX = firstRoad.transform.position.x;
        float firstZ = firstRoad.transform.position.z;
        float nextX = nextRoad.transform.position.x;
        float nextZ = nextRoad.transform.position.z;
        switch (vehicleDirection)
        {
            case Direction.N:
                if (firstZ < nextZ && nextZ - firstZ <= 10 && firstX == nextX) // next road is north of it and 1 tile away
                {
                    nextToEachOther = true;
                }
                break;
            case Direction.W:
                if (nextX < firstX && firstX - nextX <= 10 && firstZ == nextZ)
                    nextToEachOther = true;
                break;
            case Direction.S:
                if (nextZ < firstZ && firstZ - nextZ <= 10 && firstX == nextX)
                    nextToEachOther = true;
                break;
            case Direction.E:
                if (firstX < nextX && nextX - firstX <= 10 && firstZ == nextZ) // next road is east of it and 1 tile away
                {
                    nextToEachOther = true;
                }
                break;
            default:
                break;
        }
        if (firstRoad.RoadModel.Outputs.Contains<Direction>(vehicleDirection) && nextRoad.Model.Inputs.Contains<Direction>(vehicleDirection)) roadsMatching = true; // the vehicle can leave this road and enter next road based on direction
        return roadsMatching && nextToEachOther;
    }


    public enum TerrainType
    {
        Land,
        Water
    }

    public class BuildingGridCell
    {
        public Road Road { get; private set; }
        private ILocation location;
        public ILocation Location { get => location; private set => location = value; }
        private IVehicle vehicle;
        private BusStop busStop;
        private int treeCount;
        private TerrainType terrainType = TerrainType.Land;

        #region TerrainMethods
        public TerrainType GetTerrainType()
        {
            return terrainType;
        }

        public void SetTerrainType(TerrainType terrainType)
        {
            this.terrainType = terrainType;
        }

        public bool IsWater()
        {
            return terrainType == TerrainType.Water;
        }

        public bool IsLand()
        {
            return terrainType == TerrainType.Land;
        }

        #endregion
        #region TreeMethods
        public int GetTreeCount()
        {
            return treeCount;
        }

        public void SetTreeCount(int treeCount)
        {
            if (treeCount >= 0 && treeCount <= 4)
            {
                this.treeCount = treeCount;
            }

        }

        public void IncreaseTreeCount()
        {
            if (treeCount < 4)
            {
                treeCount++;
            }

        }

        public bool HasTrees()
        {
            return treeCount > 0;
        }

        public bool CanSpreadTrees()
        {
            return treeCount > 2;
        }

        public void ClearTrees()
        {
            treeCount = 0;
        }
        #endregion

        public void RegLocation(ILocation location)
        {
            this.Location = location;
        }

        public void SetVehicle(IVehicle vehicle)
        {
            this.vehicle = vehicle;
        }

        public void SetBusStop(BusStop busStop)
        {
            this.busStop = busStop;
            Road.SetBusStop(busStop);
        }

        public void SetRoad(Road road)
        {
            this.Road = road;
        }

        public bool Cell_IsCityRoad()
        {
            return Road.IsCityRoad;
        }

        public bool IsCorner()
        {
            if (Road is null) return false;
            return (Road.IsCorner);
        }

        public void Cell_RemRoad()
        {
            if (Road == null) return;
            UnityEngine.Object.Destroy(Road.gameObject);
            Road = null;
        }

        public bool IsLocation()
        {
            return this.Location != null;
        }

        public StopType GetStopType()
        {
            return this.Location.Type;
        }

        public bool IsRoad()
        {
            return this.Road != null;
        }

        public bool IsEmpty()
        {
            return this.Road == null && this.Location == null;
        }
    }
}