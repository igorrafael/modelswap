using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ModelSwap
{
    public class ModelSwapper
    {
        public class LogEntry : List<LogEntry>
        {
            public static int lastId = 0;

            readonly public int id;
            readonly public string message;
            readonly public Object local, model;

            public LogEntry(string msg, Object from, Object to)
            {
                this.message = msg;
                this.local = from;
                this.model = to;
                this.id = lastId++;
            }

            public override int GetHashCode()
            {
                return id;
            }
        }

        public bool dryRun;
        private readonly ModelReference _reference;

        public ModelSwapper(ModelReference reference)
        {
            _reference = reference;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="local"></param>
        /// <param name="dryRun">Only write a log of changes</param>
        public LogEntry Match(Transform local)
        {
            LogEntry.lastId = 0;
            return Match(local, _reference.Model);
        }

        private LogEntry Match(Transform local, Transform model)
        {
            Component[] components = model.GetComponents<Component>();

            if (components.Any(c => c is MultiModel))
            {
                return new LogEntry("Nested multimodel", local, model);
            }

            LogEntry log = new LogEntry("Transforms", local, model);

            if (!dryRun)
            {
                local.gameObject.SetActive(model.gameObject.activeSelf);
            }

            foreach (Component modelComponent in components)
            {
                if (modelComponent is Transform)
                {
                    continue;
                }

                Type type = modelComponent.GetType();
                Component localComponent = local.GetComponent(type);
                if (!dryRun && localComponent == null)
                {
                    local.gameObject.AddComponent(type);
                }

#if UNITY_EDITOR
                if (local.GetComponents(type).Length > 1)
                {
                    throw new Exception();
                }
#endif
                log.Add(Match(localComponent, modelComponent));
            }

            Dictionary<string, Transform> childDictionary = CreateChildDictionary(local);

            foreach (Transform modelChild in model)
            {
                Transform localChild;
                string name = modelChild.name;
                bool missingChild = false;
                if (childDictionary.ContainsKey(name))
                {
                    localChild = childDictionary[name];
                    childDictionary.Remove(name);
                }
                else
                {
                    missingChild = true;
                    localChild = new GameObject(name).transform;
                    localChild.SetParent(local);
                }


                localChild.localPosition = modelChild.localPosition;
                localChild.localRotation = modelChild.localRotation;
                localChild.localScale = modelChild.localScale;

                log.Add(Match(localChild, modelChild));

                if (missingChild && dryRun)
                {
                    Object.DestroyImmediate(localChild.gameObject);
                }
            }

            foreach (Transform unvisited in childDictionary.Values)
            {
                //IDEA: check if this was in the _currentModel
                if (!dryRun && unvisited.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    unvisited.gameObject.SetActive(false);
                }
                log.Add(new LogEntry("unvisited", unvisited, null));
            }

            return log;
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

        private LogEntry Match(Component localComponent, Component modelComponent)
        {
            if (!dryRun)
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
            return new LogEntry("Component", localComponent, modelComponent);
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

            var controller = _reference.GetController(model) ?? model.runtimeAnimatorController;
            if (controller)
            {
                //TODO: check uses of this
                //WARNING: runtime changing is not fully supported by unity (according to API)
                //local.avatar = model.avatar;

                local.runtimeAnimatorController = controller;
            }
        }
    }
}