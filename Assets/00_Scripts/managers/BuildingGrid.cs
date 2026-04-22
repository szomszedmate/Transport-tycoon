using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using static UnityEditor.FilePathAttribute;
#endif

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

    public BuildingGridCell[,] Grid { get => grid; private set => grid = value; } // for debug
    public int Width { get => width; set => width = value; } // for debug
    public int Height { get => height; set => height = value; } // for debug
    public List<ILocation> Locations { get => locations; set => locations = value; }

    private void Start()
    {
        Grid = new BuildingGridCell[Width, Height];
        treeVisuals = new TreeVisual[Width, Height];

        for (int i = 0; i < Grid.GetLength(0); i++)
        {
            for (int j = 0; j < Grid.GetLength(1); j++)
            {
                Grid[i, j] = new();
            }
        }

        RegisterExistingObjects();
        InitializeTrees();
        RefreshAllTreeVisuals();
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
        roads = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<Road>().ToList();
        foreach (var road in roads)
        {
            Vector3 pos = ((MonoBehaviour)road).transform.position;
            (int x, int y) = WorldToGridPosition(pos);

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Grid[x, y].SetRoad(road);
            }
        }
    }

    public void SetRoad(Road road, Vector3 roadPosition)
    {
        (int x, int y) = WorldToGridPosition(roadPosition);
        //Debug.Log("Road position: " + x +  ", " + y);
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
    public (int x, int y) WorldToGridPosition(Vector3 worldPosition) // public for debugging
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
        if (Grid[x, y].IsRoad()&&Grid[x,y].Road.Road_HasBusStop()) return true;
        return false;
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
        if (!Grid[x,y].IsRoad()) return false; // false if not road

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
    public bool CanBuild(Vector3 buildingPosition)
    {
        (int x, int y) = WorldToGridPosition(buildingPosition);
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        //if (!grid[x, y].Cell_IsCityRoad()) return false;
        if (!Grid[x, y].IsEmpty()) return false;
        foreach (ILocation location in Locations)
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

        return Grid[col, row].IsEmpty() && !Grid[col, row].HasTrees();
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

        Grid[col, row].SetTreeCount(treeCount);
        RefreshTreeVisual(col, row);
    }

    public void IncreaseTreeCount(Vector3 position)
    {
        (int col, int row) = WorldToGridPosition(position);
        if (!IsInsideGrid(col, row)) return;

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

    //ugyanezek, csak grid poz�ci�kkal
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

        Grid[col, row].SetTreeCount(treeCount);
        RefreshTreeVisual(col, row);
    }



    public void IncreaseTreeCount(int col, int row)
    {
        if (!IsInsideGrid(col, row))
        {
            return;
        }

        Grid[col, row].IncreaseTreeCount();
        RefreshTreeVisual(col, row);
    }
    //vizualiz�vi�
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
        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Height; row++)
            {
                RefreshTreeVisual(col, row);
            }
        }
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
}



public class BuildingGridCell
{
    public Road Road { get; private set; }
    public ILocation Location { get => location; private set => location = value; }

    private ILocation location;
    private IVehicle vehicle;
    private BusStop busStop;
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