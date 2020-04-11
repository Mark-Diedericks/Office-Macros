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
using System.IO;

namespace IronPython_Engine
{
    [Export(typeof(IExecutionEngine))]
    [ExportMetadata("Language", "IronPython")]
    [ExportMetadata("Runtime", "IronPython 2.7.9.0")]
    [ExportMetadata("FileExt", ".ipy")]
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly string Runtime = "IronPython 2.7.9.0";

        private IExecutionEngineIO m_IOManager;

        private Dictionary<string, object> m_ScopeValues;
        private HashSet<AssemblyDeclaration> m_Assemblies;

        private HashSet<ExecutionSession> m_Sessions;

        #region Instantiation

        private ExecutionEngine() { }

        public void Initialize()
        {
            m_ScopeValues = new Dictionary<string, object>();
            m_Assemblies = new HashSet<AssemblyDeclaration>();
            m_Sessions = new HashSet<ExecutionSession>();
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
        }

        public string GetLabel()
        {
            return Runtime;
        }

        #endregion

        #region Context/Scope

        public void SetValue(string name, object value)
        {
            if (m_ScopeValues == null)
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
        /// <param name="filepath">Source code (python)</param>
        /// <param name="async">If the code should be run asynchronously or not (synchronous)</param>
        /// <param name="OnComplete">The action to be called once the code has been executed</param>
        public void ExecuteMacro(string filepath, bool async, Action OnComplete)
        {
            m_IOManager.ClearAllStreams();

            FileInfo file = new FileInfo(filepath);

            if (!file.Exists)
            {
                m_IOManager.GetError().WriteLine("File does not exist: " + file.FullName);
                return;
            }

            string filename = file.FullName;
            string directory = file.Directory.FullName;


            if (async)
            {
                Task.Run(() => 
                {
                    ExecutionSession session = new ExecutionSession(m_IOManager, m_ScopeValues, m_Assemblies);
                    m_Sessions.Add(session);

                    session.Execute(filename, directory);
                    OnComplete?.Invoke();
                });
            }
            else
            {
                Events.InvokeEvent("OnHostExecute", new Action(() =>
                {
                    ExecutionSession session = new ExecutionSession(m_IOManager, m_ScopeValues, m_Assemblies);
                    m_Sessions.Add(session);

                    session.Execute(filename, directory);
                    OnComplete?.Invoke();
                }));
            }
        }

        /// <summary>
        /// End currently active asynchronous execution, if any
        /// </summary>
        public void TerminateExecution()
        {
            foreach (ExecutionSession session in m_Sessions)
                    session.InterruptScript();
        }

        #endregion
    }
}