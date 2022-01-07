using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestCustom))]
public class TestCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TestCustom testCustom = (TestCustom)target;
        EditorGUILayout.HelpBox("This is a tip", MessageType.Info);
        if(GUILayout.Button("Press this"))
        {
            testCustom.PrintMessage();
        }
        testCustom.isTrue = GUILayout.Toggle(testCustom.isTrue, "Toggle");
    }
}