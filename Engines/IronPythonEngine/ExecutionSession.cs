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

        private readonly dynamic Engine;
        private readonly dynamic Scope;

        private readonly dynamic Trace;
        private readonly dynamic Source;

        public object ThreadLock { get; private set; } = new object();
        public bool Executing { get; private set; }

        private IExecutionEngineIO IOManager { get; }

        internal ExecutionSession(IExecutionEngineIO manager, Dictionary<string, object> Values, HashSet<AssemblyDeclaration> Assemblies)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["Debug"] = true;

            //IOManager
            IOManager = manager;

            // Create Engine
            Engine = IronPython.Hosting.Python.CreateEngine(args);
            Engine = Engine.CreateScriptSourceFromString("sys.settrace(isExecutionThreadAlive)");

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

            string trace = "import clr\nimport sys\n" +
                            "from System import *\n" +
                            "from System.Collections.Generic import *\n" +
                            "def isExecutionThreadAlive(frame, event, arg):\n" +
                            "   if not CheckAlive(frame, event, arg):\n" +
                            "       sys.exit()\n" +
                            "   return isExecutionThreadAlive\n";

            dynamic source = Engine.CreateScriptSourceFromString(trace);
            source.Execute(ScriptScope);

            ScriptScope.SetVariable("CheckAlive", new Func<dynamic, dynamic, dynamic, bool>(CheckAlive));

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

            return ScriptScope;
        }

        private bool CheckAlive(dynamic frame, dynamic e, dynamic arg)
        {
            bool isAlive;

            lock(ThreadLock)
            {
                isAlive = Executing;
            }

            return isAlive;

        }

        #endregion

        #region Execute & Interrupt

        public void InterruptScript()
        {
            lock(ThreadLock)
            {
                Executing = false;
            }
        }

        public bool Execute(string filePath, string directory)
        {
            int ProfileID = Utilities.BeginProfileSession();
            Executing = true;

            try
            {
                dynamic addPath = Engine.CreateScriptSourceFromString("sys.path.append(r\"" + directory + "\")\n");
                dynamic script = Engine.CreateScriptSourceFromFile(filePath);

                addPath.Execute(Scope);
                script.Execute(Scope);

                IOManager?.GetOutput()?.WriteLine("Synchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(ProfileID));
                IOManager?.GetOutput()?.Flush();

                Executing = false;
                Utilities.EndProfileSession(ProfileID);
            }
            catch (Exception ex)
            {
                Executing = false;
                Utilities.EndProfileSession(ProfileID);

                IOManager?.GetError()?.WriteLine(ex.Message);
                IOManager?.GetError()?.WriteLine(ex.StackTrace);

                IOManager?.GetOutput()?.Flush();
                return false;
            }

            return true;
        }

        #endregion
    }
}
