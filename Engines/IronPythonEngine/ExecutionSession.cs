using Macro_Engine;
using Macro_Engine.Engine;
using Macro_Engine.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IronPython_Engine
{
    internal class ExecutionSession
    {
        private readonly string IronPythonNamespace = "IronPython.Runtime";
        private readonly string ScriptingNamesapce = "Microsoft.Scripting";

        private readonly dynamic Engine;
        private readonly dynamic Scope;

        public object ThreadLock { get; private set; } = new object();
        public bool Executing { get; private set; }
        public bool CanExecute { get; private set; }

        private IExecutionEngineIO IOManager { get; }

        internal ExecutionSession(IExecutionEngineIO manager, Dictionary<string, object> Values, HashSet<AssemblyDeclaration> Assemblies)
        {
            CanExecute = true;

            Dictionary<string, object> args = new Dictionary<string, object>();
            args["Debug"] = true;

            //IOManager
            IOManager = manager;

            // Create Engine
            Engine = IronPython.Hosting.Python.CreateEngine(args);

            // Create Scope
            Scope = InitializeScope(Values, Assemblies);
            
            // Redirect IO
            Engine.Runtime.IO.RedirectToConsole();

            if (IOManager?.GetOutput() != null)
                Console.SetOut(IOManager.GetOutput());

            if (IOManager?.GetError() != null)
                Console.SetError(IOManager.GetError());

            if (IOManager?.GetInput() != null)
                Console.SetIn(IOManager.GetInput());
        }

        #region Scope Initialization

        private dynamic InitializeScope(Dictionary<string, object> Values, HashSet<AssemblyDeclaration> Assemblies)
        {
            dynamic ScriptScope = Engine.CreateScope();
            ScriptScope.SetVariable("CheckAlive", new Func<bool>(CheckAlive));

            string execThreadAlive = "import clr\nimport sys\n" +
                            "from System import *\n" +
                            "from System.Collections.Generic import *\n" +
                            "def isExecutionThreadAlive(frame, event, arg):\n" +
                            "   if not CheckAlive():\n" +
                            "       sys.exit()\n" +
                            "   return isExecutionThreadAlive\n";

            dynamic source = Engine.CreateScriptSourceFromString(execThreadAlive);
            source.Execute(ScriptScope);

            if (Values != null)
                foreach (string name in Values.Keys)
                    ScriptScope.SetVariable(name, Values[name]);

            if (Assemblies != null)
            {
                foreach (AssemblyDeclaration ad in Assemblies)
                {
                    dynamic assembly = Engine.CreateScriptSourceFromString( "sys.path.append(r\"" + ad.Location + "\")\n" +
                                                                            "clr.AddReference(\"" + ad.Name + "\")\n");
                    assembly.Execute(ScriptScope);
                }
            }

            dynamic trace = Engine.CreateScriptSourceFromString("sys.settrace(isExecutionThreadAlive)");
            trace.Execute(ScriptScope);

            return ScriptScope;
        }

        private bool CheckAlive()
        {
            bool isAlive;

            lock(ThreadLock)
            {
                isAlive = Executing && CanExecute;
            }

            return isAlive;

        }

        #endregion

        #region Execute & Interrupt

        public void InterruptScript()
        {
            lock(ThreadLock)
            {
                CanExecute = false;
                Executing = false;
            }
        }

        public bool Execute(string content, string directory)
        {
            if (!CanExecute)
                return false;

            int ProfileID = Utilities.BeginProfileSession();
            Executing = true;
            bool result = true;

            try
            {
                dynamic addPath = Engine.CreateScriptSourceFromString("sys.path.append(r\"" + directory + "\")\n");
                dynamic script = Engine.CreateScriptSourceFromString(content);

                if (!string.IsNullOrEmpty(directory))
                    addPath.Execute(Scope);

                script.Execute(Scope);

                IOManager?.GetOutput()?.WriteLine("Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(ProfileID));
            }
            catch (Exception ex)
            {
                result = false;

                Type exType = ex.GetType();
                if(exType.Namespace.Contains(IronPythonNamespace) || exType.Namespace.Contains(ScriptingNamesapce))
                {
                    IOManager?.GetError()?.WriteLine(ex.Message);
                    IOManager?.GetError()?.WriteLine(ex.StackTrace);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
            }

            IOManager?.GetOutput()?.Flush();
            Utilities.EndProfileSession(ProfileID);
            Executing = false;

            return result;
        }

        #endregion
    }
}
