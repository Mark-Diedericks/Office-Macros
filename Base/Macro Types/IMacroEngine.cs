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
using System.IO;

namespace Macro_Engine
{
    public interface IMacroEngine
    {
        void Instantiate(HostState state);
        void Destroy();

        //Runtimes
        HashSet<string> GetRuntimes(string language = "");

        //Declarations / Macros
        HashSet<FileDeclaration> GetFileDeclarations();
        FileDeclaration GetDeclarationFromFullname(string fullname);

        void AddFile(FileDeclaration md);
        void RemoveFile(FileDeclaration md);
        void RenameFile(FileDeclaration md, string newName);


        //Default file extension
        string GetDefaultFileExtension();

        //Assemblies
        HashSet<AssemblyDeclaration> GetAssemblies();
        void AddAssembly(AssemblyDeclaration declaration);
        void RemoveAssembly(AssemblyDeclaration declaration);

        //Active macro
        FileDeclaration GetActiveFile();
        void SetActiveFile(FileDeclaration md);

        //Folders
        HashSet<FileDeclaration> RenameFolder(DirectoryInfo info, string newName);
        Task<bool> DeleteFolder(DirectoryInfo info);

        //Execution engine variables
        void SetExecutionValue(string name, object value);
        void RemoveExecutionValue(string name);
    }
}
