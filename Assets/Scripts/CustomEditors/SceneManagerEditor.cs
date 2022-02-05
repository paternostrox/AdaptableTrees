using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

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

        if (!buildTrees)
        {
            ActiveEditorTracker.sharedTracker.isLocked = false;
            buildTrees = GUILayout.Toggle(buildTrees, "Build Trees (Click to Start)", "Button");
        }
        else
        {
            ActiveEditorTracker.sharedTracker.isLocked = true;
            buildTrees = GUILayout.Toggle(buildTrees, "Build Trees (Click to Stop)", "Button");
        }

        DrawDefaultInspector();
        if (sceneManager.useCustomVolume)
        {
            sceneManager.treeGenVolume = (GameObject)EditorGUILayout.ObjectField("Generation Volume", sceneManager.treeGenVolume, typeof(GameObject), true);
            LayerMask tempMask = EditorGUILayout.MaskField("Volume Mask",InternalEditorUtility.LayerMaskToConcatenatedLayersMask(sceneManager.treeGenVolumeMask), InternalEditorUtility.layers);
            sceneManager.treeGenVolumeMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        }
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