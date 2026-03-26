using System;
using System.Collections.Generic;
using UnityEngine;

public class ForestSystem : MonoBehaviour
{
    [SerializeField] private BuildingGrid grid;
    [SerializeField] private float growthInterval = 5f;
    [SerializeField, Range(0f, 1f)] private float growthChance = 1f;
    [SerializeField, Range(0f, 1f)] private float spreadChance = 1f;

    private float timer = 0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= growthInterval)
        {
            timer -= growthInterval;
            ProcessForestTick();
        }
    }

    private void ProcessForestTick()
    {
        List<(int col, int row)> growthCells = new();
        List<(int col, int row)> spreadCells = new();

        for (int col = 0; col < grid.GetWidth(); col++)
        {
            for (int row = 0; row < grid.GetHeight(); row++)
            {
                int treeCount = grid.GetTreeCount(col, row);

                if (treeCount > 0 && treeCount < 4 && UnityEngine.Random.value <= growthChance)
                {
                    growthCells.Add((col, row));
                }

                if (grid.CanSpreadTrees(col, row) && UnityEngine.Random.value <= spreadChance)
                {
                    List<(int col, int row)> neighbors = grid.GetSpreadableNeighbors(col, row);

                    if (neighbors.Count > 0)
                    {
                        int randomIndex = UnityEngine.Random.Range(0, neighbors.Count);
                        spreadCells.Add(neighbors[randomIndex]);
                    }
                }
            }
        }

        foreach (var cell in growthCells)
        {
            grid.IncreaseTreeCount(cell.col, cell.row);
        }

        foreach (var cell in spreadCells)
        {
            if (grid.GetTreeCount(cell.col, cell.row) == 0)
            {
                grid.SetTreeCount(cell.col, cell.row, 1);
            }
        }
    }

}
