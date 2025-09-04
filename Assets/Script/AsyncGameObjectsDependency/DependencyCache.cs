using System;
using System.Collections.Generic;
using System.Reflection;

namespace AsyncGameObjectsDependency.Core
{
    public class DependencyCache
    {
        private static readonly Dictionary<Type, List<MemberInfo>> DependencyMembersCache =
            new Dictionary<Type, List<MemberInfo>>();

        public static List<MemberInfo> GetDependencyMembers(Type type)
        {
            if (!DependencyMembersCache.TryGetValue(type, out List<MemberInfo> dependencyMembers))
            {
                dependencyMembers = GetDependencyMembersInternal(type);
            }
            
            return dependencyMembers;

            List<MemberInfo> GetDependencyMembersInternal(Type type)
            {
                BindingFlags allMembers = BindingFlags.Static | BindingFlags.Instance |
                                          BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.DeclaredOnly;
                List<MemberInfo> dependencyMembers = new List<MemberInfo>();
                Type bbType = type;
                while (bbType != null && bbType != typeof (ADMonoBehaviour))
                {
                    FieldInfo[] fields = bbType.GetFields(allMembers);
                    foreach (var field in fields)
                    {
                        var isDefined = field.IsDefined(typeof(ADDependencyInject), inherit: true);
                        if (!field.FieldType.IsPrimitive && isDefined) dependencyMembers.Add(field);
                    }
                    bbType = bbType.BaseType;
                }
                DependencyMembersCache[type] = dependencyMembers;
                return dependencyMembers;
            }
            
        }
    }
}