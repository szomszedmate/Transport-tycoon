using UnityEngine;
using UnityEditor;

public class TerrainOvalMask : EditorWindow
{
    private Terrain terrain;
    [Range(0.1f, 1f)] private float radiusX = 0.8f;
    [Range(0.1f, 1f)] private float radiusZ = 0.8f;
    [Range(0f, 0.5f)] private float edgeSoftness = 0.15f;

    private float[,] originalHeights;

    [MenuItem("Tools/Apply Oval Terrain Mask")]
    static void Open() => GetWindow<TerrainOvalMask>("Oval Terrain Mask");

    void OnGUI()
    {
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);

        EditorGUILayout.Space();
        radiusX = EditorGUILayout.Slider("Szélesség (X)", radiusX, 0.1f, 1f);
        radiusZ = EditorGUILayout.Slider("Mélység (Z)", radiusZ, 0.1f, 1f);
        edgeSoftness = EditorGUILayout.Slider("Él lágyság (lejtő szélessége)", edgeSoftness, 0f, 0.5f);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Először Backup, aztán Apply. Reset visszaállítja az eredetit.", MessageType.Info);
        EditorGUILayout.Space();

        GUI.enabled = terrain != null;
        if (GUILayout.Button("1. Backup (mentés előtte)"))
            Backup();

        GUI.enabled = terrain != null;
        if (GUILayout.Button("2. Apply Oval"))
            Apply();

        GUI.enabled = terrain != null && originalHeights != null;
        if (GUILayout.Button("Reset (visszaállítás)"))
            ResetAll();

        GUI.enabled = true;
    }

    void Backup()
    {
        TerrainData data = terrain.terrainData;
        int res = data.heightmapResolution;
        originalHeights = data.GetHeights(0, 0, res, res);
        Debug.Log("Backup kész.");
    }

    void Apply()
    {
        if (originalHeights == null) Backup();

        TerrainData data = terrain.terrainData;
        int hRes = data.heightmapResolution;
        int holesRes = data.holesResolution;

        // Heights: smooth slope at the oval edge
        float[,] heights = new float[hRes, hRes];
        for (int z = 0; z < hRes; z++)
        {
            for (int x = 0; x < hRes; x++)
            {
                float nx = (float)x / (hRes - 1) * 2f - 1f;
                float nz = (float)z / (hRes - 1) * 2f - 1f;
                float dist = Mathf.Sqrt(nx * nx / (radiusX * radiusX) + nz * nz / (radiusZ * radiusZ));
                float mult = 1f - Mathf.Clamp01(Mathf.InverseLerp(1f - edgeSoftness, 1f, dist));
                heights[z, x] = originalHeights[z, x] * mult;
            }
        }
        data.SetHeights(0, 0, heights);

        // Holes: only outside the oval
        bool[,] holes = new bool[holesRes, holesRes];
        for (int z = 0; z < holesRes; z++)
        {
            for (int x = 0; x < holesRes; x++)
            {
                float nx = (float)x / (holesRes - 1) * 2f - 1f;
                float nz = (float)z / (holesRes - 1) * 2f - 1f;
                float dist = Mathf.Sqrt(nx * nx / (radiusX * radiusX) + nz * nz / (radiusZ * radiusZ));
                holes[z, x] = dist < 1f;
            }
        }
        data.SetHoles(0, 0, holes);

        Debug.Log("Oval alkalmazva.");
    }

    void ResetAll()
    {
        TerrainData data = terrain.terrainData;

        if (originalHeights != null)
            data.SetHeights(0, 0, originalHeights);

        int holesRes = data.holesResolution;
        bool[,] holes = new bool[holesRes, holesRes];
        for (int z = 0; z < holesRes; z++)
            for (int x = 0; x < holesRes; x++)
                holes[z, x] = true;
        data.SetHoles(0, 0, holes);

        Debug.Log("Reset kész.");
    }
}
