using Macro_Engine.Macros;
using Macro_Engine.Interop;
using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;

namespace Macro_Engine
{
    public interface IMacroEngine
    {
        CancellationTokenSource Instantiate(HostState state, Action OnCompleted);

        //Runtimes
        HashSet<string> GetRuntimes(string language = "");

        //Declarations
        Dictionary<Guid, MacroDeclaration> GetDeclarations();
        MacroDeclaration GetDeclaration(Guid id);
        void SetDeclaration(Guid id, MacroDeclaration md);

        //Macros
        Dictionary<Guid, IMacro> GetMacros();
        IMacro GetMacro(Guid id);
        Guid AddMacro(MacroDeclaration md, IMacro macro);
        void RemoveMacro(Guid id);
        void RenameMacro(Guid id, string newName);


        //RelativePath and FileExtension
        Guid GetIDFromRelativePath(string relativepath);
        string GetDefaultFileExtension();

        //Assemblies
        HashSet<AssemblyDeclaration> GetAssemblies();
        void AddAssembly(AssemblyDeclaration declaration);
        void RemoveAssembly(AssemblyDeclaration declaration);

        //Active macro
        Guid GetActiveMacro();
        void SetActiveMacro(Guid id);

        //Ribbon macros
        bool IsRibbonMacro(Guid id);
        void AddRibbonMacro(Guid id);
        void RemoveRibbonMacro(Guid id);

        //Folders
        HashSet<Guid> RenameFolder(string oldDir, string newDir);
        void DeleteFolder(string dir, Action<bool> OnReturn);
    }
}
