using Macro_Engine.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Macro_Engine
{
    public class HostState
    {
        public string[] Workspaces { get; }
        public string ActiveMacro { get; }
        public AssemblyDeclaration[] Assemblies { get; }

        public HostState(string[] workspaces, string activeMacro, AssemblyDeclaration[] assemblies)
        {
            Workspaces = workspaces;
            ActiveMacro = activeMacro;
            Assemblies = assemblies;
        }

        public HostState() : this(new string[] { }, "", new Macro_Engine.Interop.AssemblyDeclaration[] { })
        { }
    }
}
