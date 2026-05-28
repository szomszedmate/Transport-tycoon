using UnityEngine;
using UnityEditor;

// Run via menu: Tools > Fix TMP CanvasRenderer in Prefabs
// Fixes prefabs where TextMeshProUGUI.m_canvasRenderer is null after upgrading
// from the old TMP package to the UGUI-integrated TMP (com.unity.ugui).
public static class FixTMPCanvasRenderer
{
    [MenuItem("Tools/Fix TMP CanvasRenderer in Prefabs")]
    public static void Fix()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool changed = false;
            foreach (var mono in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mono == null) continue;
                if (mono.GetType().Name != "TextMeshProUGUI") continue;

                CanvasRenderer cr = mono.GetComponent<CanvasRenderer>();
                if (cr == null)
                {
                    Undo.AddComponent<CanvasRenderer>(mono.gameObject);
                    cr = mono.GetComponent<CanvasRenderer>();
                }

                SerializedObject so = new SerializedObject(mono);
                SerializedProperty prop = so.FindProperty("m_canvasRenderer");
                if (prop != null && prop.objectReferenceValue == null)
                {
                    prop.objectReferenceValue = cr;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                fixedCount++;
                Debug.Log($"Fixed: {path}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Done. Fixed {fixedCount} prefab(s).");
    }
}
