using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class Executor
    {
        public Func<Func<bool>, Task<bool>> InvokeExecute { get; set; }

        public Executor() {}

        public async Task<bool> ExecuteAction(Func<bool> a)
        {
            return await InvokeExecute?.Invoke(a);
        }
    }
}
