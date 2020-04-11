using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Macros
{
    public class Macro : IMacro
    {
        public string Language { get; set; }
        public Guid ID { get; set; }
        public string Source { get; set; }

        /// <summary>
        /// Intialize new instance of TextualMacro
        /// </summary>
        /// <param name="source">The source code of the macro (python)</param>
        public Macro(string lang, string source)
        {
            Language = lang;
            Source = source;
            ID = Guid.Empty;
        }

        /// <summary>
        /// Set the source code (python) to be blank
        /// </summary>
        public void CreateBlankMacro()
        {
            Source = "";
        }

        /// <summary>
        /// Rename the macro
        /// </summary>
        /// <param name="name">New name of the macro</param>
        public void Rename(string name)
        {
            FileManager.RenameMacro(ID, name);

            if (MacroEngine.GetInstance().IsRibbonMacro(ID))
                MacroEngine.GetInstance().RenameRibbonMacro(ID);
        }

        /// <summary>
        /// Gets the name of the macro
        /// </summary>
        /// <returns>Name of the macro</returns>
        public string Name
        {
            get
            {
                return MacroEngine.GetInstance().GetDeclaration(ID).Name;
            }
        }

        /// <summary>
        /// Gets the relative file path of the macro
        /// </summary>
        /// <returns>Relative file path</returns>
        public string GetRelativePath()
        {
            return MacroEngine.GetInstance().GetDeclaration(ID).RelativePath;
        }

        /// <summary>
        /// Save the macro to it's respective file
        /// </summary>
        public void Save()
        {
            FileManager.SaveMacro(ID, Source);
        }

        /// <summary>
        /// Export the macro to a different file -> Save Copy As.
        /// </summary>
        public void Export()
        {
            FileManager.ExportMacro(ID, Source);
        }

        /// <summary>
        /// Delete the macro and it's respective file
        /// </summary>
        /// <param name="OnReturn">Action, containing the bool result, to be fired when the task is completed</param>
        public void Delete(Action<bool> OnReturn)
        {
            FileManager.DeleteMacro(ID, OnReturn);

            if (MacroEngine.GetInstance().IsRibbonMacro(ID))
                MacroEngine.GetInstance().RemoveRibbonMacro(ID);
        }

        /// <summary>
        /// Execute the macro using the source saved in macro
        /// </summary>
        /// <param name="async">Bool identifying if the macro should be execute asynchronously or not (synchronous)</param>
        /// <param name="OnComplete">Action to be fired on the tasks completetion</param>
        /// <param name="runtime">Runtime tag identifying which execution engine to use, if empty, a default will be chosen</param>
        public void Execute(bool async, Action OnComplete = null, string runtime = "")
        {
            Save();
            string filepath = Path.Combine(Files.MacroDirectory, MacroEngine.GetInstance().GetDeclaration(ID).RelativePath);

            if (string.IsNullOrEmpty(runtime))
                runtime = GetDefaultRuntime();

            IExecutionEngine engine = MacroEngine.GetInstance().GetExecutionEngine(runtime);

            if (engine != null)
                engine.ExecuteMacro(filepath, async, OnComplete);
            else
                MacroEngine.GetInstance().GetExecutionEngine(GetDefaultRuntime())?.ExecuteMacro(filepath, async, OnComplete);
        }

        /// <summary>
        /// Returns the default runtime for the macro's language
        /// </summary>
        /// <returns>Default runtime</returns>
        public string GetDefaultRuntime()
        {
            return MacroEngine.GetInstance().GetRuntimes(Language).FirstOrDefault<string>();
        }

        /// <summary>
        /// Returns the default file extension for the macro's runtime
        /// </summary>
        /// <returns>Default file extension</returns>
        public string GetDefaultFileExtension()
        {
            return MacroEngine.GetInstance().GetFileExt(GetDefaultRuntime());
        }
    }
}
