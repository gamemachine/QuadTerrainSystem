using UnityEditor;
using UnityEngine;

namespace QuadTerrain
{
    [CustomEditor(typeof(QuadTerrainManager))]
    public class QuadTerrainManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var manager = (QuadTerrainManager)target;

           
            EditorGUILayout.Space();
            if (GUILayout.Button("Build"))
            {
                manager.Build();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("TestDisplayRenderNodes"))
            {
                manager.TestDisplayRenderNodes();
            }

        }
    }
}
