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
using Macro_Engine.Interop;

namespace Python_Engine
{

    [Export(typeof(IExecutionEngine))]
    [ExportMetadata("Language", "Python")]
    [ExportMetadata("Runtime", "PythonNET 3.5.0")]
    [ExportMetadata("FileExt", ".py")]
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly string Runtime = "PythonNET 3.5.0";

        private Thread m_ExecutionThread;

        private bool m_IsExecuting;

        private IExecutionEngineIO m_IOManager;

        private PyScope m_ScriptScope;
        private dynamic SYS;
        private dynamic CLR;
        private IntPtr ThreadPtr;

        private Dictionary<string, object> m_ScopeValues;
        private HashSet<AssemblyDeclaration> m_Assemblies;

        #region Instantiation

        private ExecutionEngine() { }

        public void Initialize()
        {
            //python.exe -m pip install comtypes

            //TODO CHANGE HOME AND PATH
            //string py_home = @"C:\Users\markd\AppData\Local\Programs\Python\Python35";

            string codeBase = System.Reflection.Assembly.GetAssembly(typeof(ExecutionEngine)).CodeBase;
            string path = new FileInfo(Uri.UnescapeDataString(new UriBuilder(codeBase).Path)).Directory.FullName;
            string solution = Path.GetFullPath(path + @"\..\..\..\..\..\..\..\");


            string py_home = solution + @"Dependencies\Python35\";
            string py_path = py_home + @"Scripts;" + py_home + @"DLLs;" + py_home + @"lib;" + py_home + @"lib\site-packages";
            
            Environment.SetEnvironmentVariable("PATH", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", py_path, EnvironmentVariableTarget.Process);

            PythonEngine.PythonHome = py_home;
            PythonEngine.PythonPath = py_path;

            PythonEngine.Initialize();
            ThreadPtr = PythonEngine.BeginAllowThreads();

            using (Py.GIL())
            {
                SYS = PythonEngine.ImportModule("sys");
                CLR = PythonEngine.ImportModule("clr");
            }

            ClearContext();

            m_IsExecuting = false;

            m_ExecutionThread = null;

            m_IOManager = null;

            SetValue("RUNTIME", new PyString(Runtime));
        }

        public void Destroy()
        {
            TerminateExecution();

            using (Py.GIL())
                m_ScriptScope.Dispose();
            m_ScriptScope = null;

            PythonEngine.EndAllowThreads(ThreadPtr);
            PythonEngine.Shutdown();
        }

        #endregion

        #region IO and Info

        public void SetIO(IExecutionEngineIO manager)
        {

            m_IOManager = manager;

            using(Py.GIL())
            {
                SYS.stdout = new PythonOutput();
                SYS.stderr = new PythonError();
                SYS.stdin = new PythonInput();
            }

            ClearContext();

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
            string v = "";
            using(Py.GIL())
                v = PythonEngine.Version.Split(' ').FirstOrDefault<string>();

            return "Python " + v;
        }

        #endregion

        #region Context/Scope

        public void ClearContext()
        {
            using(Py.GIL())
            {
                if(m_ScriptScope != null)
                    m_ScriptScope.Dispose();

                m_ScriptScope = Py.CreateScope();

                if(m_ScopeValues != null)
                    foreach (string name in m_ScopeValues.Keys)
                        m_ScriptScope.Set(name, m_ScopeValues[name].ToPython());
            }
        }

        public void SetValue(string name, object value)
        {
            if(m_ScopeValues == null)
                m_ScopeValues = new Dictionary<string, object>();

            if (m_ScopeValues.ContainsKey(name))
                m_ScopeValues[name] = value;
            else
                m_ScopeValues.Add(name, value);

            using(Py.GIL())
            {
                if (m_ScriptScope != null)
                    m_ScriptScope.Set(name, value.ToPython());
            }
        }

        public void RemoveValue(string name)
        {
            if (m_ScopeValues != null)
                if (m_ScopeValues.ContainsKey(name))
                    m_ScopeValues.Remove(name);

            using (Py.GIL())
            {
                if (m_ScriptScope != null)
                    m_ScriptScope.Remove(name);
            }
        }

        #endregion

        #region Assemblies

        public void AddAssembly(AssemblyDeclaration ad)
        {
            if (m_Assemblies == null)
                m_Assemblies = new HashSet<AssemblyDeclaration>();

            m_Assemblies.Add(ad);

            using(Py.GIL())
            {
                SYS.path.append(ad.Location);
                CLR.AddReference(ad.Name);
            }
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
        /// <param name="filepath">Source code (python)</param>
        /// <param name="async">If the code should be run asynchronously or not (synchronous)</param>
        /// <param name="OnComplete">The action to be called once the code has been executed</param>
        public void ExecuteMacro(string filepath, bool async, Action OnComplete)
        {
            if (m_IsExecuting)
                return;

            TerminateExecution();
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
        /// Execute source code through PythonNET Script Engine
        /// </summary>
        /// <param name="source">Source code (python)</param>
        private void ExecuteSource(string source)
        {
        }

        #endregion
    }
}