using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateOvalSprite : EditorWindow
{
    private Terrain terrain;
    private int texSize = 256;
    private string savePath = "Assets/02_Materials/UI/minimap_oval_mask.png";

    [MenuItem("Tools/Generate Oval Minimap Mask")]
    static void Open() => GetWindow<GenerateOvalSprite>("Oval Mask Generator");

    void OnGUI()
    {
        terrain  = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        texSize  = EditorGUILayout.IntField("Textúra méret (px)", texSize);
        savePath = EditorGUILayout.TextField("Mentési útvonal", savePath);

        EditorGUILayout.HelpBox("A terrain holes adatából generálja az alakot — pontosan illeszkedik.", MessageType.Info);

        GUI.enabled = terrain != null;
        if (GUILayout.Button("Generate from Terrain & Save"))
            Generate();
        GUI.enabled = true;
    }

    void Generate()
    {
        TerrainData data = terrain.terrainData;
        int hRes = data.holesResolution;
        bool[,] holes = data.GetHoles(0, 0, hRes, hRes);

        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                int hx = Mathf.Clamp(Mathf.RoundToInt((float)x / texSize * hRes), 0, hRes - 1);
                int hy = Mathf.Clamp(Mathf.RoundToInt((float)y / texSize * hRes), 0, hRes - 1);
                float alpha = holes[hy, hx] ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        string dir = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.WriteAllBytes(savePath, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(savePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        AssetDatabase.ImportAsset(savePath);

        Debug.Log($"Minimap maszk sprite mentve: {savePath}");
    }
}
