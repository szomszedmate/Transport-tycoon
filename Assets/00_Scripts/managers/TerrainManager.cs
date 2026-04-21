using UnityEngine;

public class TerrainShaper : MonoBehaviour
{
    public Terrain terrain;
    public float roadWidth;
    private float[,] originalHeights;

    void Awake()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        TerrainData td = terrain.terrainData; // alapállapot mentése
        originalHeights = td.GetHeights(0, 0, td.heightmapResolution, td.heightmapResolution);
        terrain.terrainData = Instantiate(td);
    }

    void OnApplicationQuit()
    {
        if (originalHeights != null) // kilépéskor reset
        {
            terrain.terrainData.SetHeights(0, 0, originalHeights);
        }
    }

    public void FlattenTerrainUnderRoad(Vector3 roadPosition)
    {
        TerrainData td = terrain.terrainData;

        // 1. Átszámítás koordinátákra (ez marad)
        Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(roadPosition);
        int mapX = (int)((terrainLocalPos.x / td.size.x) * td.heightmapResolution);
        int mapZ = (int)((terrainLocalPos.z / td.size.z) * td.heightmapResolution);

        // 2. ECSET MÉRETE (legyen elég nagy a 10-es úthoz)
        float unitsToPixels = (float)terrain.terrainData.heightmapResolution / terrain.terrainData.size.x;
        int brushSize = Mathf.CeilToInt(roadWidth * unitsToPixels) + 2;

        // 3. MAGASSÁG FIXÁLÁSA
        // roadPosition.y a világban vett magasság. Ezt alakítjuk 0-1 tartományra.
        float worldHeight = roadPosition.y - terrain.transform.position.y;
        float targetHeight = worldHeight / td.size.y;

        // Biztonsági korlát: ne menjen 0 alá
        targetHeight = Mathf.Max(0, targetHeight);

        float[,] heights = new float[brushSize, brushSize];
        for (int i = 0; i < brushSize; i++)
        {
            for (int j = 0; j < brushSize; j++)
            {
                heights[i, j] = targetHeight;
            }
        }

        int startX = Mathf.Clamp(mapX - (brushSize / 2), 0, td.heightmapResolution - brushSize);
        int startZ = Mathf.Clamp(mapZ - (brushSize / 2), 0, td.heightmapResolution - brushSize);

        td.SetHeights(startX, startZ, heights);
    }
}