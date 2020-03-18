/* 
 * Mark Diedericks
 * 18/03/2020
 * Entry point of python engine
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Macro_Engine;
using Macro_Engine.Interop;
using Python_Engine.Engine;

namespace Python_Engine
{
    public class PythonEngine : EngineBase
    {

        private PythonEngine(object application, Dispatcher dispatcher) : base(application, dispatcher, ExecutionEngine.Instantiate())
        {

        }

        public static PythonEngine Instantiate(object application, Dispatcher dispatcher, string ribbonMacros, string activeMacroRelativePath, AssemblyDeclaration[] assemblies, Action OnLoaded)
        {

            return new PythonEngine(application, dispatcher);
        }

    }
}
