using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Engine
{
    public interface IExecutionEngineIO
    {
        IExecutionEngineIO SetStreams(TextWriter output = null, TextWriter error = null, TextReader input = null);
        void ClearAllStreams();

        void SetInput(TextReader input);
        TextReader GetInput();

        TextWriter GetOutput();
        TextWriter GetError();
    }

}
