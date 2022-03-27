using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdaptableTree))]
public class AdaptableTreeEditor : Editor
{
    bool foldout = true;

    public override void OnInspectorGUI()
    {
        AdaptableTree tree = (AdaptableTree)target;
        if (GUILayout.Button("Rebuild Tree"))
        {
            tree.TreeRegen();
        }
        DrawDefaultInspector();

        foldout = EditorGUILayout.Foldout(foldout, "Point Cloud Data");

        if (foldout)
        {
            var level = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            tree.pointCloudData.cloudShape = (PointCloudShape)EditorGUILayout.EnumPopup("Cloud Shape", tree.pointCloudData.cloudShape);

            if (tree.pointCloudData.cloudShape == PointCloudShape.Box)
                tree.pointCloudData.boxSize = EditorGUILayout.Vector3Field("Box Size", tree.pointCloudData.boxSize);
            if (tree.pointCloudData.cloudShape == PointCloudShape.Sphere || tree.pointCloudData.cloudShape == PointCloudShape.HalfSphere)
                tree.pointCloudData.sphereRadius = EditorGUILayout.FloatField("Sphere Radius", tree.pointCloudData.sphereRadius);
            if (tree.pointCloudData.cloudShape == PointCloudShape.Ellipsoid || tree.pointCloudData.cloudShape == PointCloudShape.HalfEllipsoid)
            {
                tree.pointCloudData.ellipsoidSize = EditorGUILayout.Vector3Field("Ellipsoid Size", tree.pointCloudData.ellipsoidSize);
                //GUILayout.Label("Ellipsoid Parameters");
                //sceneManager.pointCloudData.ellipsoidParams.a = EditorGUILayout.FloatField("A", sceneManager.pointCloudData.ellipsoidParams.a);
                //sceneManager.pointCloudData.ellipsoidParams.b = EditorGUILayout.FloatField("B", sceneManager.pointCloudData.ellipsoidParams.b);
                //sceneManager.pointCloudData.ellipsoidParams.c = EditorGUILayout.FloatField("C", sceneManager.pointCloudData.ellipsoidParams.c);
            }
            EditorGUI.indentLevel = level;
        }
    }

}