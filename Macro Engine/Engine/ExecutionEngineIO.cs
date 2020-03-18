using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Engine
{
    public class ExecutionEngineIO
    {
        private readonly EngineBase m_Engine;
        protected EngineBase GetEngine()
        {
            return m_Engine;
        }

        private TextReader Input;
        private readonly TextWriter Output;
        private readonly TextWriter Error;

        /// <summary>
        /// Initialize EngineIOManager
        /// </summary>
        /// <param name="output">Console Output TextWriter</param>
        /// <param name="error">Console Error TextWriter</param>
        public ExecutionEngineIO(EngineBase engine, TextWriter output, TextWriter error)
        {
            m_Engine = engine;
            Input = null;
            Output = output;
            Error = error;
        }

        /// <summary>
        /// Clear all TextWriters of data and clear the UI of displayed text
        /// </summary>
        public void ClearAllStreams()
        {
            Output.Flush();
            Error.Flush();

            GetEngine().GetEventManager().ClearAllIO();
        }

        /// <summary>
        /// Sets the input TextReader for console inputs
        /// </summary>
        /// <param name="input">Input TextReader</param>
        public void SetInput(TextReader input)
        {
            Input = input;
        }

        /// <summary>
        /// Get the TextWriter object for console outputs
        /// </summary>
        /// <returns>Input TextReader</returns>
        public TextReader GetInput()
        {
            return Input;
        }

        /// <summary>
        /// Get the TextWriter object for console outputs
        /// </summary>
        /// <returns>Output TextWriter</returns>
        public TextWriter GetOutput()
        {
            return Output;
        }

        /// <summary>
        /// Get the TextWriter object for console errors
        /// </summary>
        /// <returns>Error TextWriter</returns>
        public TextWriter GetError()
        {
            return Error;
        }

    }
}
