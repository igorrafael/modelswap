using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoneTracker
{
    private class TrackedBone
    {
        readonly List<string> _path = new List<string>();

        public TrackedBone(Transform rootBone, Transform bone)
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

    private List<TrackedBone> bones;

    public BoneTracker(SkinnedMeshRenderer smr)
    {
        bones = smr.bones.Select(b => new TrackedBone(smr.rootBone, b)).ToList();
    }

    internal Transform[] Match(Transform root)
    {
        return bones.Select(b => b.FindMatch(root)).ToArray();
    }
}
