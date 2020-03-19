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
    [ExportMetadata("FileExt", ".ipy")]
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly string Language = "IronPython";
        private ScriptEngine m_ScriptEngine;
        private ScriptScope m_ScriptScope;

        private BackgroundWorker m_BackgroundWorker;
        private bool m_IsExecuting;

        private ExecutionEngine()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["Debug"] = true;

            m_ScriptEngine = IronPython.Hosting.Python.CreateEngine(args);
            m_ScriptScope = m_ScriptEngine.CreateScope();

            m_IsExecuting = false;
            m_BackgroundWorker = new BackgroundWorker();
            m_BackgroundWorker.WorkerSupportsCancellation = true;

            //Reset IO streams of ScriptEngine if they're changed
            EventManager.GetInstance().OnIOChanged += () =>
            {
                m_ScriptEngine.Runtime.IO.RedirectToConsole();
                Console.SetOut(MacroEngine.GetEngineIOManager("Python").GetOutput());
                Console.SetError(MacroEngine.GetEngineIOManager("Python").GetError());
            };

            //End running tasks if program is exiting
            EventManager.GetInstance().OnDestroyed += delegate ()
            {
                if (m_BackgroundWorker != null)
                    m_BackgroundWorker.CancelAsync();
            };
        }

        public string GetLabel()
        {
            return "Python " + m_ScriptEngine.LanguageVersion.ToString();
        }
        public void ClearContext()
        {

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
                if (MacroEngine.GetEngineIOManager(Language) != null)
                {
                    MacroEngine.GetEngineIOManager(Language).GetOutput().WriteLine("Asynchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    MacroEngine.GetEngineIOManager(Language).GetOutput().Flush();
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
            MacroEngine.GetHostDispatcher().BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                int profileID = -1;
                profileID = Utilities.BeginProfileSession();

                m_IsExecuting = true;
                ExecuteSource(source);

                if (MacroEngine.GetEngineIOManager(Language) != null)
                {
                    MacroEngine.GetEngineIOManager(Language).GetOutput().WriteLine("Synchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    MacroEngine.GetEngineIOManager(Language).GetOutput().Flush();
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

            if (MacroEngine.GetEngineIOManager(Language) != null)
                MacroEngine.GetEngineIOManager(Language).ClearAllStreams();
                
            try
            {
                m_ScriptEngine.Execute(source, m_ScriptScope);
            }
            catch (ThreadAbortException tae)
            {
                System.Diagnostics.Debug.WriteLine("Execution Error: " + tae.Message);

                /*if (Main.GetEngineIOManager() != null)
                {
                    Main.GetEngineIOManager().GetOutput().WriteLine("Thread Exited With Exception State {0}", tae.ExceptionState);
                    Main.GetEngineIOManager().GetOutput().Flush();
                }*/
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Execution Error: " + e.Message);

                /*if (Main.GetEngineIOManager() != null)
                {
                    Main.GetEngineIOManager().GetError().WriteLine("Execution Error: " + e.Message);
                    Main.GetEngineIOManager().GetOutput().Flush();
                }*/
            }
        }

        #endregion
    }
}