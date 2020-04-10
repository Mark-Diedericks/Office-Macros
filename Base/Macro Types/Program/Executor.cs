using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Executor
    {
        private Func<Action, Task> InvokeExecute;

        public Executor(Func<Action, Task> callback)
        {
            InvokeExecute = callback;
        }

        public Task ExecuteAction(Action a)
        {
            return InvokeExecute?.Invoke(a);
        }
    }
}
