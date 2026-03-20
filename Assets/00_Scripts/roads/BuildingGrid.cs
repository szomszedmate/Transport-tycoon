using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class BuildingGrid : MonoBehaviour
{
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField] private TreeVisual treeVisualPrefab;
    [SerializeField] private Transform treeContainer;
    private BuildingGridCell[,] grid;
    private List<ILocation> locations;
    private List<Road> roads;
    private TreeVisual[,] treeVisuals;

    private void Start()
    {
        grid = new BuildingGridCell[width, height];
        treeVisuals = new TreeVisual[width, height];

        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new();
            }
        }

        RegisterExistingObjects();
        InitializeTrees();
        RefreshAllTreeVisuals();
    }



    private void RegisterExistingObjects()
    {
        locations = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ILocation>().ToList();

        foreach (var location in locations)
        {
            Vector3 pos = ((MonoBehaviour)location).transform.position;
            (int x, int y) = WorldToGridPosition(pos);

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                grid[x, y].RegLocation(location);
            }
        }

        roads = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<Road>().ToList();
        foreach (var road in roads)
        {
            Vector3 pos = ((MonoBehaviour)road).transform.position;
            (int x, int y) = WorldToGridPosition(pos);

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                grid[x, y].SetRoad(road);
            }
        }
    }

    public void SetRoad(Road road, Vector3 roadPosition)
    {
            (int x, int y) = WorldToGridPosition(roadPosition);
            grid[x, y].SetRoad(road);
    }

    public void SetVehicle(IVehicle vehicle, Vector3 vehiclePosition)
    {
        (int x, int y) = WorldToGridPosition(vehiclePosition);
        grid[x, y].SetVehicle(vehicle);
    }

    public bool IsCityRoad(Vector3 roadPosition)
    {
        (int x, int y) = WorldToGridPosition(roadPosition);
        return grid[x, y].Cell_IsCityRoad();
    }

    public void RemRoad(Vector3 roadPosition)
    {
        (int x, int y) = WorldToGridPosition(roadPosition);
        grid[x, y].Cell_RemRoad();
    }
    private bool IsInsideGrid(int col, int row)
    {
        return col >= 0 && col < width && row >= 0 && row < height;
    }
    private (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / BuildingSystem.CellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / BuildingSystem.CellSize);
        return (x, y);
    }

    private Vector3 GridToWorldCenterPosition(int col, int row)
    {
        float x = transform.position.x + col * BuildingSystem.CellSize + BuildingSystem.CellSize / 2f;
        float z = transform.position.z + row * BuildingSystem.CellSize + BuildingSystem.CellSize / 2f;
        return new Vector3(x, 0f, z);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
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
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (!grid[x, y].IsEmpty()) return true;
        return false;
    }

    public bool CanBuild(Vector3 buildingPosition)
    {
        (int x, int y) = WorldToGridPosition(buildingPosition);
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        //if (!grid[x, y].Cell_IsCityRoad()) return false;
        if (!grid[x, y].IsEmpty()) return false;
        foreach (ILocation location in locations)
        {

            foreach (Vector3 pos in location.GetAllBuildingPositions())
            {
                if (Vector3.Distance(pos, buildingPosition) < BuildingSystem.CellSize * 0.76f)
                {
                    return false; // too close, inside a city
                }
            }
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (BuildingSystem.CellSize <= 0 || width <= 0 || height <= 0) return;
        Vector3 origin = transform.position;
        for (int y = 0; y < height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * BuildingSystem.CellSize);
            Vector3 end = origin + new Vector3(width * BuildingSystem.CellSize, 0.01f, y * BuildingSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x < width; x++)
        { 
            Vector3 start = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * BuildingSystem.CellSize, 0.01f, height * BuildingSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
    }

    #region TreeMethods
    private void InitializeTrees()
    {
        SetTreeCount(GridToWorldCenterPosition(2, 2), 2);
        SetTreeCount(GridToWorldCenterPosition(3, 2), 4);
        SetTreeCount(GridToWorldCenterPosition(6, 5), 1);
        SetTreeCount(GridToWorldCenterPosition(7, 5), 3);
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
        if (!IsInsideGrid(col, row))
        {
            return false;
        }

        return grid[col, row].IsEmpty() && !grid[col, row].HasTrees();
    }

    public int GetTreeCount(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return 0;

        return grid[col, row].GetTreeCount();
    }

    public bool HasTrees(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return false;

        return grid[col, row].HasTrees();
    }

    public bool CanSpreadTrees(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return false;

        return grid[col, row].CanSpreadTrees();
    }

    public void SetTreeCount(Vector3 position, int treeCount)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;

        grid[col, row].SetTreeCount(treeCount);
        RefreshTreeVisual(col, row);
    }

    public void IncreaseTreeCount(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;

        grid[col, row].IncreaseTreeCount();
        RefreshTreeVisual(col, row);
    }

    public void ClearTrees(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;

        grid[col, row].ClearTrees();
        RefreshTreeVisual(col, row);
    }

    //ugyanezek, csak grid pozíciókkal
    public int GetTreeCount(int col, int row)
    {
        if (!IsInsideGrid(col, row)) return 0;

        return grid[col, row].GetTreeCount();
    }

    public bool CanSpreadTrees(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return false;
        }

        return grid[col, row].CanSpreadTrees();
    }

    public void SetTreeCount(int col, int row, int treeCount)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        grid[col, row].SetTreeCount(treeCount);
        RefreshTreeVisual(col, row);
    }


    
    public void IncreaseTreeCount(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        grid[col, row].IncreaseTreeCount();
        RefreshTreeVisual(col, row);
    }
    //vizualizávió
    private void RefreshTreeVisual(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        int treeCount = grid[col, row].GetTreeCount();

        if (treeVisuals[col, row] == null)
        {
            if (treeCount <= 0)
            {
                return;
            }

            Vector3 position = GridToWorldCenterPosition(col, row);
            TreeVisual visual = Instantiate(treeVisualPrefab, position, Quaternion.identity, treeContainer);
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
        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                RefreshTreeVisual(col, row);
            }
        }
    }

    #endregion
}

public class BuildingGridCell
{
    private Road road;
    private ILocation location;
    private IVehicle vehicle;
    private int treeCount;

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
        this.location = location;
    }

    public void SetVehicle(IVehicle vehicle)
    {
        this.vehicle = vehicle;
    }

    public void SetRoad(Road road)
    {
        this.road = road;
    }

    public bool Cell_IsCityRoad()
    {
        return road.IsCityRoad;
    }

    public void Cell_RemRoad()
    {
        if (road == null) return;
        UnityEngine.Object.Destroy(road.gameObject);
        road = null;
    }

    public bool IsEmpty()
    {
        return this.road == null && this.location == null;
    }
}