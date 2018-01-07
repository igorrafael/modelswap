using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BoneTracker
{
    [Serializable]
    public class TrackedBone
    {
        [SerializeField]
        private List<string> _path = new List<string>();

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

    [SerializeField]
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField]
    private List<TrackedBone> _bones;

    public BoneTracker(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        _skinnedMeshRenderer = skinnedMeshRenderer;
        Transform root = skinnedMeshRenderer.rootBone;
        _bones = skinnedMeshRenderer.bones.Select(b => new TrackedBone(root, b)).ToList();
    }

    internal Transform[] Match(Transform root)
    {
        return _bones.Select(b => b.FindMatch(root)).ToArray();
    }
}
