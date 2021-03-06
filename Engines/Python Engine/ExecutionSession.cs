﻿using Macro_Engine;
using Macro_Engine.Engine;
using Macro_Engine.Interop;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Python_Engine
{
    internal class ExecutionSession
    {
        private readonly string PythonNETNamespace = "Python.Runtime";

        private readonly PyScope Scope;

        public object ThreadLock { get; private set; } = new object();
        public bool Executing { get; private set; }
        public bool CanExecute { get; private set; }

        private IExecutionEngineIO IOManager { get; }

        internal ExecutionSession(IExecutionEngineIO manager, Dictionary<string, object> Values, HashSet<AssemblyDeclaration> Assemblies)
        {
            CanExecute = true;

            //IOManager
            IOManager = manager;

            // Create Scope
            Scope = InitializeScope(Values, Assemblies);

            if (Scope == null)
            {
                CanExecute = false;
                return;
            }

            // Redirect IO
            using(Py.GIL())
            {
                dynamic SYS = Scope.Import("sys");
                SYS.stdout = new PythonOutput();
                SYS.stderr = new PythonError();
                SYS.stdin = new PythonInput();
            }

            if (IOManager?.GetOutput() != null)
                Console.SetOut(IOManager.GetOutput());

            if (IOManager?.GetError() != null)
                Console.SetError(IOManager.GetError());

            if (IOManager?.GetInput() != null)
                Console.SetIn(IOManager.GetInput());
        }

        public void Destroy()
        {
            if (Scope != null)
                using(Py.GIL())
                    Scope.Dispose();
        }

        #region Scope Initialization

        private PyScope InitializeScope(Dictionary<string, object> Values, HashSet<AssemblyDeclaration> Assemblies)
        {
            PyScope ScriptScope;
            using (Py.GIL())
            {
                ScriptScope = Py.CreateScope();

                ScriptScope.Set("CheckAlive", new Func<bool>(CheckAlive));

                string execThreadAlive = "import clr\nimport sys\n" +
                                "from System import *\n" +
                                "from System.Collections.Generic import *\n"+
                                "def isExecutionThreadAlive(frame, event, arg):\n" +
                                "	if not CheckAlive():\n" +
                                "		sys.exit()\n" +
                                "	return isExecutionThreadAlive\n";

                ScriptScope.Exec(execThreadAlive);

                if (Values != null)
                    foreach (string name in Values.Keys)
                        ScriptScope.Set(name, Values[name].ToPython());

                if (Assemblies != null)
                    foreach (AssemblyDeclaration ad in Assemblies)
                        ScriptScope.Exec("sys.path.append(r\"" + ad.Location + "\")\n" +
                                            "clr.AddReference(\"" + ad.Name + "\")\n");

                ScriptScope.Exec("sys.settrace(isExecutionThreadAlive)\n");
            }

            return ScriptScope;
        }

        private bool CheckAlive()
        {
            bool isAlive;

            lock (ThreadLock)
            {
                isAlive = Executing && CanExecute;
            }

            return isAlive;

        }

        #endregion

        #region Execute & Interrupt

        public void InterruptScript()
        {
            lock (ThreadLock)
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
                using(Py.GIL())
                {
                    Scope.Exec("sys.settrace(isExecutionThreadAlive)\n");

                    if(!string.IsNullOrEmpty(directory))
                        Scope.Exec("sys.path.append(r\"" + directory + "\")\n");

                    Scope.Exec(content);
                }

                IOManager?.GetOutput()?.WriteLine("Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(ProfileID));
            }
            catch (Exception ex)
            {
                result = false;

                Type exType = ex.GetType();
                if (exType.Namespace.Contains(PythonNETNamespace))
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
