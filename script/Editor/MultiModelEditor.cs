using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModelSwap
{
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
                EditorGUILayout.BeginVertical("Box");
                {
                    var model = group.Key;
                    if (GUILayout.Button("Activate " + model.name + " (" + group.Count() + ")"))
                    {
                        foreach (var tgt in tgts.Where(t => t.models.Contains(model)))
                        {
                            tgt.Swap(model);
                        }
                    }

                    var s =
                        from t in tgts
                        let r = t.GetReferenceOrNew(model)
                        from a in r._animators
                        select new { t, r, a};
                    foreach (var b in s)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            var a = b.a.animator;
                            ObjectField(ref a);
                            ObjectField(ref b.a.controllerOverride);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
            }

            serializedObject.Update();
            _models.isExpanded = true;
            EditorGUILayout.PropertyField(_models, true);
            serializedObject.ApplyModifiedProperties();
        }

        private static void ObjectField<T>(ref T obj)
            where T : Object
        {
            obj = EditorGUILayout.ObjectField(obj, typeof(T), false) as T;
        }
    }
}
