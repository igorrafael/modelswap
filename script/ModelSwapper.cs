using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class ModelSwapper : MonoBehaviour
{
    public Transform[] models = new Transform[0];
    public Transform currentModel;

/*
    [Serializable]
    private class ModelReference
    {
        [Serializable]
        public class BoneSet
        {
            private SkinnedMeshRenderer _skin;
            private Transform[] _bones;

            public BoneSet(SkinnedMeshRenderer smr)
            {
                _skin = smr;
            }

            internal void OnValidate(Transform boneRoot)
            {
            }

            public void Apply()
            {
                _skin.bones = _bones;
            }
        }

        public Transform model;
        public BoneSet[] bones;

        public void OnValidate(ModelSwapper transform)
        {
            bones = model.GetComponentsInChildren<SkinnedMeshRenderer>().Select(s => new BoneSet(s)).ToArray();

            foreach(BoneSet set in bones)
            {
            }
        }
    }
*/
    public void Swap(Transform model)
    {
        currentModel = model;

        Match(transform, model.transform);
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
            unvisited.gameObject.SetActive(false);
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

        local.bones = new BoneTracker(model).Match(local.rootBone);
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
