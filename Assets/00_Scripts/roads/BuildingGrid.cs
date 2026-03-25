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
    private BuildingGridCell[,] grid;
    private List<ILocation> locations;
    private List<Road> roads;

    private void Start()
    {
        grid = new BuildingGridCell[width, height];
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new();
            }
        }
        RegisterExistingObjects();
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
    private (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / BuildingSystem.CellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / BuildingSystem.CellSize);
        return (x, y);
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


    public class BuildingGridCell
    {
        private Road road;
        private ILocation location;
        private IVehicle vehicle;

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
            Object.Destroy(road.gameObject);
            road = null;
        }

        public bool IsEmpty()
        {
            return this.road == null && this.location == null;
        }
    }
}