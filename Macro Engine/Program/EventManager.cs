using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class EventManager
    {
        private readonly EngineBase m_Engine;
        protected EngineBase GetEngine()
        {
            return m_Engine;
        }

        private EventManager(EngineBase engine)
        {
            m_Engine = engine;
        }

        public static EventManager Instantiate(EngineBase engine)
        {
            return new EventManager(engine);
        }
        public delegate void ClearIOEvent();
        public event ClearIOEvent ClearAllIOEvent;

        public delegate void MacroAddEvent(Guid id, string macroName, string macroPath, Action macroClickEvent);
        public event MacroAddEvent AddRibbonMacroEvent;

        public delegate void MacroRemoveEvent(Guid id);
        public event MacroRemoveEvent RemoveRibbonMacroEvent;

        public delegate void MacroEditEvent(Guid id, string macroName, string macroPath);
        public event MacroEditEvent RenameRibbonMacroEvent;

        #region Event Firing

        /// <summary>
        /// Fires AddRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="macroName">The macro's name</param>
        /// <param name="macroPath">The macro's relative path</param>
        /// <param name="macroClickEvent">Event callback for when the macro is clicked</param>
        public void AddRibbonMacro(Guid id, string macroName, string macroPath, Action macroClickEvent)
        {
            GetInstance().AddRibbonMacroEvent?.Invoke(id, macroName, macroPath, macroClickEvent);
        }

        /// <summary>
        /// Fires RemoveRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void RemoveRibbonMacro(Guid id)
        {
            GetInstance().RemoveRibbonMacroEvent?.Invoke(id);
        }

        /// <summary>
        /// Fires RenameRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="macroName">The macro's name</param>
        /// <param name="macroPath">The macro's relative path</param>
        public void RenameRibbonMacro(Guid id, string macroName, string macroPath)
        {
            GetInstance().RenameRibbonMacroEvent?.Invoke(id, macroName, macroPath);
        }

        /// <summary>
        /// Fires ClearAllIO event
        /// </summary>
        public void ClearAllIO()
        {
            GetInstance().ClearAllIOEvent?.Invoke();
        }

        #endregion
    }
}
