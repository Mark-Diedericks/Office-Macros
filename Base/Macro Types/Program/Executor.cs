using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Executor
    {
        private Action<Action> InvokeExecute;

        public Executor(Action<Action> callback)
        {
            InvokeExecute = callback;
        }

        public void ExecuteAction(Action a)
        {
            InvokeExecute?.Invoke(a);
        }
    }
}
