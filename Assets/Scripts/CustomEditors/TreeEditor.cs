using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tree))]
public class TreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Tree tree = (Tree)target;
        if (GUILayout.Button("Regenerate Tree"))
        {
            tree.TreeRegen();
        }
        DrawDefaultInspector();
    }
}