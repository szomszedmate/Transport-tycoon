using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Unity.CodeEditor;

public static class GenerateSolution
{
    public static void RegenerateProjectFiles()
    {
        AssetDatabase.Refresh();

        var editorField = CodeEditor.Editor
            .GetType()
            .GetField("m_ExternalCodeEditors",
                BindingFlags.NonPublic | BindingFlags.Instance);

        if (editorField == null)
        {
            Console.WriteLine("[GenerateSolution] ERROR: m_ExternalCodeEditors field not found.");
            EditorApplication.Exit(1);
            return;
        }

        var externalCodeEditors = editorField.GetValue(CodeEditor.Editor)
            as List<IExternalCodeEditor>;

        if (externalCodeEditors == null || externalCodeEditors.Count == 0)
        {
            Console.WriteLine("[GenerateSolution] ERROR: No external code editors registered.");
            EditorApplication.Exit(1);
            return;
        }

        foreach (var externalEditor in externalCodeEditors)
        {
            Console.WriteLine($"[GenerateSolution] Found editor: {externalEditor.GetType().FullName}");

            // Try SyncAll via interface/reflection — works for VS, Rider, VSCode packages
            var syncMethod = externalEditor.GetType()
                .GetMethod("SyncAll",
                    BindingFlags.Public | BindingFlags.Instance);

            if (syncMethod != null)
            {
                syncMethod.Invoke(externalEditor, null);
                Console.WriteLine($"[GenerateSolution] SyncAll called on {externalEditor.GetType().Name}");
                EditorApplication.Exit(0);
                return;
            }
        }

        Console.WriteLine("[GenerateSolution] ERROR: No editor with SyncAll found.");
        EditorApplication.Exit(1);
    }
}