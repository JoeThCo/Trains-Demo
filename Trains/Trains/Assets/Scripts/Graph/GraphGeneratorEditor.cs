using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(GraphGenerator))]
public class GraphGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        GraphGenerator graphGenerator = target as GraphGenerator;
        if (GUILayout.Button("Create Graph"))
            graphGenerator.CreateGraph();

        if (GUILayout.Button("Reset"))
            graphGenerator.ResetGraph();

        GUILayout.Space(10);
        if (GUILayout.Button("Round Position"))
            graphGenerator.RoundBezierKnots();

        if (GUILayout.Button("Zero Y"))
            graphGenerator.ZeroYBezierKnots();
    }
}
#endif