using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Voxelization))]
public class VoxelizationEditor : Editor
{
    private bool buildTrees = false;

    public override void OnInspectorGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        //labelStyle.alignment = TextAnchor.MiddleCenter;

        Voxelization sceneManager = (Voxelization)target;

        // AMBIENT

        GUILayout.Label("Voxelization Settings", labelStyle);

        sceneManager.size = EditorGUILayout.Vector3IntField("Size", sceneManager.size);

        sceneManager.unitSize = EditorGUILayout.FloatField("Unit Size", sceneManager.unitSize);

        sceneManager.floodStartPos = EditorGUILayout.Vector3Field("Flood Start Pos", sceneManager.floodStartPos);

        sceneManager.showOccupied = EditorGUILayout.Toggle("Show Occupied Voxels", sceneManager.showOccupied);

        sceneManager.showFree = EditorGUILayout.Toggle("Show Free Voxels", sceneManager.showFree);

        if (GUILayout.Button("Rebuild Voxelization"))
        {
            sceneManager.ProcessLevel();
        }

        // TREE
        GUILayout.Label("Tree Settings", labelStyle);

        sceneManager.treeTubeVertexAmount = EditorGUILayout.IntField("Tube Vertex Amount", sceneManager.treeTubeVertexAmount);

        sceneManager.abortCollidingBranches = EditorGUILayout.Toggle("Abort Colliding Branches", sceneManager.abortCollidingBranches);

        sceneManager.attractorsAmount = EditorGUILayout.IntField("Attractors Amount", sceneManager.attractorsAmount);

        sceneManager.nodeAttractionDistance = EditorGUILayout.FloatField("Node Attraction Distance", sceneManager.nodeAttractionDistance);

        sceneManager.nodeKillDistance = EditorGUILayout.FloatField("Node Kill Distance", sceneManager.nodeKillDistance);

        sceneManager.nodeSegmentLength = EditorGUILayout.FloatField("Node Segment Length", sceneManager.nodeSegmentLength);

        sceneManager.treeBaseThickness = EditorGUILayout.FloatField("Tree Base Thickness", sceneManager.treeBaseThickness);

        sceneManager.treeStepThickness = EditorGUILayout.FloatField("Tree Step Thickness", sceneManager.treeStepThickness);

        sceneManager.treeMaxDiffThickness = EditorGUILayout.FloatField("Tree Max Diff Thickness", sceneManager.treeMaxDiffThickness);

        sceneManager.treeMaterial = (Material)EditorGUILayout.ObjectField("Material", sceneManager.treeMaterial, typeof(Material));

        DrawDefaultInspector();

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
            Voxelization sceneManager = (Voxelization)target;
            sceneManager.MouseInteraction();
        }
    }
}