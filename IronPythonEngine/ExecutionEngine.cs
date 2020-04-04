using Macro_Engine;
using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Reflection;
using Microsoft.Scripting.Hosting;
using Macro_Engine.Interop;

namespace IronPython_Engine
{
    [Export(typeof(IExecutionEngine))]
    [ExportMetadata("Language", "IronPython")]
    [ExportMetadata("Runtime", "IronPython 2.7.9.0")]
    [ExportMetadata("FileExt", ".ipy")]
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly string Runtime = "IronPython 2.7.9.0";

        private ScriptEngine m_ScriptEngine;
        private ScriptScope m_ScriptScope;

        private Thread m_ExecutionThread;

        private bool m_IsExecuting;

        private IExecutionEngineIO m_IOManager;

        private Dictionary<string, object> m_ScopeValues;
        private HashSet<AssemblyDeclaration> m_Assemblies;

        #region Instantiation

        private ExecutionEngine() { }

        public void Initialize()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["Debug"] = true;

            m_ScriptEngine = IronPython.Hosting.Python.CreateEngine(args);
            m_ScriptScope = m_ScriptEngine.CreateScope();

            m_IsExecuting = false;

            m_ExecutionThread = null;

            m_IOManager = null;

            SetValue("RUNTIME", Runtime);
        }

        public void Destroy()
        {
            TerminateExecution();
        }

        #endregion

        #region IO and Info

        public void SetIO(IExecutionEngineIO manager)
        {
            m_IOManager = manager;

            m_ScriptEngine.Runtime.IO.RedirectToConsole();

            if (m_IOManager.GetOutput() != null)
                Console.SetOut(m_IOManager.GetOutput());

            if (m_IOManager.GetError() != null)
                Console.SetError(m_IOManager.GetError());

            if (m_IOManager.GetInput() != null)
                Console.SetIn(m_IOManager.GetInput());
        }

        public string GetLabel()
        {
            return Runtime;
        }

        public string GetVersion()
        {
            return "Python " + m_ScriptEngine.LanguageVersion.ToString();
        }

        #endregion

        #region Context/Scope

        public void ClearContext()
        {
            m_ScriptScope = m_ScriptEngine.CreateScope();

            if (m_ScopeValues != null)
                foreach (string name in m_ScopeValues.Keys)
                    m_ScriptScope.SetVariable(name, m_ScopeValues[name]);

            foreach(AssemblyDeclaration ad in m_Assemblies)
            {
                string code = "import clr\nimport sys\n" +
                                "sys.path.append(r\"" + ad.Location + "\")\n" +
                                "clr.AddReference(\"" + ad.Name + "\")\n";
                ExecuteSource(code);
            }
        }
        public void SetValue(string name, object value)
        {
            if (m_ScopeValues == null)
                m_ScopeValues = new Dictionary<string, object>();

            if (m_ScopeValues.ContainsKey(name))
                m_ScopeValues[name] = value;
            else
                m_ScopeValues.Add(name, value);

            if (m_ScriptScope != null)
                m_ScriptScope.SetVariable(name, value);
        }

        public void RemoveValue(string name)
        {
            if (m_ScopeValues != null)
                if (m_ScopeValues.ContainsKey(name))
                    m_ScopeValues.Remove(name);

            if (m_ScriptScope != null)
                m_ScriptScope.RemoveVariable(name);
        }

        #endregion

        #region Assemblies

        public void AddAssembly(AssemblyDeclaration ad)
        {
            if (m_Assemblies == null)
                m_Assemblies = new HashSet<AssemblyDeclaration>();

            m_Assemblies.Add(ad);

            string code =   "import clr\nimport sys\n" +
                            "sys.path.append(r\"" + ad.Location + "\")\n"+
                            "clr.AddReference(\"" + ad.Name + "\")\n";
            ExecuteSource(code);
        }
        public void RemoveAssembly(AssemblyDeclaration ad)
        {
            if (m_Assemblies == null)
                return;

            m_Assemblies.Remove(ad);
        }

        #endregion

        #region Execution

        /// <summary>
        /// Determines how to execute a macro
        /// </summary>
        /// <param name="source">Source code (python)</param>
        /// <param name="OnCompletedAction">The action to be called once the code has been executed</param>
        /// <param name="async">If the code should be run asynchronously or not (synchronous)</param>
        /// <returns></returns>
        public bool ExecuteMacro(string source, Action OnCompletedAction, bool async)
        {
            if (m_IsExecuting)
                return false;

            TerminateExecution();

            if (async)
                ExecuteSourceAsynchronous(source, OnCompletedAction);
            else
                ExecuteSourceSynchronous(source, OnCompletedAction);

            return true;
        }

        /// <summary>
        /// End currently active asynchronous execution, if any
        /// </summary>
        public void TerminateExecution()
        {
            if (m_ExecutionThread != null)
            {
                try
                {
                    m_ExecutionThread.Interrupt();

                    if (!m_ExecutionThread.Join(500))
                        m_ExecutionThread.Abort();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            m_ExecutionThread = null;
            m_IsExecuting = false;
        }

        /// <summary>
        /// Execute code asynchronously
        /// </summary>
        /// <param name="source">Source code (python)</param>
        /// <param name="OnCompletedAction">The action to be called once the code has been executed</param>
        private void ExecuteSourceAsynchronous(string source, Action OnCompletedAction)
        {
            m_IsExecuting = true;
            int profileID = -1;

            m_ExecutionThread = new Thread(() =>
            {
                profileID = Utilities.BeginProfileSession();
                ExecuteSource(source);

                if (m_IOManager != null)
                {
                    m_IOManager.GetOutput().WriteLine("Asynchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    m_IOManager.GetOutput().Flush();
                }

                Utilities.EndProfileSession(profileID);

                m_IsExecuting = false;
                OnCompletedAction?.Invoke();
            });

            m_ExecutionThread.SetApartmentState(ApartmentState.STA);
            m_ExecutionThread.IsBackground = true;
            m_ExecutionThread.Start();
        }

        /// <summary>
        /// Execute code synchronously
        /// </summary>
        /// <param name="source">Source code (python)</param>
        /// <param name="OnCompletedAction">The action to be called once the code has been executed</param>
        private void ExecuteSourceSynchronous(string source, Action OnCompletedAction)
        {
            Events.InvokeEvent("OnHostExecute", new Action(() =>
            {
                int profileID = -1;
                profileID = Utilities.BeginProfileSession();

                m_IsExecuting = true;
                ExecuteSource(source);

                if (m_IOManager != null)
                {
                    m_IOManager.GetOutput().WriteLine("Synchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    m_IOManager.GetOutput().Flush();
                }

                Utilities.EndProfileSession(profileID);

                m_IsExecuting = false;
                OnCompletedAction?.Invoke();
            }));
        }

        /// <summary>
        /// Execute source code through IronPython Script Engine
        /// </summary>
        /// <param name="source">Source code (python)</param>
        private void ExecuteSource(string source)
        {
            //m_ScriptScope.SetVariable("RUNTIME", Runtime);
            //m_ScriptScope.SetVariable("Utils", Utilities.GetExcelUtilities());
            //m_ScriptScope.SetVariable("Application", Utilities.GetInstance().GetApplication());
            //m_ScriptScope.SetVariable("ActiveWorkbook", Utilities.GetInstance().GetActiveWorkbook());
            //m_ScriptScope.SetVariable("ActiveWorksheet", Utilities.GetInstance().GetActiveWorksheet());
            //m_ScriptScope.SetVariable("MissingType", Type.Missing);

            if (m_IOManager != null)
                m_IOManager.ClearAllStreams();
                
            try
            {
                m_ScriptEngine.Execute(source, m_ScriptScope);
            }
            catch (ThreadAbortException tae)
            {
                System.Diagnostics.Debug.WriteLine("Execution Error: " + tae.Message);

                if (m_IOManager != null)
                {
                    m_IOManager.GetOutput().WriteLine("Thread Exited", tae.ExceptionState);
                    m_IOManager.GetOutput().Flush();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);

                if (m_IOManager != null)
                {
                    m_IOManager.GetError().WriteLine(e.Message);
                    m_IOManager.GetError().WriteLine(e.StackTrace);
                    m_IOManager.GetOutput().Flush();
                }
            }
        }

        #endregion
    }
}