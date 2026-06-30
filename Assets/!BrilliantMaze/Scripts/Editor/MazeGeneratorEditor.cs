using UnityEditor;
using UnityEngine;

public class MazeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MazeGenerator generator = (MazeGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Управление", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        if (GUILayout.Button("Generate Maze", GUILayout.Height(32)))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Maze");
            generator.Generate();
            EditorUtility.SetDirty(generator);
        }

        GUI.backgroundColor = new Color(0.9f, 0.6f, 0.6f);
        if (GUILayout.Button("Clear Maze", GUILayout.Height(32)))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Maze");
            generator.ClearMaze();
            EditorUtility.SetDirty(generator);
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }
}
