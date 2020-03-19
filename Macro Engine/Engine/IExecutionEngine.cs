using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Engine
{
    public interface IExecutionEngine
    {
        string GetLabel();
        bool ExecuteMacro(string source, Action OnCompleted, bool async);
        void TerminateExecution();

    }

    public interface IExecutionEngineData
    {
        string Language { get; }
        string FileExt { get; }
    }

}
