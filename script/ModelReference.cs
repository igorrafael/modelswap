using System;
using System.Linq;
using UnityEngine;

namespace ModelSwap
{
    [Serializable]
    public class ModelReference
    {
        [Serializable]
        private struct BoneSet
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

        [Serializable]
        private struct AnimatorOverride
        {
            public Animator animator;
            public RuntimeAnimatorController controllerOverride;
        }

        [SerializeField]
        private Transform _model;
        [SerializeField]
        private BoneSet[] _bones = new BoneSet[0];

        [SerializeField]
        private AnimatorOverride[] _animators = new AnimatorOverride[0];


        public Transform Model
        {
            get
            {
                return _model;
            }
        }

        public ModelReference(Transform local, Transform model, bool bakeBones = true)
        {
            _model = model;
            if (bakeBones)
            {
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

            _animators = (
                from animator in model.GetComponentsInChildren<Animator>()
                select new AnimatorOverride
                    {
                        animator = animator
                    }
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
            BoneSet boneSet = _bones.FirstOrDefault(b => b.skinnedMeshRenderer == model);
            return boneSet.bones ?? new BoneTracker(model).Match(local);
        }

        internal RuntimeAnimatorController GetController(Animator local)
        {
            AnimatorOverride animatorOverride = _animators.FirstOrDefault(o => o.animator == local);
            return animatorOverride.controllerOverride;
        }
    }
}