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

        private IExecutionEngineIO m_IOManager;

        private IntPtr ThreadPtr;

        private Dictionary<string, object> m_ScopeValues;
        private HashSet<AssemblyDeclaration> m_Assemblies;
        private HashSet<ExecutionSession> m_Sessions;

        #region Instantiation

        private ExecutionEngine() { }

        public void Initialize()
        {
            //TODO CHANGE HOME AND PATH
            //string py_home = @"C:\Users\markd\AppData\Local\Programs\Python\Python35";

            /*string codeBase = System.Reflection.Assembly.GetAssembly(typeof(ExecutionEngine)).CodeBase;
            string path = new FileInfo(Uri.UnescapeDataString(new UriBuilder(codeBase).Path)).Directory.FullName;
            string solution = Path.GetFullPath(path + @"\..\..\..\..\..\..\..\");


            string py_home = solution + @"Dependencies\Python35\";
            string py_path = py_home + @"Scripts;" + py_home + @"DLLs;" + py_home + @"lib;" + py_home + @"lib\site-packages";
            
            Environment.SetEnvironmentVariable("PATH", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONHOME", py_home, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PYTHONPATH", py_path, EnvironmentVariableTarget.Process);

            PythonEngine.PythonHome = py_home;
            PythonEngine.PythonPath = py_path;*/

            PythonEngine.Initialize();
            ThreadPtr = PythonEngine.BeginAllowThreads();

            m_ScopeValues = new Dictionary<string, object>();
            m_Assemblies = new HashSet<AssemblyDeclaration>();
            m_Sessions = new HashSet<ExecutionSession>();

            m_IOManager = null;

            SetValue("RUNTIME", new PyString(Runtime));
        }

        public void Destroy()
        {
            TerminateExecution();

            using (Py.GIL())
                foreach (ExecutionSession session in m_Sessions)
                    session?.Destroy();

            PythonEngine.EndAllowThreads(ThreadPtr);
            PythonEngine.Shutdown();
        }

        #endregion

        #region IO and Info

        public void SetIO(IExecutionEngineIO manager)
        {
            m_IOManager = manager;
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

        public void SetValue(string name, object value)
        {
            if(m_ScopeValues == null)
                m_ScopeValues = new Dictionary<string, object>();

            if (m_ScopeValues.ContainsKey(name))
                m_ScopeValues[name] = value;
            else
                m_ScopeValues.Add(name, value);
        }

        public void RemoveValue(string name)
        {
            if (m_ScopeValues != null)
                if (m_ScopeValues.ContainsKey(name))
                    m_ScopeValues.Remove(name);
        }

        #endregion

        #region Assemblies

        public void AddAssembly(AssemblyDeclaration ad)
        {
            if (m_Assemblies == null)
                m_Assemblies = new HashSet<AssemblyDeclaration>();

            m_Assemblies.Add(ad);
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
        /// <param name="content">Source code (python)</param>
        /// <param name="async">If the code should be run asynchronously or not (synchronous)</param>
        public async Task<bool> ExecuteMacro(string content, bool async, string workspace = "")
        {
            TerminateExecution();
            m_IOManager?.ClearAllStreams();

            Func<bool> execute = new Func<bool>(() =>
            {
                ExecutionSession session = new ExecutionSession(m_IOManager, m_ScopeValues, m_Assemblies);
                m_Sessions.Add(session);

                bool result = session.Execute(content, workspace);

                m_Sessions.Remove(session);
                return result;
            });

            if (async)
                return await Task.Run(execute);
            else
                return await Events.ExecuteOnHost(execute);
                //Events.InvokeEvent("OnHostExecute", execute);
        }

        /// <summary>
        /// End currently active asynchronous execution, if any
        /// </summary>
        public void TerminateExecution()
        {
            foreach (ExecutionSession session in m_Sessions)
                session?.InterruptScript();
        }

        #endregion
    }
}