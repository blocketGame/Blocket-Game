#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator), true)]
public class RandomDungeonGeneratorEditor : Editor
{
    DungeonGenerator generator;

    private void Awake()
    {
        generator = (DungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create Dungeon"))
        {
            generator.GenerateDungeon();
        }
    }
}
#endif
