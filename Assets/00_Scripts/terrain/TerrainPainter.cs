using UnityEngine;

public class TerrainPainter : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private int roadTextureIndex = 4;

    [Header("Road Settings")]
    [SerializeField] private float roadWidth = 4f;      // Width in world units (meters)
    [SerializeField] private float roadLength = 10f;    // Length in world units (meters) - for straight roads

    //[SerializeField] private float edgeSoftness = 0.0f; // 0 = sharp edges, 0.5 = soft edges

    public Terrain Terrain { get => terrain; private set => terrain = value; }

    public void PaintRoadAt(Vector3 worldPos, RoadType type, float rotation)
    {
        TerrainData td = Terrain.terrainData;

        float relX = (worldPos.x - Terrain.transform.position.x) / td.size.x;
        float relZ = (worldPos.z - Terrain.transform.position.z) / td.size.z;

        int centerX = Mathf.RoundToInt(relX * td.alphamapWidth);
        int centerZ = Mathf.RoundToInt(relZ * td.alphamapHeight);

        // === NEW: Better width & length calculation ===
        int widthInPixels = Mathf.CeilToInt((roadWidth / td.size.x) * td.alphamapWidth);
        int lengthInPixels = Mathf.CeilToInt((roadLength / td.size.z) * td.alphamapHeight);

        int halfWidth = widthInPixels / 2;
        int halfLength = lengthInPixels / 2;

        // Expand the area a tiny bit to ensure overlap and clean connections
        int expand = 2;   // This helps prevent gaps

        int startX = Mathf.Clamp(centerX - halfWidth - expand, 0, td.alphamapWidth - 1);
        int startZ = Mathf.Clamp(centerZ - halfLength - expand, 0, td.alphamapHeight - 1);
        int sizeX = Mathf.Clamp(centerX + halfWidth + expand, 0, td.alphamapWidth) - startX;
        int sizeZ = Mathf.Clamp(centerZ + halfLength + expand, 0, td.alphamapHeight) - startZ;

        float[,,] alphas = td.GetAlphamaps(startX, startZ, sizeX, sizeZ);
        int layerCount = td.alphamapLayers;

        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                float nx = (x / (float)sizeX) * 2f - 1f;
                float nz = (z / (float)sizeZ) * 2f - 1f;

                Vector2 p = Rotate(new Vector2(nx, nz), -rotation);

                bool isRoad = IsRoadPixel(p.x, p.y, type);

                if (isRoad)
                {
                    // Force this pixel to be ONLY the road layer
                    for (int l = 0; l < layerCount; l++)
                        alphas[z, x, l] = 0f;

                    alphas[z, x, roadTextureIndex] = 1f;
                }
            }
        }

        td.SetAlphamaps(startX, startZ, alphas);
    }

    private bool IsRoadPixel(float px, float pz, RoadType type)
    {
        float roadW = 0.5f;   // Half-width in normalized space

        switch (type)
        {
            case RoadType.STRAIGHT:
                return Mathf.Abs(px) < roadW;           // Horizontal road (after rotation)

            case RoadType.TURN:
            case RoadType.T:
            case RoadType.CROSS:
                // You can expand these later
                return Mathf.Abs(px) < roadW || Mathf.Abs(pz) < roadW;

            default:
                return true;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}