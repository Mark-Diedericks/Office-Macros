using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Macros
{
    public class Macro
    {
        public string Language { get; }
        private Guid m_ID;
        private string m_Source;

        /// <summary>
        /// Intialize new instance of TextualMacro
        /// </summary>
        /// <param name="source">The source code of the macro (python)</param>
        public Macro(string lang, string source)
        {
            Language = lang;
            m_Source = source;
            m_ID = Guid.Empty;
        }

        /// <summary>
        /// Set the source code (python) to be blank
        /// </summary>
        public void CreateBlankMacro()
        {
            m_Source = "";
        }

        /// <summary>
        /// Get the ID of the macro
        /// </summary>
        /// <returns>Guid of the macro</returns>
        public Guid GetID()
        {
            return m_ID;
        }

        /// <summary>
        /// Set the ID of the macro
        /// </summary>
        /// <param name="id">Guid to set</param>
        public void SetID(Guid id)
        {
            m_ID = id;
        }

        /// <summary>
        /// Set the source of the macro
        /// </summary>
        /// <param name="source">Source code (python)</param>
        public void SetSource(string source)
        {
            m_Source = source;
        }

        /// <summary>
        /// Get the source code of the macro
        /// </summary>
        /// <returns>Source code (python)</returns>
        public string GetSource()
        {
            return m_Source;
        }

        /// <summary>
        /// Rename the macro
        /// </summary>
        /// <param name="name">New name of the macro</param>
        public void Rename(string name)
        {
            FileManager.RenameMacro(m_ID, name);

            if (MacroEngine.IsRibbonMacro(m_ID))
                MacroEngine.RenameRibbonMacro(m_ID);
        }

        /// <summary>
        /// Gets the name of the macro
        /// </summary>
        /// <returns>Name of the macro</returns>
        public string GetName()
        {
            return MacroEngine.GetDeclaration(m_ID).Name;
        }

        /// <summary>
        /// Gets the relative file path of the macro
        /// </summary>
        /// <returns>Relative file path</returns>
        public string GetRelativePath()
        {
            return MacroEngine.GetDeclaration(m_ID).RelativePath;
        }

        /// <summary>
        /// Save the macro to it's respective file
        /// </summary>
        public void Save()
        {
            FileManager.SaveMacro(m_ID, m_Source);
        }

        /// <summary>
        /// Export the macro to a different file -> Save Copy As.
        /// </summary>
        public void Export()
        {
            FileManager.ExportMacro(m_ID, m_Source);
        }

        /// <summary>
        /// Delete the macro and it's respective file
        /// </summary>
        /// <param name="OnReturn">Action, containing the bool result, to be fired when the task is completed</param>
        public void Delete(Action<bool> OnReturn)
        {
            FileManager.DeleteMacro(m_ID, OnReturn);

            if (MacroEngine.IsRibbonMacro(m_ID))
                MacroEngine.RemoveRibbonMacro(m_ID);
        }

        /// <summary>
        /// Execute the macro using the Debug Execution Engine
        /// </summary>
        /// <param name="OnCompletedAction">Action to be fire when the task is completed</param>
        /// <param name="async">Bool identifying if the macro should be execute asynchronously or not (synchronous)</param>
        public void Execute(Action OnCompletedAction, bool async, string runtime = "")
        {
            if (string.IsNullOrEmpty(runtime))
                runtime = GetDefaultRuntime();

            IExecutionEngine engine = MacroEngine.GetExecutionEngine(runtime);
            if(engine != null)
                engine.ExecuteMacro(m_Source, OnCompletedAction, async);
        }

        /// <summary>
        /// Returns the default runtime for the macro's language
        /// </summary>
        /// <returns>Default runtime</returns>
        public string GetDefaultRuntime()
        {
            return MacroEngine.GetRuntimes(Language).FirstOrDefault<string>();
        }

        /// <summary>
        /// Returns the default file extension for the macro's runtime
        /// </summary>
        /// <returns>Default file extension</returns>
        public string GetDefaultFileExtension()
        {
            return MacroEngine.GetFileExt(GetDefaultRuntime());
        }
    }
}
