using Macro_Engine.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Macro_Engine
{
    public class MEvents
    {
        /*
        #region MacroEngine Events

        //OnLoaded event, for all Forms and GUIs
        public delegate void OnLoadedEvent();
        public static event OnLoadedEvent OnLoaded;

        //OnDestroyed event, for all Forms and GUIs
        public delegate void DestroyEvent();
        public static event DestroyEvent OnDestroyed;

        //OnFocused event, for all Forms and GUIs
        public delegate void FocusEvent();
        public static event FocusEvent OnFocused;

        //OnShown event, for all Forms and GUIs
        public delegate void ShowEvent();
        public static event ShowEvent OnShown;

        //OnHidden event, for all Forms and GUIs
        public delegate void HideEvent();
        public static event HideEvent OnHidden;

        //OnIOChanged event, for all Forms and GUIs
        public delegate void IOChangedEvent(string runtime, IExecutionEngineIO manager);
        public static event IOChangedEvent OnIOChanged;

        //OnHostExecute event, for executing macros synchronously 
        public delegate void OnHostExecuteEvent(DispatcherPriority priority, Action task);
        public static event OnHostExecuteEvent OnHostExecute;

        //OnTerminateExecution event, for all macros
        public delegate void TerminateExecutionEvent();
        public static event TerminateExecutionEvent OnTerminateExecution;

        //OnAssembliesChanged event, for all Forms and GUIs
        public delegate void AssembliesChangedEvent();
        public static event AssembliesChangedEvent OnAssembliesChanged;

        //OnMacroCountChanged event, for all Forms and GUIs
        public delegate void MacroCountChangedEvent();
        public static event MacroCountChangedEvent OnMacroCountChanged;

        //MacroRenamed event, for all Forms and GUIs
        public delegate void MacroRenameEvent(Guid id);
        public static event MacroRenameEvent OnMacroRenamed;

        #endregion

        #region Update host UI Events

        public delegate void ClearIOEvent();
        public static event ClearIOEvent ClearAllIOEvent;

        public delegate void MacroAddEvent(Guid id, string macroName, string macroPath, Action macroClickEvent);
        public static event MacroAddEvent AddRibbonMacroEvent;

        public delegate void MacroRemoveEvent(Guid id);
        public static event MacroRemoveEvent RemoveRibbonMacroEvent;

        public delegate void MacroEditEvent(Guid id, string macroName, string macroPath);
        public static event MacroEditEvent RenameRibbonMacroEvent;

        public delegate void SetEnabled(bool enabled);
        public static event SetEnabled SetInteractiveEvent;

        #endregion

        #region Event Firing MacroEngine

        public static void OnLoadedInvoke()
        {
            OnLoaded?.Invoke();
        }

        public static void OnDestroyedInvoke()
        {
            OnDestroyed?.Invoke();
        }

        public static void OnFocusedInvoke()
        {
            OnFocused?.Invoke();
        }

        public static void OnShownInvoke()
        {
            OnShown?.Invoke();
        }

        public static void OnHiddenInvoke()
        {
            OnHidden?.Invoke();
        }

        public static void OnIOChangedInvoke(string runtime, IExecutionEngineIO manager)
        {
            OnIOChanged?.Invoke(runtime, manager);
        }

        public static void OnHostExecuteInvoke(DispatcherPriority priority, Action task)
        {
            OnHostExecute?.Invoke(priority, task);
        }

        public static void OnAssembliesChangedInvoke()
        {
            OnAssembliesChanged?.Invoke();
        }

        public static void OnMacroCountChangedInvoke()
        {
            OnMacroCountChanged?.Invoke();
        }

        public static void OnMacroRenamedInvoke(Guid id)
        {
            OnMacroRenamed?.Invoke(id);
        }

        public static void OnTerminateExecutionInvoke()
        {
            OnTerminateExecution?.Invoke();
        }

        #endregion

        #region Event Firing Host UI

        /// <summary>
        /// Fires SetExcelIneractive event
        /// </summary>
        /// <param name="enabled">Whether or not Excel should be set as interactive</param>
        public static void SetInteractive(bool enabled)
        {
            SetInteractiveEvent?.Invoke(enabled);
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
            AddRibbonMacroEvent?.Invoke(id, macroName, macroPath, macroClickEvent);
        }

        /// <summary>
        /// Fires RemoveRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        public static void RemoveRibbonMacro(Guid id)
        {
            RemoveRibbonMacroEvent?.Invoke(id);
        }

        /// <summary>
        /// Fires RenameRibbonMacro event
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="macroName">The macro's name</param>
        /// <param name="macroPath">The macro's relative path</param>
        public static void RenameRibbonMacro(Guid id, string macroName, string macroPath)
        {
            RenameRibbonMacroEvent?.Invoke(id, macroName, macroPath);
        }

        /// <summary>
        /// Fires ClearAllIO event
        /// </summary>
        public static void ClearAllIO()
        {
            ClearAllIOEvent?.Invoke();
        }

        #endregion
        */
    }
}
