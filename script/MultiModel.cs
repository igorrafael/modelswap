﻿using System.Linq;
using UnityEngine;

namespace ModelSwap
{
    public class MultiModel : MonoBehaviour
    {
        public Transform[] models = new Transform[0];
        [SerializeField]
        private ModelReference[] _modelReference = new ModelReference[0];
        public Transform currentModel;
        [SerializeField]
        private bool _bakeBones;

        public void OnValidate()
        {
            _modelReference = models.Where(m => m != null).Select(GetReferenceOrNew).ToArray();
        }

        private ModelReference GetReferenceOrNew(Transform model)
        {
            ModelReference reference = _modelReference.FirstOrDefault(m => m.Model == model);
            return reference ?? new ModelReference(transform, model, _bakeBones);
        }

        public bool Swap(Transform model)
        {
            ModelReference reference = _modelReference.FirstOrDefault(m => m.Model == model);
            if (reference == null)
            {
                return false;
            }

            ModelSwapper swapper = new ModelSwapper(reference);
            swapper.Match(transform, model);
            currentModel = model;

            return true;
        }
    }
}