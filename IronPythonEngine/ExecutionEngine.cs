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

        private BackgroundWorker m_BackgroundWorker;
        private bool m_IsExecuting;

        private IExecutionEngineIO m_IOManager;

        private ExecutionEngine()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["Debug"] = true;

            m_ScriptEngine = IronPython.Hosting.Python.CreateEngine(args);
            m_ScriptScope = m_ScriptEngine.CreateScope();

            m_IsExecuting = false;

            m_BackgroundWorker = new BackgroundWorker();
            m_BackgroundWorker.WorkerSupportsCancellation = true;

            m_IOManager = null;
        }

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

        public void Destroy()
        {
            if (m_BackgroundWorker != null)
                m_BackgroundWorker.CancelAsync();
        }

        public string GetLabel()
        {
            return Runtime;
        }

        public string GetVersion()
        {
            return "Python " + m_ScriptEngine.LanguageVersion.ToString();
        }

        public void ClearContext()
        {
            m_ScriptScope = m_ScriptEngine.CreateScope();
        }

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
            if (m_BackgroundWorker != null)
                m_BackgroundWorker.CancelAsync();

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
            m_BackgroundWorker = new BackgroundWorker();
            m_BackgroundWorker.WorkerSupportsCancellation = true;
            int profileID = -1;

            m_BackgroundWorker.RunWorkerCompleted += (s, args) =>
            {
                if (m_IOManager != null)
                {
                    m_IOManager.GetOutput().WriteLine("Asynchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    m_IOManager.GetOutput().Flush();
                }

                Utilities.EndProfileSession(profileID);

                m_IsExecuting = false;
                OnCompletedAction?.Invoke();
            };

            m_BackgroundWorker.DoWork += (s, args) =>
            {
                profileID = Utilities.BeginProfileSession();
                ExecuteSource(source);
            };

            m_BackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Execute code synchronously
        /// </summary>
        /// <param name="source">Source code (python)</param>
        /// <param name="OnCompletedAction">The action to be called once the code has been executed</param>
        private void ExecuteSourceSynchronous(string source, Action OnCompletedAction)
        {
            Events.InvokeEvent("OnHostExecute", DispatcherPriority.Normal, new Action(() =>
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
            /*object temp;
            if (!m_ScriptScope.TryGetVariable("Utils", out temp))
            {
                m_ScriptScope.SetVariable("Utils", Utilities.GetExcelUtilities());
                m_ScriptScope.SetVariable("Application", Utilities.GetInstance().GetApplication());
                m_ScriptScope.SetVariable("ActiveWorkbook", Utilities.GetInstance().GetActiveWorkbook());
                m_ScriptScope.SetVariable("ActiveWorksheet", Utilities.GetInstance().GetActiveWorksheet());
                m_ScriptScope.SetVariable("MissingType", Type.Missing);
            }*/

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
                    m_IOManager.GetOutput().WriteLine("Thread Exited With Exception State {0}", tae.ExceptionState);
                    m_IOManager.GetOutput().Flush();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Execution Error: " + e.Message);

                if (m_IOManager != null)
                {
                    m_IOManager.GetError().WriteLine("Execution Error: " + e.Message);
                    m_IOManager.GetOutput().Flush();
                }
            }
        }

        #endregion
    }
}