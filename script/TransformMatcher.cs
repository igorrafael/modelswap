using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TransformMatcher
{
    [SerializeField]
    private List<string> _path = new List<string>();

    public TransformMatcher(Transform rootBone, Transform bone)
    {
        while (rootBone != bone)
        {
            if (bone == null)
            {
                throw new Exception();
            }

            _path.Add(bone.name);
            bone = bone.parent;
        }
        _path.Reverse();
    }

    internal Transform FindMatch(Transform root)
    {
        return _path.Aggregate(root, (a, b) => a.Find(b));
    }
}
