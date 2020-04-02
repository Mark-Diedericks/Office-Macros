using Macro_Engine;
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
    [ExportMetadata("Runtime", "PythonNET 3.5.0")]
    [ExportMetadata("FileExt", ".py")]
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly string Runtime = "PythonNET 3.5.0";

        private BackgroundWorker m_BackgroundWorker;
        private bool m_IsExecuting;

        private IExecutionEngineIO m_IOManager;

        private dynamic CLR;
        private IntPtr ThreadPtr; 

        private ExecutionEngine() { }

        public void Initialize()
        {
            //TODO CHANGE HOME AND PATH
            //string py_home = @"D:\Mark Diedericks\Documents\Visual Studio 2019\Projects\Office Python\Dependencies\Python35\";

            string codeBase = System.Reflection.Assembly.GetAssembly(typeof(ExecutionEngine)).CodeBase;
            string path = new FileInfo(Uri.UnescapeDataString(new UriBuilder(codeBase).Path)).Directory.FullName;
            string solution = Path.GetFullPath(path + @"\..\..\..\..\..\..\");


            string py_home = solution + @"Dependencies\Python35\";
            string py_path = py_home + @"Scripts;" + py_home + @"lib;" + py_home + @"lib\site-packages";
            
            Environment.SetEnvironmentVariable("PATH", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", py_path, EnvironmentVariableTarget.Process);

            PythonEngine.PythonHome = py_home;
            PythonEngine.PythonPath = py_path;

            System.Diagnostics.Debug.WriteLine("CREATE  " + Thread.CurrentThread.ManagedThreadId);
            PythonEngine.Initialize();
            ThreadPtr = PythonEngine.BeginAllowThreads();

            using (Py.GIL())
                CLR = PythonEngine.ImportModule("clr");

            m_IsExecuting = false;
            m_BackgroundWorker = new BackgroundWorker();
            m_BackgroundWorker.WorkerSupportsCancellation = true;

            m_IOManager = null;
        }

        public void SetIO(IExecutionEngineIO manager)
        {

            m_IOManager = manager;

            using(Py.GIL())
            {
                dynamic sys = PythonEngine.ImportModule("sys");
                sys.stdout = new PythonOutput();
                sys.stderr = new PythonError();
                sys.stdin = new PythonInput();
            }

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

            System.Diagnostics.Debug.WriteLine("DESTROY  " + Thread.CurrentThread.ManagedThreadId);
            PythonEngine.EndAllowThreads(ThreadPtr);
            PythonEngine.Shutdown();
        }

        public string GetLabel()
        {
            return Runtime;
        }

        public string GetVersion()
        {
            string v = "";
            using(Py.GIL())
                v = PythonEngine.Version.Split(' ').FirstOrDefault<string>();

            return "Python " + v;
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
            /*object temp;
            if (!m_ScriptScope.TryGetVariable("Utils", out temp))
            {
                m_ScriptScope.Set("Utils", Utilities.GetInstance());
                m_ScriptScope.Set("Application", Utilities.GetInstance().GetApplication());
                m_ScriptScope.Set("ActiveWorkbook", Utilities.GetInstance().GetActiveWorkbook());
                m_ScriptScope.Set("ActiveWorksheet", Utilities.GetInstance().GetActiveWorksheet());
                m_ScriptScope.Set("MissingType", Type.Missing);
            }*/

            if (m_IOManager != null)
                m_IOManager.ClearAllStreams();

            try
            {
                using(Py.GIL())
                {
                    PythonEngine.Exec(source);
                }
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