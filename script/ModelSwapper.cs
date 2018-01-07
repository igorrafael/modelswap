using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSwapper : MonoBehaviour
{
    public Transform[] models = new Transform[0];
    [SerializeField]
    private ModelReference[] _models = new ModelReference[0];
    public Transform currentModel;
    private ModelReference _currentReference;

    [Serializable]
    private class ModelReference
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
                select new BoneSet(modelSmr, tracker.Match(localSmr.rootBone))
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

    public void OnValidate()
    {
        _models = models.Where(m => m != null).Select(m => new ModelReference(transform, m)).ToArray();
    }

    public void Swap(Transform model)
    {
        _currentReference = _models.FirstOrDefault(m => m.Model == model);
        Match(transform, model);
        currentModel = model;
    }

    private void Match(Transform local, Transform model)
    {
        local.gameObject.SetActive(model.gameObject.activeSelf);

        foreach (Component modelComponent in model.GetComponents<Component>())
        {
            if (modelComponent is Transform)
            {
                continue;
            }

            Type type = modelComponent.GetType();
            Component localComponent = local.GetComponent(type) ?? local.gameObject.AddComponent(type);

#if UNITY_EDITOR
            if (local.GetComponents(type).Length > 1)
            {
                throw new Exception();
            }
#endif

            Match(localComponent, modelComponent);
        }

        Dictionary<string, Transform> childDictionary = CreateChildDictionary(local);

        foreach (Transform modelChild in model)
        {
            Transform localChild = childDictionary[modelChild.name];
            childDictionary.Remove(modelChild.name);

            localChild.localPosition = modelChild.localPosition;
            localChild.localRotation = modelChild.localRotation;
            localChild.localScale = modelChild.localScale;

            Match(localChild, modelChild);
        }

        foreach (Transform unvisited in childDictionary.Values)
        {
            //IDEA: check if this was in the _currentModel
            if (unvisited.GetComponent<SkinnedMeshRenderer>() != null)
            {
                unvisited.gameObject.SetActive(false);
            }
        }
    }

    //IDEA: bake this dictionary for runtime use
    private static Dictionary<string, Transform> CreateChildDictionary(Transform model)
    {
#if UNITY_EDITOR
        var childGroups = model.Cast<Transform>().GroupBy(t => t.name);
        if (childGroups.Any(g => g.Count() > 1))
        {
            throw new Exception();
        }
#endif

        return model.Cast<Transform>().ToDictionary(c => c.name);
    }

    private void Match(Component localComponent, Component modelComponent)
    {
        SkinnedMeshRenderer skinned = modelComponent as SkinnedMeshRenderer;
        if (skinned)
        {
            MatchComponent(localComponent as SkinnedMeshRenderer, skinned);
        }
        else
        {
            MatchComponent(localComponent as MeshRenderer, modelComponent as MeshRenderer);
            MatchComponent(localComponent as MeshFilter, modelComponent as MeshFilter);
        }

        MatchComponent(localComponent as Animator, modelComponent as Animator);
    }

    private void MatchComponent(SkinnedMeshRenderer local, SkinnedMeshRenderer model)
    {
        if (local == null || model == null)
        {
            return;
        }

        local.sharedMesh = model.sharedMesh;
        local.sharedMaterials = model.sharedMaterials;

        local.bones = _currentReference.GetBones(local, model);
    }

    private void MatchComponent(MeshRenderer local, MeshRenderer model)
    {
        if (local == null || model == null)
        {
            return;
        }

        local.sharedMaterials = model.sharedMaterials;
    }

    private void MatchComponent(MeshFilter local, MeshFilter model)
    {
        if (local == null || model == null)
        {
            return;
        }

        local.sharedMesh = model.sharedMesh;
    }

    private void MatchComponent(Animator local, Animator model)
    {
        if (local == null || model == null)
        {
            return;
        }

        if (model.runtimeAnimatorController)
        {
            local.avatar = model.avatar;
            local.runtimeAnimatorController = model.runtimeAnimatorController;
        }
    }
}
