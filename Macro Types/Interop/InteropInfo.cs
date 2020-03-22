using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Interop
{
    /// <summary>
    /// Enum identifying member type
    /// </summary>
    public enum InteropMemberType
    {
        METHOD = 0,
        PROPERTY = 1
    }

    /// <summary>
    /// Data strcture containing info on a type (class)
    /// </summary>
    public struct InteropTypeInfo
    {
        public string nameregion;
        public string name;

        public Type type;
        public InteropMemberInfo[] members;

        /// <summary>
        /// Initialize a new instance of the data structure
        /// </summary>
        /// <param name="n">Name of the type</param>
        /// <param name="s">Namespace of the type</param>
        /// <param name="t">System Type of the type</param>
        /// <param name="m">Members of the type (array)</param>
        public InteropTypeInfo(string n, string s, Type t, InteropMemberInfo[] m)
        {
            name = n;
            nameregion = s;
            type = t;
            members = m;
        }
    }

    /// <summary>
    /// Data structure containing info on a parameter contained within a type
    /// </summary>
    public struct InteropParamInfo
    {
        public string name;
        public string actualname;

        public Type type;

        /// <summary>
        /// Initialize new instance of the data structure
        /// </summary>
        /// <param name="n">Name of the parameters</param>
        /// <param name="a">Actual name of the member</param>
        /// <param name="t">System Type of the memeber</param>
        public InteropParamInfo(string n, string a, Type t)
        {
            name = n;
            actualname = a;
            type = t;
        }
    }

    /// <summary>
    /// Data structure containing info on a member contained within a type
    /// </summary>
    public struct InteropMemberInfo
    {
        public InteropMemberType type;
        public string name;
        public bool accessmod;

        public InteropTypeInfo returnType;
        public InteropParamInfo[] paramTypes;

        /// <summary>
        /// Initialize new instance of the data structure
        /// </summary>
        /// <param name="t">The identified type of the member (method or property)</param>
        /// <param name="n">Name of the member</param>
        /// <param name="a">Access modifier of the member</param>
        /// <param name="r">Type of the return object</param>
        /// <param name="p">Types of the parameters, if any</param>
        public InteropMemberInfo(InteropMemberType t, string n, bool a, InteropTypeInfo r, InteropParamInfo[] p)
        {
            type = t;
            name = n;
            accessmod = a;
            returnType = r;
            paramTypes = p;
        }
    }
}
