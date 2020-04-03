﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Macro_Engine.Engine
{
    public interface IExecutionEngine
    {
        void Initialize();
        string GetLabel();
        string GetVersion();
        bool ExecuteMacro(string source, Action OnCompleted, bool async);
        void TerminateExecution();
        void Destroy();
        void ClearContext();
        void SetIO(IExecutionEngineIO manager);
        void SetValue(string name, object value);
        void RemoveValue(string name);
    }

    public interface IExecutionEngineData
    {
        string Language { get; }
        string Runtime { get; }
        string FileExt { get; }
    }

}
