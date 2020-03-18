using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Macro_Engine;
using Macro_Engine.Macros;

namespace Python_Engine.Macros
{
    class PythonMacro : Macro
    {
        /// <summary>
        /// Intialize new instance of PythonMacro
        /// </summary>
        /// <param name="source">The source code of the macro (python)</param>
        public PythonMacro(EngineBase instance, string source) : base(instance, source)
        { }

        /// <summary>
        /// Set the source code (python) to be blank
        /// </summary>
        public override void CreateBlankMacro()
        {
            m_Source = "";
        }

        /// <summary>
        /// Get the ID of the macro
        /// </summary>
        /// <returns>Guid of the macro</returns>
        public override Guid GetID()
        {
            return m_ID;
        }

        /// <summary>
        /// Set the ID of the macro
        /// </summary>
        /// <param name="id">Guid to set</param>
        public override void SetID(Guid id)
        {
            m_ID = id;
        }

        /// <summary>
        /// Set the source of the macro
        /// </summary>
        /// <param name="source">Source code (python)</param>
        public override void SetSource(string source)
        {
            m_Source = source;
        }

        /// <summary>
        /// Get the source code of the macro
        /// </summary>
        /// <returns>Source code (python)</returns>
        public override string GetSource()
        {
            return m_Source;
        }

        /// <summary>
        /// Execute the macro using the Debug Execution Engine
        /// </summary>
        /// <param name="OnCompletedAction">Action to be fire when the task is completed</param>
        /// <param name="async">Bool identifying if the macro should be execute asynchronously or not (synchronous)</param>
        public override void ExecuteDebug(Action OnCompletedAction, bool async)
        {
            GetEngine().GetExecutionEngine().GetDebugEngine().ExecuteMacro(m_Source, OnCompletedAction, async);
        }

        /// <summary>
        /// Execute the macro using the Release Execution Engine
        /// </summary>
        /// <param name="OnCompletedAction">Action to be fire when the task is completed</param>
        /// <param name="async">Bool identifying if the macro should be execute asynchronously or not (synchronous)</param>
        public override void ExecuteRelease(Action OnCompletedAction, bool async)
        {
            GetEngine().GetExecutionEngine().GetReleaseEngine().ExecuteMacro(m_Source, OnCompletedAction, async);
        }
    }
}
