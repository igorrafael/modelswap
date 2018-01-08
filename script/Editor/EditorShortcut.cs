using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ModelSwap.EditorShortcut
{
    public class Vertical : IDisposable
    {
        public Vertical()
        {
            EditorGUILayout.BeginVertical();
        }

        public Vertical(string s)
        {
            EditorGUILayout.BeginVertical(s);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }
    public class Horizontal : IDisposable
    {
        public Horizontal()
        {
            EditorGUILayout.BeginHorizontal();
        }

        public Horizontal(string s)
        {
            EditorGUILayout.BeginHorizontal(s);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    public static class Field
    {
        public static void Object(Object obj)
        {
            Type objType = obj == null ? typeof(Object) : obj.GetType();
            EditorGUILayout.ObjectField(obj, objType, false);
        }

        public static void Object<T>(ref T obj)
            where T : Object
        {
            obj = EditorGUILayout.ObjectField(obj, typeof(T), false) as T;
        }
    }
}