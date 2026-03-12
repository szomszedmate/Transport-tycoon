using System.Collections.Generic;
using UnityEngine;

public class BuildingGrid : MonoBehaviour
{
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    private BuildingGridCell[,] grid;

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
    }

    public void SetBuilding(Building building, Vector3 buildingPosition)
    {
            (int x, int y) = WorldToGridPosition(buildingPosition);
            grid[x, y].SetBuilding(building);
    }

    public void RemBuilding(Vector3 buildingPosition)
    {
        (int x, int y) = WorldToGridPosition(buildingPosition);
        grid[x, y].Cell_RemBuilding();
    }
    private (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / BuildingSystem.CellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / BuildingSystem.CellSize);
        return (x, y);
    }

    public bool CanBuild(Vector3 buildingPosition)
    {
        (int x, int y) = WorldToGridPosition(buildingPosition);
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (!grid[x, y].IsEmpty()) return false;
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
}

public class BuildingGridCell
{
    private Building building;
    public void SetBuilding(Building build)
    {
        this.building = build;
    }

    public void Cell_RemBuilding()
    {
        if (building == null) return;
        Object.Destroy(building.gameObject);
        building = null;
    }

    public bool IsEmpty()
    {
        return this.building == null;
    }
}