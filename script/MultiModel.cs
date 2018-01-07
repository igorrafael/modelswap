using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModelSwap
{
    public class MultiModel : MonoBehaviour
    {
        public Transform[] models = new Transform[0];
        [SerializeField]
        private ModelReference[] _models = new ModelReference[0];
        public Transform currentModel;
        private ModelReference _currentReference;

        public void OnValidate()
        {
            _models = models.Where(m => m != null).Select(m => new ModelReference(transform, m)).ToArray();
        }

        public void Swap(Transform model)
        {
            var swapper = new ModelSwapper(_models.FirstOrDefault(m => m.Model == model));
            swapper.Match(transform, model);
            currentModel = model;
        }
    }
}