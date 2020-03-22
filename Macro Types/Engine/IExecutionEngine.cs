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
        string GetVersion();
        bool ExecuteMacro(string source, Action OnCompleted, bool async);
        void TerminateExecution();
        void ClearContext();
    }

    public interface IExecutionEngineData
    {
        string Language { get; }
        string Runtime { get; }
        string FileExt { get; }
    }

}
