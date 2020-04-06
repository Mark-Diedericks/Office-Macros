using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Macros
{
    public interface IMacro
    {
        string Language { get; set; }
        string Name { get; }
        Guid ID { get; set; }
        string Source { get; set; }

        void CreateBlankMacro();

        void Rename(string name);
        void Save();
        void Export();
        void Delete(Action<bool> OnReturn);

        void Execute(Action OnCompleted, bool async, string runtime = "");
        void ExecuteSource(Action OnCompleted, string source, bool async, string runtime = "");

        string GetRelativePath();
        string GetDefaultRuntime();
        string GetDefaultFileExtension();
    }
}
