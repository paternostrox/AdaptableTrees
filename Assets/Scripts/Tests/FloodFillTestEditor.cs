using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(FloodFillTest))]
public class FloodFillTestEditor : Editor
{
    private bool buildTrees = false;

    bool foldout = true;

    public override void OnInspectorGUI()
    {
        //GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        //labelStyle.alignment = TextAnchor.MiddleCenter;

        FloodFillTest floodFillTest = (FloodFillTest)target;

        // AMBIENT

        GUILayout.Label("Voxelization Settings");

        floodFillTest.size = EditorGUILayout.Vector3IntField("Size", floodFillTest.size);

        floodFillTest.unitSize = EditorGUILayout.FloatField("Unit Size", floodFillTest.unitSize);

        floodFillTest.floodStartPos = EditorGUILayout.Vector3Field("Flood Start Pos", floodFillTest.floodStartPos);

        floodFillTest.showOccupied = EditorGUILayout.Toggle("Show Occupied Voxels", floodFillTest.showOccupied);

        floodFillTest.showFree = EditorGUILayout.Toggle("Show Free Voxels", floodFillTest.showFree);

        if (GUILayout.Button("Rebuild Voxelization"))
        {
            floodFillTest.ProcessLevel();
        }

        // TREE
        GUILayout.Label("Tree Settings");


        floodFillTest.abortCollidingBranches = EditorGUILayout.Toggle("Abort Colliding Branches", floodFillTest.abortCollidingBranches);

        floodFillTest.treeHeight = EditorGUILayout.FloatField("Trunk Height", floodFillTest.treeHeight);

        floodFillTest.attractorsAmount = EditorGUILayout.IntField("Attractors Amount", floodFillTest.attractorsAmount);

        floodFillTest.nodeAttractionDistance = EditorGUILayout.FloatField("Node Attraction Distance", floodFillTest.nodeAttractionDistance);

        floodFillTest.nodeKillDistance = EditorGUILayout.FloatField("Node Kill Distance", floodFillTest.nodeKillDistance);

        floodFillTest.nodeSegmentLength = EditorGUILayout.FloatField("Node Segment Length", floodFillTest.nodeSegmentLength);

        floodFillTest.treeBaseThickness = EditorGUILayout.FloatField("Tree Base Thickness", floodFillTest.treeBaseThickness);

        floodFillTest.treePerChildThickness = EditorGUILayout.FloatField("Tree Per Child Thickness", floodFillTest.treePerChildThickness);

        floodFillTest.treeMaxDiffThickness = EditorGUILayout.FloatField("Tree Max Diff Thickness", floodFillTest.treeMaxDiffThickness);

        floodFillTest.treeMaterial = (Material)EditorGUILayout.ObjectField("Material", floodFillTest.treeMaterial, typeof(Material));

        floodFillTest.treeTubeVertexAmount = EditorGUILayout.IntField("Tube Vertex Amount", floodFillTest.treeTubeVertexAmount);

        foldout = EditorGUILayout.Foldout(foldout, "Point Cloud Data");

        if(foldout)
        {
            var level = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            floodFillTest.pointCloudData.cloudShape = (PointCloudShape) EditorGUILayout.EnumPopup("Cloud Shape", floodFillTest.pointCloudData.cloudShape);

            if (floodFillTest.pointCloudData.cloudShape == PointCloudShape.Box)
                floodFillTest.pointCloudData.boxSize = EditorGUILayout.Vector3Field("Box Size", floodFillTest.pointCloudData.boxSize);
            if (floodFillTest.pointCloudData.cloudShape == PointCloudShape.Sphere || floodFillTest.pointCloudData.cloudShape == PointCloudShape.HalfSphere)
                floodFillTest.pointCloudData.sphereRadius = EditorGUILayout.FloatField("Sphere Radius", floodFillTest.pointCloudData.sphereRadius);
            if (floodFillTest.pointCloudData.cloudShape == PointCloudShape.Ellipsoid || floodFillTest.pointCloudData.cloudShape == PointCloudShape.HalfEllipsoid)
            {
                floodFillTest.pointCloudData.ellipsoidSize = EditorGUILayout.Vector3Field("Ellipsoid Size", floodFillTest.pointCloudData.ellipsoidSize);
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

        if (floodFillTest.useCustomVolume)
        {
            floodFillTest.treeGenVolume = (GameObject)EditorGUILayout.ObjectField("Generation Volume", floodFillTest.treeGenVolume, typeof(GameObject), true);
            LayerMask tempMask = EditorGUILayout.MaskField("Volume Mask",InternalEditorUtility.LayerMaskToConcatenatedLayersMask(floodFillTest.treeGenVolumeMask), InternalEditorUtility.layers);
            floodFillTest.treeGenVolumeMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        }
    }
}