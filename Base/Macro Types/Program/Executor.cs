using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Executor
    {
        public Action<Action> InvokeExecute { get; set; }

        public Executor() {}

        public void ExecuteAction(Action a)
        {
            InvokeExecute?.Invoke(a);
        }
    }
}
