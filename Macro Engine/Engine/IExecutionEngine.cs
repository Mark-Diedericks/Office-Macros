using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Engine
{
    public interface IExecutionEngine
    {
        IExecutionEngine Instantiate(EngineBase engine);

        IExecutionEngine GetDebugEngine();
        IExecutionEngine GetReleaseEngine();

        bool ExecuteMacro(string source, Action OnCompleted, bool async);
        void TerminateExecution();

    }
}
