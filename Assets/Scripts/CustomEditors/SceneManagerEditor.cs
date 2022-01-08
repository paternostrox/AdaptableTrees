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
        buildTrees = GUILayout.Toggle(buildTrees, "Build Trees");
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