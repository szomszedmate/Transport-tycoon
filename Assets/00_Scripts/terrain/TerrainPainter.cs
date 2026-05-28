using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainPainter : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    private float[,,] initialAlphamaps;
    private int lastLayer = 0;

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

    //public void PaintRoadAt(Vector3 snappedPos, RoadType type, float rotation, int layerIndex)
    //{
    //    if (terrain == null) terrain = Terrain.activeTerrain;
    //    TerrainData terrainData = terrain.terrainData;

    //    // 1. Konvertßlßs Terrain-helyi koordinßtßkra
    //    Vector3 terrainLocalPos = snappedPos - terrain.transform.position;

    //    switch(type)
    //    {
    //        case RoadType.STRAIGHT:
    //            rotation += 90;
    //            break;
    //        case RoadType.TURN:
    //            rotation = -rotation;
    //            break;
    //        default:
    //            break;
    //    }

    //    // 2. Kiszßmoljuk a "befoglalˇ nÚgyzetet" (Bounding Box)
    //    // Forgatßskor a terŘlet nagyobb lehet, mint az eredeti szÚlessÚg/hossz˙sßg
    //    float rad = rotation * Mathf.Deg2Rad;
    //    Debug.Log($"CROSS - rotation: {rotation}░ | rad: {rad} | roadW={roadWidth}, roadL={roadLength}");

    //    float cos = Mathf.Abs(Mathf.Cos(rad));
    //    float sin = Mathf.Abs(Mathf.Sin(rad));

    //    // Map koordinßtßk Ús mÚretek
    //    float mapX = (terrainLocalPos.x / terrainData.size.x) * terrainData.alphamapWidth;
    //    float mapZ = (terrainLocalPos.z / terrainData.size.z) * terrainData.alphamapHeight;

    //    float unitsPerPixelX = terrainData.size.x / terrainData.alphamapWidth;
    //    float unitsPerPixelZ = terrainData.size.z / terrainData.alphamapHeight;

    //    //float turnSize = (roadLength + roadWidth) / 2f;

    //    float boundsX, boundsZ;

    //    switch (type)
    //    {
    //        case RoadType.CROSS:
    //            // Keresztez§dÚsnÚl mindkÚt irßnyt figyelembe kell venni a befoglalˇ mÚretnÚl
    //            boundsX = Mathf.Max(roadWidth * cos + roadLength * sin, roadLength * cos + roadWidth * sin);
    //            boundsZ = Mathf.Max(roadWidth * sin + roadLength * cos, roadLength * sin + roadWidth * cos);
    //            break;

    //        case RoadType.STRAIGHT:
    //            boundsX = roadWidth * cos + roadLength * sin;
    //            boundsZ = roadWidth * sin + roadLength * cos;
    //            break;
    //        case RoadType.TURN:
    //            float maxDim = Mathf.Max(roadWidth, roadLength);
    //            boundsX = maxDim * (cos + sin);
    //            boundsZ = maxDim * (sin + cos);
    //            break;
    //        case RoadType.T: // A T-elßgazßs befoglalˇ mÚrete hasonlˇ a keresztez§dÚsÚhez
    //            boundsX = (roadWidth + roadLength) * (cos + sin) * 0.7f; // CROSS-nßl mindkÚt irßnyban kell
    //            boundsZ = (roadWidth + roadLength) * (sin + cos) * 0.7f;
    //            break;
    //        default:
    //            boundsX = roadWidth * cos + roadLength * sin;
    //            boundsZ = roadWidth * sin + roadLength * cos;
    //            break;
    //    }

    //    int bWidth = Mathf.RoundToInt((boundsX / terrainData.size.x) * terrainData.alphamapWidth) + 2;
    //    int bHeight = Mathf.RoundToInt((boundsZ / terrainData.size.z) * terrainData.alphamapHeight) + 2;
    //    Debug.Log($"Bounds -> X: {boundsX:F2} | Z: {boundsZ:F2} | bWidth={bWidth}, bHeight={bHeight}");

    //    int startX = Mathf.Clamp(Mathf.FloorToInt(mapX - bWidth / 2f), 0, terrainData.alphamapWidth - 1);
    //    int startZ = Mathf.Clamp(Mathf.FloorToInt(mapZ - bHeight / 2f), 0, terrainData.alphamapHeight - 1);
    //    int width = Mathf.Min(bWidth, terrainData.alphamapWidth - startX);
    //    int height = Mathf.Min(bHeight, terrainData.alphamapHeight - startZ);

    //    float[,,] maps = terrainData.GetAlphamaps(startX, startZ, width, height);

    //    // El§re kiszßmolt forgatßsi ÚrtÚkek
    //    float cosR = Mathf.Cos(-rad); // Inverz forgatßs, hogy a vilßgpontot forgassuk vissza a helyi tÚrbe
    //    float sinR = Mathf.Sin(-rad);
    //    //Debug.Log($"rotation={rotation} rad={rad} cos={cos} sin={sin} boundsX={boundsX} boundsZ={boundsZ} bWidth={bWidth} bHeight={bHeight}");
    //    //float testSize = Mathf.Max(roadWidth, roadLength) * 2f;
    //    //boundsX = testSize;
    //    //boundsZ = testSize;

    //    for (int y = 0; y < height; y++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            float pixelDistX = (startX + x) - mapX; 
    //            float pixelDistZ = (startZ + y) - mapZ;

    //            // A pixel aktußlis vilßgkoordinßtßja (relatÝvan a snappedPos-hoz)
    //            float currentRelX = pixelDistX * unitsPerPixelX;
    //            float currentRelZ = pixelDistZ * unitsPerPixelZ;

    //            // Visszaforgatjuk a pontot a "0 fokos" ßllapotba
    //            float rotatedX = currentRelX * cosR - currentRelZ * sinR;
    //            float rotatedZ = currentRelX * sinR + currentRelZ * cosR;

    //            bool shouldIPaint = false;
    //            float w = roadWidth / 2f;
    //            float l = roadLength / 2f;

    //            switch (type)
    //            {
    //                case RoadType.CROSS:
    //                    bool horizontal = Mathf.Abs(rotatedX) <= w && Mathf.Abs(rotatedZ) <= l;
    //                    bool vertical = Mathf.Abs(rotatedX) <= l && Mathf.Abs(rotatedZ) <= w;
    //                    shouldIPaint = horizontal || vertical;
    //                    Debug.Log($"Painting at mapX={mapX:F1}, mapZ={mapZ:F1}, start=({startX},{startZ}) size=({width},{height})");
    //                    break;

    //                case RoadType.STRAIGHT:
    //                    shouldIPaint = Mathf.Abs(rotatedX) <= w && Mathf.Abs(rotatedZ) <= l;
    //                    break;
    //                case RoadType.TURN:
    //                    bool stem1 = Mathf.Abs(rotatedX) <= w && rotatedZ <= w && rotatedZ >= - l;
    //                    bool stem2 = rotatedX <= w && rotatedX >= -l && Mathf.Abs(rotatedZ) <= w;

    //                    shouldIPaint = stem1 || stem2;
    //                    break;
    //                case RoadType.T:
    //                    bool mainRoad = Mathf.Abs(rotatedZ) <= w && Mathf.Abs(rotatedX) <= l;
    //                    bool sideBranch = Mathf.Abs(rotatedX) <= w && rotatedZ <= 0 && rotatedZ >= -l;

    //                    shouldIPaint = mainRoad || sideBranch;
    //                    break;
    //                default:
    //                    shouldIPaint = Mathf.Abs(rotatedX) <= w && Mathf.Abs(rotatedZ) <= l;
    //                    break;
    //            }

    //            if (shouldIPaint)
    //            {
    //                for (int i = 0; i < terrainData.alphamapLayers; i++)
    //                {
    //                    maps[y, x, i] = 0;
    //                }
    //                maps[y, x, layerIndex] = 1f;
    //            }
    //        }
    //    }

    //    terrainData.SetAlphamaps(startX, startZ, maps);
    //}


    public void PaintRoadAt(Vector3 snappedPos, RoadType type, float rotation, int layerIndex, List<int> mainLayers, bool isBasicPreview, int lastLayer)
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;
        //Debug.Log("Painting...");
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

        // 2. Map egysÚgek kiszßmÝtßsa (Mennyi vilßg-egysÚg egy pixel?)
        float unitsPerPixelX = terrainData.size.x / (terrainData.alphamapWidth - 1);
        float unitsPerPixelZ = terrainData.size.z / (terrainData.alphamapHeight - 1);

        // Pixel koordinßta (lebeg§pontosan a pontos k÷zÚpponthoz)
        float mapX = (terrainLocalPos.x / terrainData.size.x) * (terrainData.alphamapWidth - 1);
        float mapZ = (terrainLocalPos.z / terrainData.size.z) * (terrainData.alphamapHeight - 1);

        // 3. Befoglalˇ keret (Bounds) meghatßrozßsa
        // Biztonsßgi rßhagyßs (+2), hogy a szÚleken ne vßgja le a forgatott alakzatot
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

        //debug
        List<int> activeLayers = new List<int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 4. A pixel AKTU┴LIS vilßgkoordinßtßjßnak kiszßmÝtßsa a k÷zepe alapjßn (+0.5f)
                // Itt vonjuk le a snappedPos-t, hogy relatÝv koordinßtßt kapjunk
                float currentWorldX = (startX + x) * unitsPerPixelX;
                float currentWorldZ = (startZ + y) * unitsPerPixelZ;

                float relX = currentWorldX - terrainLocalPos.x;
                float relZ = currentWorldZ - terrainLocalPos.z;

                // 5. Forgatßs vissza a helyi (0 fokos) tÚrbe
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
                            //debug
                            if (maps[y, x, i] != 0 && !activeLayers.Contains(i)) {
                                activeLayers.Add(i);
                            }
                        }
                    } else
                    {
                        maps[y, x, lastLayer] = 0;
                        //debug
                        if (maps[y, x, lastLayer] != 0 && !activeLayers.Contains(lastLayer))
                        {
                            activeLayers.Add(lastLayer);
                        }
                    }
                    maps[y, x, layerIndex] = 1f;
                    //Debug.Log("Changed " + lastLayer + " into " + layerIndex);
                    if (layerIndex == 4)
                    {
                        for (int i = 0; i < terrainData.alphamapLayers; i++) 
                        {
                            //debug
                            if (maps[y, x, i] != 0 && !activeLayers.Contains(i))
                            {
                                activeLayers.Add(i);
                            }

                            if (maps[y, x, i] != 0 &&i != 4)
                            {
                                //Debug.Log("another active layer:" + i);

                            }
                        }
                    }
                }
            }
        }
        //Debug.Log("Aktív rétegek: " + string.Join(", ", activeLayers.ConvertAll(i => i.ToString()).ToArray()));
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

    //private void SaveInitialAlphamaps()
    //{
    //    TerrainData data = terrain.terrainData;
    //    initialAlphamaps = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
    //}
    private void SaveInitialAlphamaps()
    {
        TerrainData data = terrain.terrainData;
        int w = data.alphamapWidth;
        int h = data.alphamapHeight;
        int layers = data.alphamapLayers;

        // 1. Előre lefoglaljuk a helyet, ez általában még nem crash
        initialAlphamaps = new float[h, w, layers];

        // 2. 64 pixel magas sávokban olvassuk be (ez sokkal stabilabb)
        int chunkSize = 64;

        for (int y = 0; y < h; y += chunkSize)
        {
            int currentChunkHeight = Mathf.Min(chunkSize, h - y);

            // Csak egy kisebb szeletet kérünk le a motortól
            float[,,] chunk = data.GetAlphamaps(0, y, w, currentChunkHeight);

            // Átmásoljuk a nagy tárolóba
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
