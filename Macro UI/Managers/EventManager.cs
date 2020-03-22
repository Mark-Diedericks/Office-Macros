/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.9
 * Event manager, allowing for cross-thread interaction between the Excel Ribbon tab and the UI/Interop projects
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Macro_Engine;
using Macro_Engine.Macros;
using System.Windows.Threading;
using System.Threading;
using Macro_UI.View;
using Macro_UI.Model;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using Macro_UI.ViewModel.Base;
using Macro_UI.ViewModel;
using Macro_Engine.Interop;

namespace Macro_UI.Routing
{
    public class EventManager
    {
        /*public delegate void ClearIOEvent();
        public event ClearIOEvent ClearAllIOEvent;

        public delegate void MacroAddEvent(Guid id, string macroName, string macroPath, Action macroClickEvent);
        public event MacroAddEvent AddRibbonMacroEvent;

        public delegate void MacroRemoveEvent(Guid id);
        public event MacroRemoveEvent RemoveRibbonMacroEvent;

        public delegate void MacroEditEvent(Guid id, string macroName, string macroPath);
        public event MacroEditEvent RenameRibbonMacroEvent;

        public delegate void LoadEvent();
        public static event LoadEvent ApplicationLoadedEvent;
        public static event LoadEvent RibbonLoadedEvent;
        private event LoadEvent ShutdownEvent;

        public delegate void SetEnabled(bool enabled);
        public event SetEnabled SetInteractiveEvent;

        public delegate void ThemeEvent();
        public static event ThemeEvent ThemeChangedEvent;

        public delegate void DocumentEvent(DocumentViewModel vm);
        public static event DocumentEvent DocumentChangedEvent;*/

        public delegate void InputMessageEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type, Action<object> OnResult);
        public event InputMessageEvent DisplayInputMessageEvent;

        public delegate object InputMessageReturnEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type);
        public event InputMessageReturnEvent DisplayInputMessageReturnEvent;

        private static App s_UIApp;

        private static EventManager s_Instance;
        private static bool s_IsLoaded;
        private static bool s_IsRibbonLoaded;

        /// <summary>
        /// Instiantiation of EventManager
        /// </summary>
        private EventManager()
        {
            s_Instance = this;
            s_IsLoaded = false;
        }

        /// <summary>
        /// Gets instance of EventManager
        /// </summary>
        /// <returns></returns>
        public static EventManager GetInstance()
        {
            return s_Instance;
        }

        /// <summary>
        /// Returns whether or not the application has been loaded
        /// </summary>
        /// <returns>Whether or not the application is loaded</returns>
        public static bool IsLoaded()
        {
            return s_IsLoaded;
        }

        /// <summary>
        /// Returns whether or not the ribbon tab has been loaded
        /// </summary>
        /// <returns>Whether or not the application is loaded</returns>
        public static bool IsRibbonLoaded()
        {
            return s_IsRibbonLoaded;
        }

        /// <summary>
        /// Initializes the UI and Interop and binds events to the AddIn.
        /// </summary>
        /// <param name="application">Excel Application</param>
        /// <param name="dispatcher">Excel UI Dispatcher</param>
        /// <param name="RibbonMacros">A serialized list of ribbon accessible macros</param>
        public static void CreateApplicationInstance(Dispatcher dispatcher, string[] RibbonMacros)
        {
            new EventManager();

            string[] workspaces = new string[] { Path.GetFullPath(Files.AssemblyDirectory + "/Macros/") };
            HostState state = new HostState(workspaces, RibbonMacros, Properties.Settings.Default.ActiveMacro, Properties.Settings.Default.IncludedLibraries);

            s_UIApp = new App();
            s_UIApp.InitializeComponent();

            CancellationTokenSource cts = MacroEngine.Instantiate(dispatcher, state, new Action(() =>
            {
                //Events.OnFocused += WindowFocusEvent;
                //Events.OnShown += WindowShowEvent;
                //Events.OnHidden += WindowHideEvent;

                Events.SubscribeEvent("OnFocused", (Action)FocusWindow);
                Events.SubscribeEvent("OnShown", (Action)ShowWindow);
                Events.SubscribeEvent("OnHidden", (Action)HideWindow);


                Messages.DisplayOkMessageEvent += DisplayOkMessage;
                Messages.DisplayYesNoMessageEvent += DisplayYesNoMessage;
                Messages.DisplayYesNoMessageReturnEvent += DisplayYesNoMessageReturn;

                Messages.DisplayInputMessageEvent += EventManager_DisplayInputMessageEvent;
                Messages.DisplayInputMessageReturnEvent += EventManager_DisplayInputMessageReturnEvent;


                //Events.ClearAllIOEvent += ClearAllIO;
                //Events.AddRibbonMacroEvent += GetInstance().AddMacro;
                //Events.RemoveRibbonMacroEvent += GetInstance().RemoveMacro;
                //Events.RenameRibbonMacroEvent += GetInstance().RenameMacro;

                //Events.SubscribeEvent("ClearAllIO", (Action)ClearAllIO);
                //Events.SubscribeEvent("AddRibbonMacro", (Action<Guid, string, string, Action>)GetInstance().AddMacro);
                //Events.SubscribeEvent("RemoveRibbonMacro", (Action<Guid>)GetInstance().RemoveMacro);
                //Events.SubscribeEvent("RenameRibbonMacro", (Action<Guid, string, string>)GetInstance().RenameMacro);

                //Events.SubscribeEvent("SetInteractive", new Action<bool>((enabled) => 
                //{
                //    GetInstance().SetInteractiveEvent?.Invoke(enabled);
                //}));

                //GetInstance().IOChangedEvent += MacroEngine.SetIOStreams;
                //GetInstance().IOChangedEvent += delegate(string runtime, TextWriter output, TextWriter error, TextReader input) 
                //{ Events.InvokeEvent("SetIO", new object[] { runtime, output, error, input}); };

                if (s_IsRibbonLoaded)
                    Events.InvokeEvent("LoadRibbonMacros");

                s_IsLoaded = true;
                Events.InvokeEvent("ApplicationLoaded");
            }));

            Events.SubscribeEvent("Shutdown", new Action(() =>
            {
                try
                {
                    cts.Cancel();
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                if(MainWindow.GetInstance() != null)
                {
                    MainWindow.GetInstance().Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                    {
                        MacroDeclaration md = MacroEngine.GetDeclaration(MacroEngine.GetActiveMacro());
                        if (md != null)
                            Properties.Settings.Default.ActiveMacro = md.RelativePath;

                        Properties.Settings.Default.IncludedLibraries = MacroEngine.GetAssemblies().ToArray<AssemblyDeclaration>();

                        if (MainWindowViewModel.GetInstance() != null)
                        {
                            MainWindowViewModel.GetInstance().SaveAll();
                            List<DocumentViewModel> unsaved = MainWindowViewModel.GetInstance().DockManager.GetUnsavedDocuments();

                            if (unsaved.Count > 0)
                            {
                                bool save = DisplayYesNoMessageReturn("You have unsaved documents. Would you like to save them?", "Unsaved Documents");

                                if (save)
                                    foreach (DocumentViewModel document in unsaved)
                                        if (document is TextualEditorViewModel)
                                            document.Save(null);
                            }
                        }

                        s_UIApp.Shutdown();
                    }));
                }
                else if (s_UIApp != null)
                {
                    s_UIApp.Shutdown();
                }
            }));

            s_UIApp.Run();
        }


        #region Main to UI to Excel Events

        /// <summary>
        /// Fowards event to Excel's InputBox -> Asynchronous
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="def"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="helpFile"></param>
        /// <param name="helpContextID"></param>
        /// <param name="type"></param>
        /// <param name="OnResult"></param>
        private static void EventManager_DisplayInputMessageEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type, Action<object> OnResult)
        {
            GetInstance().DisplayInputMessageEvent?.Invoke(message, title, def, left, top, helpFile, helpContextID, type, OnResult);
        }

        /// <summary>
        /// Forwards event to Excel's InputBox -> Synchronous
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="def"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="helpFile"></param>
        /// <param name="helpContextID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object EventManager_DisplayInputMessageReturnEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type)
        {
            return GetInstance().DisplayInputMessageReturnEvent?.Invoke(message, title, def, left, top, helpFile, helpContextID, type);
        }

        #endregion

        #region Excel to UI Events

        /// <summary>
        /// Shows the main window
        /// </summary>
        public void MacroEditorClick()
        {
            if (MainWindowViewModel.GetInstance() == null || MainWindow.GetInstance() == null)
                return;

            MainWindow.GetInstance().Dispatcher.Invoke(() =>
            {
                MainWindowViewModel.GetInstance().ShowWindow();
            });
        }

        /// <summary>
        /// Shows the main window and creates new textual macro
        /// </summary>
        public void NewMacroClick()
        {
            if (MainWindowViewModel.GetInstance() == null || MainWindow.GetInstance() == null)
                return;

            MainWindow.GetInstance().Dispatcher.Invoke(() =>
            {
                MainWindowViewModel.GetInstance().ShowWindow();
                MainWindowViewModel.GetInstance().CreateMacroAsync();
            });
        }

        /// <summary>
        /// SHows the main window and prompts to import a macro
        /// </summary>
        public void OpenMacroClickEvent()
        {
            if (MainWindowViewModel.GetInstance() == null || MainWindow.GetInstance() == null)
                return;

            MainWindow.GetInstance().Dispatcher.Invoke(() =>
            {
                MainWindowViewModel.GetInstance().ShowWindow();
                MainWindowViewModel.GetInstance().ImportMacroAsync();
            });
        }

        #endregion

        #region Main to UI
        
        /// <summary>
        /// Focuses main window
        /// </summary>
        public static void FocusWindow()
        {
            MainWindowViewModel.GetInstance().TryFocus();
        }

        /// <summary>
        /// Shows main window
        /// </summary>
        public static void ShowWindow()
        {
            MainWindowViewModel.GetInstance().ShowWindow();
        }

        /// <summary>
        /// Hides main window
        /// </summary>
        public static void HideWindow()
        {
            MainWindowViewModel.GetInstance().HideWindow();
        }

        /// <summary>
        /// Displays OK message
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        public static void DisplayOkMessage(string content, string title)
        {
            MainWindowViewModel.GetInstance().DisplayOkMessage(content, title);
        }

        /// <summary>
        /// Displays yes/no message -> asynchronous
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <param name="OnReturn">The Action, and bool representing the user's input, to be fires when the user returns input</param>
        public static void DisplayYesNoMessage(string content, string title, Action<bool> OnReturn)
        {
            MainWindowViewModel.GetInstance().DisplayYesNoMessage(content, title, OnReturn);
        }

        /// <summary>
        /// Displays yes/no message -> synchronous
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <returns>Bool representing the user's input</returns>
        public static bool DisplayYesNoMessageReturn(string content, string title)
        {
            Task<bool> t = MainWindowViewModel.GetInstance().DisplayYesNoMessageReturn(content, title);
            t.Wait();
            return t.Result;
        }

        /// <summary>
        /// Displays yes/no message -> synchronous
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The message's header</param>
        /// <param name="aux">The text in the 3rd button</param>
        /// <param name="OnReturn">The Action, and MessageDialogResult of the user's input, to be fired when the user provides input</param>
        public static void DisplayYesNoCancelMessage(string message, string caption, string aux, Action<MessageDialogResult> OnReturn)
        {
            MainWindowViewModel.GetInstance().DisplayYesNoCancelMessage(message, caption, aux, OnReturn);
        }

        #endregion
    }
}
