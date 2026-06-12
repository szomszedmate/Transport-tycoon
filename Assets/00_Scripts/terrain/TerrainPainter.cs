using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainPainter : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    private float[,,] initialAlphamaps;

    [Header("Road Settings")]
    [SerializeField] public float roadWidth = 4f;      // Width in world units (meters)
    [SerializeField] public float roadLength = 10f;    // Length in world units (meters) - for straight roads

    public Terrain Terrain { get => terrain; private set => terrain = value; }

    private void Awake()
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogWarning("TerrainPainter: no terrain assigned, disabling.");
                enabled = false;
                return;
            }
        }

        TerrainData runtimeData = Instantiate(terrain.terrainData);
        terrain.terrainData = runtimeData;
        if (GetComponent<TerrainCollider>())
        {
            GetComponent<TerrainCollider>().terrainData = runtimeData;
        }
        SaveInitialAlphamaps();
    }

    public void PaintRoadAt(Vector3 snappedPos, RoadType type, float rotation, int layerIndex, List<int> mainLayers, bool isBasicPreview, int lastLayer)
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainLocalPos = snappedPos - terrain.transform.position;

        // 1. TÝpusfŘgg§ rotßciˇ korrekciˇ
        float finalRotation = rotation;
        switch (type)
        {
            case RoadType.STRAIGHT: finalRotation += 90; break;
            case RoadType.TURN: finalRotation = -rotation; break;
        }

        float rad = finalRotation * Mathf.Deg2Rad;
        float cosR = Mathf.Cos(-rad);
        float sinR = Mathf.Sin(-rad);

        // 2. Map units per pixel (how many world units per pixel?)
        float unitsPerPixelX = terrainData.size.x / (terrainData.alphamapWidth - 1);
        float unitsPerPixelZ = terrainData.size.z / (terrainData.alphamapHeight - 1);

        // Pixel coordinate (floating point, for the exact center)
        float mapX = (terrainLocalPos.x / terrainData.size.x) * (terrainData.alphamapWidth - 1);
        float mapZ = (terrainLocalPos.z / terrainData.size.z) * (terrainData.alphamapHeight - 1);

        // 3. Befoglalˇ keret (Bounds) meghatßrozßsa
        // Safety margin (+2) so the rotated shape isn't clipped at the edges
        float areaSize = Mathf.Max(roadWidth, roadLength) * 1.5f;
        int bWidth = Mathf.CeilToInt(areaSize / unitsPerPixelX) + 2;
        int bHeight = Mathf.CeilToInt(areaSize / unitsPerPixelZ) + 2;

        int startX = Mathf.Clamp(Mathf.FloorToInt(mapX - bWidth / 2f), 0, terrainData.alphamapWidth - 1);
        int startZ = Mathf.Clamp(Mathf.FloorToInt(mapZ - bHeight / 2f), 0, terrainData.alphamapHeight - 1);
        int width = Mathf.Min(bWidth, terrainData.alphamapWidth - startX);
        int height = Mathf.Min(bHeight, terrainData.alphamapHeight - startZ);

        float[,,] maps = terrainData.GetAlphamaps(startX, startZ, width, height);

        float halfW = roadWidth * 0.5f;
        float halfL = roadLength * 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 4. Calculate the pixel's actual world coordinate based on its center (+0.5f)
                // Subtract snappedPos to get a relative coordinate
                float currentWorldX = (startX + x) * unitsPerPixelX;
                float currentWorldZ = (startZ + y) * unitsPerPixelZ;

                float relX = currentWorldX - terrainLocalPos.x;
                float relZ = currentWorldZ - terrainLocalPos.z;

                // 5. Rotate back into local (0 degree) space
                float rotatedX = relX * cosR - relZ * sinR;
                float rotatedZ = relX * sinR + relZ * cosR;

                bool shouldIPaint = false;

                switch (type)
                {
                    case RoadType.STRAIGHT:
                        shouldIPaint = Mathf.Abs(rotatedX) <= halfW && Mathf.Abs(rotatedZ) <= halfL;
                        break;
                    case RoadType.TURN:
                        bool stem1 = Mathf.Abs(rotatedX) <= halfW && rotatedZ <= halfW && rotatedZ >= -halfL;
                        bool stem2 = rotatedX <= halfW && rotatedX >= -halfL && Mathf.Abs(rotatedZ) <= halfW;
                        shouldIPaint = stem1 || stem2;
                        break;
                    case RoadType.CROSS:
                        bool hor = Mathf.Abs(rotatedX) <= halfW && Mathf.Abs(rotatedZ) <= halfL;
                        bool ver = Mathf.Abs(rotatedX) <= halfL && Mathf.Abs(rotatedZ) <= halfW;
                        shouldIPaint = hor || ver;
                        break;
                    case RoadType.T:
                        bool main = Mathf.Abs(rotatedZ) <= halfW && Mathf.Abs(rotatedX) <= halfL;
                        bool side = Mathf.Abs(rotatedX) <= halfW && rotatedZ <= 0 && rotatedZ >= -halfL;
                        shouldIPaint = main || side;
                        break;
                }

                if (shouldIPaint)
                {
                    if (isBasicPreview) // improvement?
                    {
                        foreach(int i in mainLayers)
                        {
                            maps[y, x, i] = 0;
                        }
                    } else
                    {
                        maps[y, x, lastLayer] = 0;
                    }
                    maps[y, x, layerIndex] = 1f;
                }
            }
        }
        terrainData.SetAlphamaps(startX, startZ, maps);
    }

    public void ResetLayer(Vector3 snappedPos, List<int> activeLayers, RoadType type, float rotation)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainLocalPos = snappedPos - terrain.transform.position;

        // Ugyanaz a bounds szßmÝtßs mint PaintRoadAt-ban
        float rad = rotation * Mathf.Deg2Rad;
        float cos = Mathf.Abs(Mathf.Cos(rad));
        float sin = Mathf.Abs(Mathf.Sin(rad));

        float boundsX = Mathf.Max(roadWidth * cos + roadLength * sin, roadLength * cos + roadWidth * sin);
        float boundsZ = Mathf.Max(roadWidth * sin + roadLength * cos, roadLength * sin + roadWidth * cos);

        float mapX = (terrainLocalPos.x / terrainData.size.x) * terrainData.alphamapWidth;
        float mapZ = (terrainLocalPos.z / terrainData.size.z) * terrainData.alphamapHeight;

        int bWidth = Mathf.RoundToInt((boundsX / terrainData.size.x) * terrainData.alphamapWidth);
        int bHeight = Mathf.RoundToInt((boundsZ / terrainData.size.z) * terrainData.alphamapHeight);

        int startX = Mathf.Clamp(Mathf.FloorToInt(mapX - bWidth / 2f), 0, terrainData.alphamapWidth - 1);
        int startZ = Mathf.Clamp(Mathf.FloorToInt(mapZ - bHeight / 2f), 0, terrainData.alphamapHeight - 1);
        int width = Mathf.Min(bWidth, terrainData.alphamapWidth - startX);
        int height = Mathf.Min(bHeight, terrainData.alphamapHeight - startZ);

        float[,,] maps = terrainData.GetAlphamaps(startX, startZ, width, height);
        int totalLayers = terrainData.alphamapLayers;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                {
                    int globalX = startX + x;
                    int globalZ = startZ + y;

                    maps[y, x, layer] = initialAlphamaps[globalZ, globalX, layer];
                }
            }
        }

        terrainData.SetAlphamaps(startX, startZ, maps);
    }

    private void SaveInitialAlphamaps()
    {
        TerrainData data = terrain.terrainData;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;

        // 1. Pre-allocate the array — this alone typically doesn't crash
        initialAlphamaps = new float[h, w, layers];

        // 2. Read in 64-pixel-high strips (much more stable)
        int chunkSize = 64;

        for (int y = 0; y < h; y += chunkSize)
        {
            int currentChunkHeight = Mathf.Min(chunkSize, h - y);

            // Request only a small slice from the engine
            float[,,] chunk = data.GetAlphamaps(0, y, w, currentChunkHeight);

            // Copy into the main storage array
            for (int cy = 0; cy < currentChunkHeight; cy++)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int l = 0; l < layers; l++)
                    {
                        initialAlphamaps[y + cy, x, l] = chunk[cy, x, l];
                    }
                }
            }
        }
    }
}
