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

        GUILayout.Space(10);

        GraphGenerator graphGenerator = target as GraphGenerator;
        if (GUILayout.Button("Create Graph"))
        {
            graphGenerator.CreateGraph();
        }

        if (GUILayout.Button("Reset"))
        {
            graphGenerator.ResetGraph();
        }
    }
}