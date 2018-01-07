using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelSwapper
{
    private readonly ModelReference _reference;

    public ModelSwapper(ModelReference reference)
    {
        _reference = reference;
    }

    public void Match(Transform local, Transform model)
    {
        Component[] components = model.GetComponents<Component>();

        if (components.Any(c => c is MultiModel))
        {
            return;
        }

        local.gameObject.SetActive(model.gameObject.activeSelf);

        foreach (Component modelComponent in components)
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
            Transform localChild;
            string name = modelChild.name;
            if (childDictionary.ContainsKey(name))
            {
                localChild = childDictionary[name];
            }
            else
            {
                //TODO treat this better
                localChild = new GameObject(name).transform;
                localChild.SetParent(local);
            }

            childDictionary.Remove(name);

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

        local.bones = _reference.GetBones(local, model);
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
