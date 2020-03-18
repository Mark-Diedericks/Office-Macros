using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine.Macros
{
    public abstract class Macro
    {
        private readonly EngineBase m_Engine;
        protected EngineBase GetEngine()
        {
            return m_Engine;
        }

        protected Guid m_ID;
        protected string m_Source;

        protected Macro(EngineBase engine, string source)
        {
            m_Engine = engine;
            m_Source = source;
            m_ID = Guid.Empty;
        }

        #region engine specific methods

        public abstract void CreateBlankMacro();

        public abstract void SetID(Guid id);
        public abstract Guid GetID();

        public abstract void SetSource(string source);
        public abstract string GetSource();

        public abstract void ExecuteDebug(Action OnCompletedAction, bool async);
        public abstract void ExecuteRelease(Action OnCompletedAction, bool async);

        #endregion

        #region non-specific methods

        /// <summary>
        /// Rename the macro
        /// </summary>
        /// <param name="name">New name of the macro</param>
        public void Rename(string name)
        {
            GetEngine().GetFileManager().RenameMacro(m_ID, name);

            if (GetEngine().IsRibbonMacro(m_ID))
                GetEngine().RenameRibbonMacro(m_ID);
        }

        /// <summary>
        /// Gets the name of the macro
        /// </summary>
        /// <returns>Name of the macro</returns>
        public string GetName()
        {
            return GetEngine().GetDeclaration(m_ID).name;
        }

        /// <summary>
        /// Gets the relative file path of the macro
        /// </summary>
        /// <returns>Relative file path</returns>
        public string GetRelativePath()
        {
            return GetEngine().GetDeclaration(m_ID).relativepath;
        }

        /// <summary>
        /// Save the macro to it's respective file
        /// </summary>
        public void Save()
        {
            GetEngine().GetFileManager().SaveMacro(m_ID, m_Source);
        }

        /// <summary>
        /// Export the macro to a different file -> Save Copy As.
        /// </summary>
        public void Export()
        {
            GetEngine().GetFileManager().ExportMacro(m_ID, m_Source);
        }

        /// <summary>
        /// Delete the macro and it's respective file
        /// </summary>
        /// <param name="OnReturn">Action, containing the bool result, to be fired when the task is completed</param>
        public void Delete(Action<bool> OnReturn)
        {
            GetEngine().GetFileManager().DeleteMacro(m_ID, OnReturn);

            if (GetEngine().IsRibbonMacro(m_ID))
                GetEngine().RemoveRibbonMacro(m_ID);
        }

        #endregion
    }
}
