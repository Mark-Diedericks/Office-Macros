using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Engine
{
    public class ExecutionEngineIO : IExecutionEngineIO
    {
        private TextReader Input;
        private TextWriter Output;
        private TextWriter Error;

        /// <summary>
        /// Initialize EngineIOManager
        /// </summary>
        public ExecutionEngineIO()
        {
            Input = null;
            Output = null;
            Error = null;
        }

        /// <summary>
        /// Set the streams of the IO manager, remains unchanged if parameter is null
        /// </summary>
        /// <param name="output">Output stream writer</param>
        /// <param name="error">Error stream writer</param>
        /// <param name="input">Input stream reader</param>
        /// <returns>Current instance of IExecutionEngineIO</returns>
        public IExecutionEngineIO SetStreams(TextWriter output = null, TextWriter error = null, TextReader input = null)
        {
            if (input != null)
                Input = input;

            if (output != null)
                Output = output;

            if (error != null)
                Error = error;

            return this;
        }

        /// <summary>
        /// Clear all TextWriters of data and clear the UI of displayed text
        /// </summary>
        public void ClearAllStreams()
        {
            Output.Flush();
            Error.Flush();

            //Events.ClearAllIO();
            Events.InvokeEvent("ClearAllIO");
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
