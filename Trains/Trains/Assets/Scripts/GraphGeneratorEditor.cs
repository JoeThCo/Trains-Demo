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
        if (GUILayout.Button("Create Final Splines"))
        {
            graphGenerator.ResetGraphGenerator();
            graphGenerator.CreateFinalSplines();
        }

        if (GUILayout.Button("Create Gameplay"))
        {
            graphGenerator.ResetGraphGenerator();
            graphGenerator.CreateFinalSplines();
            graphGenerator.CreateGameplay();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Reset"))
        {
            graphGenerator.ResetGraphGenerator();
        }
    }
}