using Macro_Engine;
using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Python_Engine.Engine
{
    public class ExecutionEngine : IExecutionEngine
    {
        private readonly PythonEngine m_Engine;
        protected PythonEngine GetEngine()
        {
            return m_Engine;
        }
        
        private ExecutionEngine(PythonEngine engine)
        {
            m_Engine = engine;
        }

        public static ExecutionEngine Instantiate(EngineBase engine)
        {
            throw new NotImplementedException();
        }

        public IExecutionEngine GetDebugEngine()
        {
            throw new NotImplementedException();
        }

        public IExecutionEngine GetReleaseEngine()
        {
            throw new NotImplementedException();
        }

        public bool ExecuteMacro(string source, Action OnCompleted, bool async)
        {
            throw new NotImplementedException();
        }

        public void TerminateExecution()
        {
            throw new NotImplementedException();
        }



        private void ExecuteSourceAsynchronous(string source, Action OnCompleted)
        { }

        private void ExecuteSourceSynchronous(string source, Action OnCompleted)
        { }

        private void ExecuteSource(string source)
        { }
    }
}
