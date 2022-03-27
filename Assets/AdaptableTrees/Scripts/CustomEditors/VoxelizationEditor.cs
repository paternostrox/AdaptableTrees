using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Voxelization))]
public class VoxelizationEditor : Editor
{
    private bool buildTrees = false;

    bool foldout = true;

    public override void OnInspectorGUI()
    {
        //GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        //labelStyle.alignment = TextAnchor.MiddleCenter;

        Voxelization voxelization = (Voxelization)target;

        // AMBIENT

        GUILayout.Label("Voxelization Settings");

        voxelization.size = EditorGUILayout.Vector3IntField("Size", voxelization.size);

        voxelization.unitSize = EditorGUILayout.FloatField("Unit Size", voxelization.unitSize);

        voxelization.floodStartPos = EditorGUILayout.Vector3Field("Flood Start Pos", voxelization.floodStartPos);

        voxelization.showOccupied = EditorGUILayout.Toggle("Show Occupied Voxels", voxelization.showOccupied);

        voxelization.showFree = EditorGUILayout.Toggle("Show Free Voxels", voxelization.showFree);

        if (GUILayout.Button("Rebuild Voxelization"))
        {
            voxelization.ProcessLevel();
        }

        // TREE
        GUILayout.Label("Tree Settings");


        voxelization.abortCollidingBranches = EditorGUILayout.Toggle("Abort Colliding Branches", voxelization.abortCollidingBranches);

        voxelization.animateGrowth = EditorGUILayout.Toggle("Animate Growth", voxelization.animateGrowth);

        voxelization.treeHeight = EditorGUILayout.FloatField("Trunk Height", voxelization.treeHeight);

        voxelization.attractorsAmount = EditorGUILayout.IntField("Attractors Amount", voxelization.attractorsAmount);

        voxelization.nodeAttractionDistance = EditorGUILayout.FloatField("Node Attraction Distance", voxelization.nodeAttractionDistance);

        voxelization.nodeKillDistance = EditorGUILayout.FloatField("Node Kill Distance", voxelization.nodeKillDistance);

        voxelization.nodeSegmentLength = EditorGUILayout.FloatField("Node Segment Length", voxelization.nodeSegmentLength);

        voxelization.treeBaseThickness = EditorGUILayout.FloatField("Tree Base Thickness", voxelization.treeBaseThickness);

        voxelization.treePerChildThickness = EditorGUILayout.FloatField("Tree Per Child Thickness", voxelization.treePerChildThickness);

        voxelization.treeMaxDiffThickness = EditorGUILayout.FloatField("Tree Max Diff Thickness", voxelization.treeMaxDiffThickness);

        voxelization.treeMaterial = (Material)EditorGUILayout.ObjectField("Material", voxelization.treeMaterial, typeof(Material));

        voxelization.treeTubeVertexAmount = EditorGUILayout.IntField("Tube Vertex Amount", voxelization.treeTubeVertexAmount);

        foldout = EditorGUILayout.Foldout(foldout, "Point Cloud Data");

        if(foldout)
        {
            var level = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            voxelization.pointCloudData.cloudShape = (PointCloudShape) EditorGUILayout.EnumPopup("Cloud Shape", voxelization.pointCloudData.cloudShape);

            if (voxelization.pointCloudData.cloudShape == PointCloudShape.Box)
                voxelization.pointCloudData.boxSize = EditorGUILayout.Vector3Field("Box Size", voxelization.pointCloudData.boxSize);
            if (voxelization.pointCloudData.cloudShape == PointCloudShape.Sphere || voxelization.pointCloudData.cloudShape == PointCloudShape.HalfSphere)
                voxelization.pointCloudData.sphereRadius = EditorGUILayout.FloatField("Sphere Radius", voxelization.pointCloudData.sphereRadius);
            if (voxelization.pointCloudData.cloudShape == PointCloudShape.Ellipsoid || voxelization.pointCloudData.cloudShape == PointCloudShape.HalfEllipsoid)
            {
                voxelization.pointCloudData.ellipsoidSize = EditorGUILayout.Vector3Field("Ellipsoid Size", voxelization.pointCloudData.ellipsoidSize);
                //GUILayout.Label("Ellipsoid Parameters");
                //sceneManager.pointCloudData.ellipsoidParams.a = EditorGUILayout.FloatField("A", sceneManager.pointCloudData.ellipsoidParams.a);
                //sceneManager.pointCloudData.ellipsoidParams.b = EditorGUILayout.FloatField("B", sceneManager.pointCloudData.ellipsoidParams.b);
                //sceneManager.pointCloudData.ellipsoidParams.c = EditorGUILayout.FloatField("C", sceneManager.pointCloudData.ellipsoidParams.c);
            }
            EditorGUI.indentLevel = level;
        }


        //DrawDefaultInspector();

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

        if (voxelization.useCustomVolume)
        {
            voxelization.treeGenVolume = (GameObject)EditorGUILayout.ObjectField("Generation Volume", voxelization.treeGenVolume, typeof(GameObject), true);
            LayerMask tempMask = EditorGUILayout.MaskField("Volume Mask",InternalEditorUtility.LayerMaskToConcatenatedLayersMask(voxelization.treeGenVolumeMask), InternalEditorUtility.layers);
            voxelization.treeGenVolumeMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        }
    }

    private void OnSceneGUI()
    {
        if(buildTrees)
        {
            Voxelization voxelization = (Voxelization)target;
            voxelization.MouseInteraction();
        }
    }
}