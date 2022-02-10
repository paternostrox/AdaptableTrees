using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdaptableTree))]
public class TreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AdaptableTree tree = (AdaptableTree)target;
        if (GUILayout.Button("Regenerate Tree"))
        {
            tree.TreeRegen();
        }
        DrawDefaultInspector();
    }
}