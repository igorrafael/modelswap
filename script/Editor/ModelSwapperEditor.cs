using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ModelSwapper))]
public class ModelSwapperEditor : Editor {
    
    public override void OnInspectorGUI()
    {
        var tgt = target as ModelSwapper;

        foreach(var t in tgt.models)
        {
            if (GUILayout.Button("Activate " + t.name))
            {
                tgt.Swap(t);
            }
        }
    }
}
