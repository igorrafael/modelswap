using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BoneTracker
{
    [SerializeField]
    private List<TransformMatcher> _bones;

    public BoneTracker(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        Transform root = skinnedMeshRenderer.rootBone;
        _bones = skinnedMeshRenderer.bones.Select(b => new TransformMatcher(root, b)).ToList();
    }

    internal Transform[] Match(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        Transform root = skinnedMeshRenderer.rootBone;
        return _bones.Select(b => b.FindMatch(root)).ToArray();
    }
}
