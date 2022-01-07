using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneManager))]
public class SceneManagerEditor : Editor
{
    private bool buildTrees = false;

    public override void OnInspectorGUI()
    {
        SceneManager sceneManager = (SceneManager)target;
        if (GUILayout.Button("Rebuild Discretization"))
        {
            sceneManager.ProcessLevel();
        }
        buildTrees = GUILayout.Toggle(buildTrees, "Build Trees");
        DrawDefaultInspector();
    }

    private void OnSceneGUI()
    {
        if(buildTrees)
        {
            SceneManager sceneManager = (SceneManager)target;
            sceneManager.MouseInteraction();
        }
    }
}