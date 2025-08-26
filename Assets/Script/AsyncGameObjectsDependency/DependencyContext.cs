using System;
using UnityEngine;

namespace AsyncGameObjectsDependency.Core
{
    public class DependencyContext : MonoBehaviour
    {
        public static DIContainer CurrenctContainer = new DIContainer();
        private bool IsDependenciesChanged = false;
        void Awake()
        {
            CurrenctContainer.OnDependenciesChanged += OnDependenciesChanged;
        }

        void OnDestroy()
        {
            CurrenctContainer.OnDependenciesChanged -= OnDependenciesChanged;
        }
        private void OnDependenciesChanged()
        {
            IsDependenciesChanged = true;
        }

        protected void LateUpdate()
        {
            if (IsDependenciesChanged)
            {
                IsDependenciesChanged = false;
            }
            
            ADMonoBehaviour.CallAllObjectsLateUpdates();
        }

        protected void Update()
        {
            if (IsDependenciesChanged)
            {
                IsDependenciesChanged = false;
            }
            
            ADMonoBehaviour.CallAllObjectsUpdates();
        }
        
    }
}