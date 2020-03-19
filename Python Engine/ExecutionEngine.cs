﻿using Macro_Engine;
using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Reflection;
using System.IO;

namespace Python_Engine
{
    [Export(typeof(IExecutionEngine))]
    [ExportMetadata("Language", "Python")]
    [ExportMetadata("FileExt", ".py")]
    public class ExecutionEngine : IExecutionEngine
    {
        private BackgroundWorker m_BackgroundWorker;
        private bool m_IsExecuting;

        private ExecutionEngine()
        {
            string py_home = @"D:\Mark Diedericks\Documents\Visual Studio 2019\Projects\Office Python\Dependencies\Python35\";
            string py_path = py_home + @"\Scripts;" + py_home + @"\lib;" + py_home + @"lib\site-packages";

            Environment.SetEnvironmentVariable("PATH", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", py_path, EnvironmentVariableTarget.Process);

            PythonEngine.PythonHome = py_home;
            PythonEngine.PythonPath = py_path;

            PythonEngine.Initialize();

            m_IsExecuting = false;
            m_BackgroundWorker = new BackgroundWorker();
            m_BackgroundWorker.WorkerSupportsCancellation = true;

            //Reset IO streams of ScriptEngine if they're changed
            EventManager.GetInstance().OnIOChanged += () =>
            {
                /*m_ScriptEngine.Runtime.IO.RedirectToConsole();
                Console.SetOut(MacroEngine.GetEngineIOManager("Python").GetOutput());
                Console.SetError(MacroEngine.GetEngineIOManager("Python").GetError());*/
            };

            //End running tasks if program is exiting
            EventManager.GetInstance().OnDestroyed += delegate ()
            {
                if (m_BackgroundWorker != null)
                    m_BackgroundWorker.CancelAsync();

                PythonEngine.Shutdown();
            };
        }

        public string GetLabel()
        {
            using(Py.GIL())
            {
                return "Python " + PythonEngine.Version;
            }
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
                /*if (MacroEngine.GetEngineIOManager() != null)
                {
                    MacroEngine.GetEngineIOManager().GetOutput().WriteLine("Asynchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    MacroEngine.GetEngineIOManager().GetOutput().Flush();
                }*/

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

                /*if (MacroEngine.GetEngineIOManager() != null)
                {
                    MacroEngine.GetEngineIOManager().GetOutput().WriteLine("Synchronous Execution Completed. Runtime of {0:N2}s", Utilities.GetTimeIntervalSeconds(profileID));
                    MacroEngine.GetEngineIOManager().GetOutput().Flush();
                }*/

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
            }

            if (Main.GetEngineIOManager() != null)
                Main.GetEngineIOManager().ClearAllStreams();
                */
            try
            {
                using(Py.GIL())
                {
                    PythonEngine.Exec(source);
                    System.Diagnostics.Debug.WriteLine(">>>> >>>> >>>> >>>> Executed successfully");
                }
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