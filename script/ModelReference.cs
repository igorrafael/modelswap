using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class ModelReference
{
    [Serializable]
    private class BoneSet
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public Transform[] bones;

        public BoneSet(SkinnedMeshRenderer smr, Transform[] bones)
        {
            skinnedMeshRenderer = smr;
            this.bones = bones;
            if (this.bones.Length != smr.bones.Length)
            {
                throw new ArgumentException();
            }
        }
    }

    [SerializeField]
    private Transform _model;
    [SerializeField]
    private RuntimeAnimatorController _controller;
    [SerializeField]
    private Avatar avatar;
    [SerializeField]
    private BoneSet[] _bones;

    public Transform Model
    {
        get
        {
            return _model;
        }
    }

    public ModelReference(Transform local, Transform model)
    {
        _model = model;

        _bones = (
            from modelSmr in model.GetComponentsInChildren<SkinnedMeshRenderer>()
            let tracker = new BoneTracker(modelSmr)
            let localTransform = FindMatch(local, model, modelSmr.transform)
            where localTransform
            let localSmr = localTransform.GetComponent<SkinnedMeshRenderer>()
            where localSmr
            select new BoneSet(modelSmr, tracker.Match(localSmr))
            ).ToArray();
    }

    public Transform FindMatch(Transform searchRoot, Transform referenceRoot, Transform referenceTarget)
    {
        try
        {
            var tracker = new TransformMatcher(referenceRoot, referenceTarget);
            return tracker.FindMatch(searchRoot);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }

    internal Transform[] GetBones(SkinnedMeshRenderer local, SkinnedMeshRenderer model)
    {
        return _bones.FirstOrDefault(b => b.skinnedMeshRenderer == model).bones;
    }
}
