using System.Linq;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MultiModel))]
public class MultiModelEditor : Editor
{
    private SerializedProperty _models;

    void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        _models = serializedObject.FindProperty("models");

        var tgts = targets.Cast<MultiModel>().ToArray();
        var distinctModels = tgts.SelectMany(t => t.models.Distinct()).GroupBy(t => t);

        foreach (var group in distinctModels)
        {
            var model = group.Key;
            if (GUILayout.Button("Activate " + model.name + " (" + group.Count() + ")"))
            {
                foreach (var tgt in tgts.Where(t => t.models.Contains(model)))
                {
                    tgt.Swap(model);
                }
            }
        }

        serializedObject.Update();
        _models.isExpanded = true;
        EditorGUILayout.PropertyField(_models, true);
        serializedObject.ApplyModifiedProperties();
    }
}
