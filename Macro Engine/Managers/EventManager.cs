using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Engine
{
    public class EventManager
    {
        public static EventManager GetInstance()
        {
            return MacroEngine.GetEventManager();
        }

        #region MacroEngine events
        #region Events

        //OnLoaded event, for all Forms and GUIs
        public delegate void OnLoadedEvent();
        public event OnLoadedEvent OnLoaded;

        //OnDestroyed event, for all Forms and GUIs
        public delegate void DestroyEvent();
        public event DestroyEvent OnDestroyed;

        //OnFocused event, for all Forms and GUIs
        public delegate void FocusEvent();
        public event FocusEvent OnFocused;

        //OnShown event, for all Forms and GUIs
        public delegate void ShowEvent();
        public event ShowEvent OnShown;

        //OnHidden event, for all Forms and GUIs
        public delegate void HideEvent();
        public event HideEvent OnHidden;

        //OnIOChanged event, for all Forms and GUIs
        public delegate void IOChangedEvent();
        public event IOChangedEvent OnIOChanged;

        //OnLoaded event, for all Forms and GUIs
        public delegate void TerminateExecutionEvent();
        public event TerminateExecutionEvent OnTerminateExecution;

        //OnAssembliesChanged event, for all Forms and GUIs
        public delegate void AssembliesChangedEvent();
        public event AssembliesChangedEvent OnAssembliesChanged;

        //OnMacroCountChanged event, for all Forms and GUIs
        public delegate void MacroCountChangedEvent();
        public event MacroCountChangedEvent OnMacroCountChanged;

        //MacroRenamed event, for all Forms and GUIs
        public delegate void MacroRenameEvent(Guid id);
        public event MacroRenameEvent OnMacroRenamed;

        #endregion

        #region Event Firing

        public static void OnLoadedInvoke()
        {
            GetInstance().OnLoaded?.Invoke();
        }

        public static void OnDestroyedInvoke()
        {
            GetInstance().OnDestroyed?.Invoke();
        }

        public static void OnFocusedInvoke()
        {
            GetInstance().OnFocused?.Invoke();
        }

        public static void OnShownInvoke()
        {
            GetInstance().OnShown?.Invoke();
        }

        public static void OnHiddenInvoke()
        {
            GetInstance().OnHidden?.Invoke();
        }

        public static void OnIOChangedInvoke()
        {
            GetInstance().OnIOChanged?.Invoke();
        }

        public static void OnAssembliesChangedInvoke()
        {
            GetInstance().OnAssembliesChanged?.Invoke();
        }

        public static void OnMacroCountChangedInvoke()
        {
            GetInstance().OnMacroCountChanged?.Invoke();
        }

        public static void OnMacroRenamedInvoke(Guid id)
        {
            GetInstance().OnMacroRenamed?.Invoke(id);
        }

        public static void OnTerminateExecutionInvoke()
        {
            GetInstance().OnTerminateExecution?.Invoke();
        }

        #endregion
        #endregion

        #region Update host UI
        #region Events
        public delegate void ClearIOEvent();
        public event ClearIOEvent ClearAllIOEvent;

        public delegate void MacroAddEvent(Guid id, string macroName, string macroPath, Action macroClickEvent);
        public event MacroAddEvent AddRibbonMacroEvent;

        public delegate void MacroRemoveEvent(Guid id);
        public event MacroRemoveEvent RemoveRibbonMacroEvent;

        public delegate void MacroEditEvent(Guid id, string macroName, string macroPath);
        public event MacroEditEvent RenameRibbonMacroEvent;

        public delegate void SetEnabled(bool enabled);
        public event SetEnabled SetInteractiveEvent;

        #endregion

        #region Event Firing

        /// <summary>
        /// Fires SetExcelIneractive event
        /// </summary>
        /// <param name="enabled">Whether or not Excel should be set as interactive</param>
        public static void SetInteractive(bool enabled)
        {
            GetInstance().SetInteractiveEvent?.Invoke(enabled);
        }

        /// <summary>
        /// Fires AddRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="macroName">The macro's name</param>
        /// <param name="macroPath">The macro's relative path</param>
        /// <param name="macroClickEvent">Event callback for when the macro is clicked</param>
        public static void AddRibbonMacro(Guid id, string macroName, string macroPath, Action macroClickEvent)
        {
            GetInstance().AddRibbonMacroEvent?.Invoke(id, macroName, macroPath, macroClickEvent);
        }

        /// <summary>
        /// Fires RemoveRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        public static void RemoveRibbonMacro(Guid id)
        {
            GetInstance().RemoveRibbonMacroEvent?.Invoke(id);
        }

        /// <summary>
        /// Fires RenameRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="macroName">The macro's name</param>
        /// <param name="macroPath">The macro's relative path</param>
        public static void RenameRibbonMacro(Guid id, string macroName, string macroPath)
        {
            GetInstance().RenameRibbonMacroEvent?.Invoke(id, macroName, macroPath);
        }

        /// <summary>
        /// Fires ClearAllIO event
        /// </summary>
        public static void ClearAllIO()
        {
            GetInstance().ClearAllIOEvent?.Invoke();
        }

        #endregion
        #endregion
    }
}
