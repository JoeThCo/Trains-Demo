using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphGenerator))]
public class GraphGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GraphGenerator graphGenerator = target as GraphGenerator;
        if (GUILayout.Button("Split Splines"))
        {
            graphGenerator.SplitSplines();
        }

        if (GUILayout.Button("Delete Debug"))
        {
            graphGenerator.DeleteDebug();
        }
    }
}