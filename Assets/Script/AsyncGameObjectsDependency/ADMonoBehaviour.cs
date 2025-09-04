using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AsyncGameObjectsDependency.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ADDependencyInject : Attribute
    {
    }

    public class ADMonoBehaviour : MonoBehaviour
    {
        private static readonly Dictionary<Type, bool> TypeToMethodValid = new Dictionary<Type, bool>();
        private bool ComputeIsUpdateReady =>
            _FilledAllDependencies && !IsDestroyingBehaviour && _IsBehaviourEnabled;
        
        private DIContainer _DiContainer;
        public DIContainer DiContainer
        {
            get
            {
                SetupContainer();
                return _DiContainer;
            }
        }
        
        private bool _FilledAllDependencies = false;
        public bool FilledAllDependencies
        {
            get => _FilledAllDependencies;
            private set
            {
                _FilledAllDependencies = value;
                IsUpdateReady = ComputeIsUpdateReady;
            }
        }

        [NonSerialized] public bool IsRegisteredAsDependency = false;
        private bool _isDestroyingBehaviour;
        public bool IsDestroyingBehaviour
        {
            get => _isDestroyingBehaviour;
            private set
            {
                _isDestroyingBehaviour = value;
                IsUpdateReady = ComputeIsUpdateReady;
            }
        }
        
        private bool _IsBehaviourEnabled;
        protected bool IsBehaviourEnabled
        {
            get => _IsBehaviourEnabled;
            set
            {
                _IsBehaviourEnabled = value;
                IsUpdateReady = ComputeIsUpdateReady;
            }
        }
        
        private bool IsUpdateReady;

        private void SetupContainer()
        {
            if (_DiContainer != null)
            {
                return;
            }
            _DiContainer = DependencyContext.CurrenctContainer;

            SetupDependencies();
        }

        private void SetupDependencies()
        {
            var type = GetType();
            List<MemberInfo> requests = DependencyCache.GetDependencyMembers(type);
            if (requests.Count == 0)
            {
                ReadyDependencies();
            }
            else
            {
                _DiContainer.Request(this, requests);
            }
        }
        
        private static readonly HashSet<ADMonoBehaviour> BehavioursToUpdate = new HashSet<ADMonoBehaviour>();
        private static bool BehavioursToUpdateArrayNeedsUpdate = true;
        private static ADMonoBehaviour[] BehavioursToUpdateArray = new ADMonoBehaviour[512];
        private static int BehavioursToUpdateCount = 0;

        public static void CallAllObjectsUpdates()
        {
            if (BehavioursToUpdateArrayNeedsUpdate)
            {
                UpdateBehavioursToUpdateArray();
            }
            for (int i = 0, len = BehavioursToUpdateCount; i < len; i++)
            {
                ADMonoBehaviour adMonoBehaviour = BehavioursToUpdateArray[i];
                if (adMonoBehaviour.IsUpdateReady)
                {
                    adMonoBehaviour.ADUpdate();
                }
            }
        }
        
        public static void CallAllObjectsLateUpdates()
        {
            if (BehavioursToUpdateArrayNeedsUpdate)
            {
                UpdateBehavioursToUpdateArray();
            }
            for (int i = 0, len = BehavioursToUpdateCount; i < len; i++)
            {
                ADMonoBehaviour adMonoBehaviour = BehavioursToUpdateArray[i];
                if (adMonoBehaviour.IsUpdateReady)
                {
                    adMonoBehaviour.ADLateUpdate();
                }
            }
        }
        
        private static void UpdateBehavioursToUpdateArray()
        {
            BehavioursToUpdateArrayNeedsUpdate = false;
            BehavioursToUpdateCount = BehavioursToUpdate.Count;
            if (BehavioursToUpdateArray.Length < BehavioursToUpdateCount)
            {
                Array.Resize(ref BehavioursToUpdateArray, Mathf.NextPowerOfTwo(BehavioursToUpdateCount));
            }
            BehavioursToUpdate.CopyTo(BehavioursToUpdateArray, 0, BehavioursToUpdateCount);
            Array.Clear(BehavioursToUpdateArray, BehavioursToUpdateCount,
                BehavioursToUpdateArray.Length - BehavioursToUpdateCount);
        }

        public void ReadyDependencies()
        {
            if (!_FilledAllDependencies)
            {
                _FilledAllDependencies = true;
                IsUpdateReady = ComputeIsUpdateReady;
                ADOnFilledDependencies();
            }
        }
        
        protected void Awake()
        {
            ADAwake();
            if (_DiContainer != null)
            {
                SetupDependencies();
            }
            else
            {
                SetupContainer();
            }
            
            Type t = GetType();
            bool valid;
            if (!TypeToMethodValid.TryGetValue(t, out valid))
            {
                MethodInfo miUpdate = t.GetMethod("ADUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo miLateUpdate = t.GetMethod("ADLateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

                valid = (miUpdate != null || miLateUpdate != null) && (miUpdate.DeclaringType != typeof(ADMonoBehaviour) || miLateUpdate.DeclaringType != typeof(ADMonoBehaviour));
                TypeToMethodValid.Add(t, valid);
            }

            if (valid)
            {
                BehavioursToUpdate.Add(this);
                BehavioursToUpdateArrayNeedsUpdate = true;
            }
        }
        
        protected void Start()
        {
            ADStart();
        }
        
        protected void OnDestroy()
        {
            if (IsDestroyingBehaviour)
            {
                return;
            }

            IsDestroyingBehaviour = true;

            if (_DiContainer != null)
            {
                _DiContainer.UnloadObject(this);
            }
            
            this.ADOnDestroy();
            
            BehavioursToUpdate.Remove(this);
            BehavioursToUpdateArrayNeedsUpdate = true;
        }
        
        protected void OnEnable()
        {
            IsBehaviourEnabled = true;
            this.ADOnEnable();
        }

        protected void OnDisable()
        {
            IsBehaviourEnabled = false;
            this.ADOnDisable();
        }


        /// <summary>
        /// Called once on Awake, often *before* a LocalContext is established or any dependencies are filled
        /// </summary>
        protected virtual void ADAwake()
        {
        }
        
        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        protected virtual void ADStart()
        {
        }
        
        /// <summary>
        /// Called when this Behaviour is unloaded from the scene (but before destruction)
        /// </summary>
        protected virtual void ADOnUnload()
        {
        }

        /// <summary>
        /// Called once after all [Dependency] members have been filled
        /// </summary>
        protected virtual void ADOnFilledDependencies()
        {
        }

        /// <summary>
        /// Called every Update only if this Behaviour has its dependencies filled
        /// </summary>
        protected virtual void ADUpdate()
        {
        }
        
        /// <summary>
        /// Called every LateUpdate only if this Behaviour has its dependencies filled
        /// </summary>
        protected virtual void ADLateUpdate()
        {
        }


        /// <summary>
        /// Called after this Behaviour is finally destroyed
        /// </summary>
        protected virtual void ADOnDestroy()
        {
        }

        /// <summary>
        /// Called after this Behaviour is enabled
        /// </summary>
        protected virtual void ADOnEnable()
        {
        }

        /// <summary>
        /// Called after this Behaviour is disabled
        /// </summary>
        protected virtual void ADOnDisable()
        {
        }

    }
}