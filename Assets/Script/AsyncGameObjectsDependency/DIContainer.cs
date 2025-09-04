using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AsyncGameObjectsDependency.Core
{  
    public class UnityDependencyEntry
    {
        public Type Type;
        public object Target;
    }
    
    public class DIContainer
    {       
        private bool AlreadyRegistering;
        private readonly Dictionary<Type, UnityDependencyEntry> PendingRegistry = new Dictionary<Type, UnityDependencyEntry>();
        private readonly Dictionary<Type, UnityDependencyEntry> DependencyRegistry = new Dictionary<Type, UnityDependencyEntry>();
        public readonly Dictionary<ADMonoBehaviour, List<MemberInfo>> ObjectsToDependencies = new Dictionary<ADMonoBehaviour, List<MemberInfo>>();
        private readonly List<ADMonoBehaviour> TempReadied = new List<ADMonoBehaviour>();

        public event Action OnDependenciesChanged;

        public void Register<T>(T obj) where T : UnityEngine.Object
        {
            Register(obj, typeof(T));
        }

        public void Register(UnityEngine.Object obj, Type type)
        {
            if (obj is ADMonoBehaviour bb)
            {
                bb.IsRegisteredAsDependency = true;
            }
            
            UnityDependencyEntry newEntry = new UnityDependencyEntry();
            newEntry.Type = type;
            newEntry.Target = obj;
            
            PendingRegistry[type] = newEntry;
            if (AlreadyRegistering)
            {
                return;
            }
            
            AlreadyRegistering = true;
            try
            {
                while (PendingRegistry.Count > 0)
                {
                    KeyValuePair<Type, UnityDependencyEntry> kvp = PendingRegistry.First();
                    PendingRegistry.Remove(kvp.Key);
                    DoRegister(kvp.Value);
                }
            }
            finally
            {
                AlreadyRegistering = false;
            }

            OnDependenciesChanged?.Invoke();
        }
        private void DoRegister(UnityDependencyEntry entry)
        {
            DependencyRegistry[entry.Type] = entry;
            foreach (KeyValuePair<ADMonoBehaviour, List<MemberInfo>> kvp in ObjectsToDependencies)
            {
                ADMonoBehaviour requestingObj = kvp.Key;
                List<MemberInfo> requests = kvp.Value;
                for (int i = 0; i < requests.Count; i++)
                {
                    MemberInfo request = requests[i];
                    if (entry.Type == GetMemberType(request))
                    {
                        SetMemberValue(request, requestingObj, entry.Target);
                        requests[i] = requests[requests.Count - 1];
                        requests.RemoveAt(requests.Count - 1);
                        i--;
                    }
                }
                if (requests.Count == 0)
                {
                    TempReadied.Add(kvp.Key);
                }
            }

            foreach (ADMonoBehaviour ready in TempReadied)
            {
                ObjectsToDependencies.Remove(ready);
                ready.ReadyDependencies();
            }
            
            TempReadied.Clear();
        }
        
        public void Request(ADMonoBehaviour requestor, List<MemberInfo> requests)
        {
            requests = new List<MemberInfo>(requests);

            List<MemberInfo> existingRequests;
            if (ObjectsToDependencies.TryGetValue(requestor, out existingRequests))
            {
                foreach (MemberInfo member in existingRequests)
                {
                    if (!requests.Contains(member))
                    {
                        requests.Add(member);
                    }
                }
                ObjectsToDependencies.Remove(requestor);
            }

            for (int i = 0; i < requests.Count; i++)
            {
                UnityDependencyEntry registered;
                if (DependencyRegistry.TryGetValue(GetMemberType(requests[i]), out registered))
                {
                    SetMemberValue(requests[i], requestor, registered.Target);
                    requests[i] = requests[requests.Count - 1];
                    requests.RemoveAt(requests.Count - 1);
                    i--;
                }
            }

            if (requests.Count == 0)
            {
                requestor.ReadyDependencies();
            }
            else
            {
                ObjectsToDependencies.Add(requestor, requests);
            }

            OnDependenciesChanged?.Invoke();
        }
        
        private static readonly List<Type> TypeGraveyard = new List<Type>();
        public void UnloadObject(ADMonoBehaviour toUnload)
        {
            if (toUnload.IsRegisteredAsDependency)
            {
                foreach (KeyValuePair<Type, UnityDependencyEntry> kvp in DependencyRegistry)
                {
                    ADMonoBehaviour bb = kvp.Value.Target as ADMonoBehaviour;
                    if (bb == toUnload)
                    {
                        TypeGraveyard.Add(kvp.Key);
                    }
                }

                foreach (Type type in TypeGraveyard)
                {
                    DependencyRegistry.Remove(type);
                }

                TypeGraveyard.Clear();
            }
            

            ObjectsToDependencies.Remove(toUnload);
        }

        public Type GetMemberType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field: return ((FieldInfo) member).FieldType;
                case MemberTypes.Property: return ((PropertyInfo) member).PropertyType;
                default: throw new InvalidCastException("Unexpected MemberInfo.MemberType: " + member.MemberType);
            }
        }
        
        public static void SetMemberValue(MemberInfo member, object obj, object value)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo) member).SetValue(obj, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo) member).SetValue(obj, value);
                    break;
                default:
                    throw new InvalidCastException("Unexpected MemberInfo.MemberType: " + member.MemberType);
            }
        }
        
        
    }
}