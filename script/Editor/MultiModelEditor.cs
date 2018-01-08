using System;
using System.Collections.Generic;
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

        private Dictionary<Transform, bool> _modelFoldout = new Dictionary<Transform, bool>();
        private Dictionary<int, bool> _logFoldout = new Dictionary<int, bool>();
        private int _indent;

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

                    LogGUI(model);

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
                        select new { t, r, a };
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

        private void LogGUI(Transform model)
        {
            if (!_modelFoldout.ContainsKey(model))
            {
                _modelFoldout[model] = false;
            }
            _modelFoldout[model] = EditorGUILayout.Foldout(_modelFoldout[model], model.name);
            if (_modelFoldout[model])
            {
                var tgts = targets.Cast<MultiModel>().ToArray();

                foreach (var tgt in tgts)
                {
                    EditorGUILayout.BeginVertical("Box");
                    {
                        ModelSwapper swapper = new ModelSwapper(tgt[model]);
                        swapper.dryRun = true;
                        _indent = 0;
                        swapper.Match(tgt.transform).ForEach(LogEntryGUI);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void LogEntryGUI(ModelSwapper.LogEntry entry)
        {
            var key = entry.id;
            /*
            var key = entry.local as Transform
                ?? entry.model as Transform
                ?? (entry.local as Component).transform
                ?? (entry.model as Component).transform;
                */

            var foldout = _logFoldout;
            if (!foldout.ContainsKey(key))
            {
                foldout[key] = true;
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(6 * _indent);
                foldout[key] = EditorGUILayout.Foldout(entry.Count == 0 || foldout[key], GUIContent.none);
                ObjectField(entry.local);
                ObjectField(entry.model);
            }
            EditorGUILayout.EndHorizontal();

            if (foldout[key])
            {
                ++_indent;
                entry.ForEach(LogEntryGUI);
                --_indent;
            }
        }

        private static void ObjectField(Object obj)
        {
            Type objType = obj == null ? typeof(Object) : obj.GetType();
            EditorGUILayout.ObjectField(obj, objType, false);
        }

        private static void ObjectField<T>(ref T obj)
            where T : Object
        {
            obj = EditorGUILayout.ObjectField(obj, typeof(T), false) as T;
        }
    }
}
