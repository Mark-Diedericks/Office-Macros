using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Macro_Engine.Interop
{
    public class InteropTools
    {
        /// <summary>
        /// Converts the System Types of an assembly into a custom data structure; InteropTypeInfo
        /// </summary>
        /// <param name="types">A list of types (classes) from the Assembly</param>
        /// <returns>InteropTypeInfo Array</returns>
        public static InteropTypeInfo[] GetInteropTypeInfos(Type[] types)
        {
            types = types.OrderBy(o => o.Name).ToArray();

            //Find all members of each type
            Dictionary<Type, FieldInfo[]> constants = GetTypeConstants(types);
            Dictionary<Type, PropertyInfo[]> members = GetTypeMembers(types);
            Dictionary<Type, MethodInfo[]> methods = GetTypeMethods(types);
            InteropTypeInfo[] typeInfos = CreateInteropTypeInfos(types, constants, members, methods);
            return typeInfos;
        }

        /// <summary>
        /// Returns the System Types contained within an assembly
        /// </summary>
        /// <param name="a">An instance of the assembly</param>
        /// <returns>System Type Array</returns>
        public static Type[] GetAssemblyObjects(Assembly a)
        {
            return a.GetTypes();
        }

        /// <summary>
        /// Gathers further information on each type (class) within an assembly.
        /// </summary>
        /// <param name="types">Types contained within a type</param>
        /// <param name="typeConstants">FieldInfos related to the type</param>
        /// <param name="typeMembers">PropertyInfos related to the type</param>
        /// <param name="typeMethods">MethodInfos related to the type</param>
        /// <returns>InteropTypeInfo Array</returns>
        private static InteropTypeInfo[] CreateInteropTypeInfos(Type[] types, Dictionary<Type, FieldInfo[]> typeConstants, Dictionary<Type, PropertyInfo[]> typeMembers, Dictionary<Type, MethodInfo[]> typeMethods)
        {
            List<InteropTypeInfo> infos = new List<InteropTypeInfo>();
            List<Type> st = types.OrderBy(o => o.Name).ToList();

            foreach (Type t in st)
            {
                if (Regex.IsMatch(t.Name, @"^[a-zA-Z]+$") && (!t.Name.ToLower().Trim().Contains("event")))
                {
                    InteropTypeInfo info = new InteropTypeInfo(t.Name, t.Namespace, t, new InteropMemberInfo[0]);
                    List<InteropMemberInfo> constants = new List<InteropMemberInfo>();
                    List<InteropMemberInfo> members = new List<InteropMemberInfo>();
                    List<InteropMemberInfo> methods = new List<InteropMemberInfo>();

                    if (typeConstants.ContainsKey(t))
                        foreach (FieldInfo mi in typeConstants[t])
                            if (Regex.IsMatch(mi.Name, @"^[a-zA-Z0-9]+$") && Regex.IsMatch(mi.FieldType.Name, @"^[a-zA-Z0-9]+$") && (!IsBaseImplementation(mi.Name)))
                                constants.Add(new InteropMemberInfo(InteropMemberType.PROPERTY, mi.Name, mi.IsStatic, new InteropTypeInfo(mi.FieldType.Name, mi.FieldType.Namespace, mi.FieldType, new InteropMemberInfo[0]), new InteropParamInfo[0]));

                    if (typeMembers.ContainsKey(t))
                        foreach (PropertyInfo mi in typeMembers[t])
                            if (Regex.IsMatch(mi.Name, @"^[a-zA-Z0-9]+$") && Regex.IsMatch(mi.PropertyType.Name, @"^[a-zA-Z0-9]+$") && (!IsBaseImplementation(mi.Name)))
                                members.Add(new InteropMemberInfo(InteropMemberType.PROPERTY, mi.Name, mi.GetGetMethod().IsStatic, new InteropTypeInfo(mi.PropertyType.Name, mi.PropertyType.Namespace, mi.PropertyType, new InteropMemberInfo[0]), new InteropParamInfo[0]));

                    if (typeMethods.ContainsKey(t))
                        foreach (MethodInfo mi in typeMethods[t])
                            if (Regex.IsMatch(mi.Name, @"^[a-zA-Z0-9]+$") && (!IsBaseImplementation(mi.Name)) && Regex.IsMatch(mi.ReturnType.Name, @"^[a-zA-Z0-9]+$"))
                                methods.Add(new InteropMemberInfo(InteropMemberType.METHOD, mi.Name, mi.IsStatic, new InteropTypeInfo(mi.ReturnType.Name, mi.ReturnType.Namespace, mi.ReturnType, new InteropMemberInfo[0]), GetMethodParameters(mi)));

                    constants = constants.OrderBy(o => o.name).ToList();
                    members = members.OrderBy(o => o.name).ToList();
                    methods = methods.OrderBy(o => o.name).ToList();

                    constants.AddRange(members);
                    constants.AddRange(methods);

                    info.members = constants.ToArray();
                    if (info.members.Length > 0) infos.Add(info);
                }
            }

            return infos.ToArray();
        }

        /// <summary>
        /// Gets the parameters of a method
        /// </summary>
        /// <param name="method">MethodInfo of specified method</param>
        /// <returns>Parameters of inputted method</returns>
        private static InteropParamInfo[] GetMethodParameters(MethodInfo method)
        {
            List<InteropParamInfo> infos = new List<InteropParamInfo>();

            foreach (ParameterInfo pi in method.GetParameters())
                infos.Add(new InteropParamInfo(pi.ParameterType.Name, pi.Name, pi.ParameterType));

            return infos.ToArray();
        }

        /// <summary>
        /// Identifyies whether or not a member is a base implementation
        /// </summary>
        /// <param name="member">The member to check</param>
        /// <returns>Bool identifying whether or not a member is the base implementation</returns>
        private static bool IsBaseImplementation(string member)
        {
            //Exclude inherited Object members
            //https://msdn.microsoft.com/en-us/library/system.object_methods(v=vs.110).aspx
            string l = member.ToLower().Trim();
            return l.Contains("equals") || l.Contains("finalize") || l.Contains("gethashcode") || l.Contains("gettype") || l.Contains("memberwiseclone") || l.Contains("referenceequals") || l.Contains("tostring");
        }

        /// <summary>
        /// Get the methods contained within a system type (class)
        /// </summary>
        /// <param name="types">The system type to check</param>
        /// <returns>Returns a dictionary of the methods contianed within a system type</returns>
        private static Dictionary<Type, MethodInfo[]> GetTypeMethods(Type[] types)
        {
            Dictionary<Type, MethodInfo[]> result = new Dictionary<Type, MethodInfo[]>();

            foreach (Type t in types)
                result.Add(t, t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));

            return result;
        }

        /// <summary>
        /// Get the properties contained within a system type (class)
        /// </summary>
        /// <param name="types">The system type to check</param>
        /// <returns>Returns a dictionary of the properties contianed within a system type</returns>
        private static Dictionary<Type, PropertyInfo[]> GetTypeMembers(Type[] types)
        {
            Dictionary<Type, PropertyInfo[]> result = new Dictionary<Type, PropertyInfo[]>();

            foreach (Type t in types)
                result.Add(t, t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));

            return result;
        }

        /// <summary>
        /// Get the fields contained within a system type (class)
        /// </summary>
        /// <param name="types">The system type to check</param>
        /// <returns>Returns a dictionary of the fields contianed within a system type</returns>
        private static Dictionary<Type, FieldInfo[]> GetTypeConstants(Type[] types)
        {
            Dictionary<Type, FieldInfo[]> result = new Dictionary<Type, FieldInfo[]>();

            foreach (Type t in types)
                result.Add(t, t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToArray());

            return result;
        }
    }
}
