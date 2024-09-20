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
        if (GUILayout.Button("Create Final Splines"))
        {
            graphGenerator.CreateFinalSplines();
        }

        if (GUILayout.Button("Create Gameplay"))
        {
            graphGenerator.CreateGamePlay();
        }

        if (GUILayout.Button("Reset"))
        {
            graphGenerator.ResetGraphGenerator();
        }
    }
}