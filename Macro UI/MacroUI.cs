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

namespace Macro_UI
{
    public class MacroUI : IMacroEngine
    {
        #region Dispatchers & Threading

        private readonly Dispatcher m_HostDispatcher;

        /// <summary>
        /// Gets host office application UI dispatcher
        /// </summary>
        /// <returns>Office application UI dispatcher</returns>
        public static Dispatcher GetHostDispatcher()
        {
            return GetInstance().m_HostDispatcher;
        }

        #endregion

        public delegate void InputMessageEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type, Action<object> OnResult);
        public event InputMessageEvent DisplayInputMessageEvent;

        public delegate object InputMessageReturnEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type);
        public event InputMessageReturnEvent DisplayInputMessageReturnEvent;

        private static MacroUI s_Instance;
        private static bool s_IsLoaded;
        private static bool s_IsRibbonLoaded;
        public IMacroEngine MacroEngine { get; private set; }
        public MainWindow MainWindow { get; private set; }

        /// <summary>
        /// Instiantiation of EventManager
        /// </summary>
        private MacroUI(Dispatcher dispatcher, IMacroEngine engine)
        {
            s_Instance = this;
            s_IsLoaded = false;

            m_HostDispatcher = dispatcher;
            MacroEngine = engine;
        }
        public CancellationTokenSource Instantiate(HostState state, Action OnLoaded)
        {
            Events.SubscribeEvent("OnFocused", (Action)FocusWindow);
            Events.SubscribeEvent("OnShown", (Action)ShowWindow);
            Events.SubscribeEvent("OnHidden", (Action)HideWindow);


            Messages.DisplayOkMessageEvent += DisplayOkMessage;
            Messages.DisplayYesNoMessageEvent += DisplayYesNoMessage;
            Messages.DisplayYesNoMessageReturnEvent += DisplayYesNoMessageReturn;

            Messages.DisplayInputMessageEvent += EventManager_DisplayInputMessageEvent;
            Messages.DisplayInputMessageReturnEvent += EventManager_DisplayInputMessageReturnEvent;

            if (s_IsRibbonLoaded)
                Events.InvokeEvent("LoadRibbonMacros");

            MainWindow = new MainWindow() { DataContext = new MainWindowViewModel() };
            ((MainWindowViewModel)MainWindow.DataContext).SetTheme(Macro_UI.Properties.Settings.Default.Theme);

            System.Diagnostics.Debug.WriteLine(">>>> >>>> >>>> >>>> " + MainWindow.DataContext);
            
            GetHostDispatcher().BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                OnLoaded?.Invoke();
            }));

            return null;
        }

        public void Destroy()
        {
            GetInstance().MainWindow.Close();
            GetInstance().MacroEngine.Destroy();

            Events.InvokeEvent("Shutdown");
        }

        /// <summary>
        /// Gets instance of EventManager
        /// </summary>
        /// <returns></returns>
        public static MacroUI GetInstance()
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
        public static void CreateApplicationInstance(Dispatcher dispatcher, IMacroEngine engine, string[] RibbonMacros)
        {
            new MacroUI(dispatcher, engine);
            string[] workspaces = new string[] { Path.GetFullPath(Files.AssemblyDirectory + "/Macros/") };
            HostState state = new HostState(workspaces, RibbonMacros, Properties.Settings.Default.ActiveMacro, Properties.Settings.Default.IncludedLibraries);

            CancellationTokenSource cts_eng = engine.Instantiate(state, new Action(() => {
                GetInstance().Instantiate(state, null);

                s_IsLoaded = true;
                Events.InvokeEvent("ApplicationLoaded");
            }));

            Events.SubscribeEvent("Shutdown", new Action(() =>
            {
                try
                {
                    cts_eng?.Cancel();
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                if(MainWindow.GetInstance() != null)
                {
                    MainWindow.GetInstance().Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                    {
                        MacroDeclaration md = GetInstance().GetDeclaration(GetInstance().GetActiveMacro());
                        if (md != null)
                            Properties.Settings.Default.ActiveMacro = md.RelativePath;

                        Properties.Settings.Default.IncludedLibraries = GetInstance().GetAssemblies().ToArray<AssemblyDeclaration>();

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
                    }));
                }
            }));
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
            MainWindowViewModel.GetInstance()?.TryFocus();
        }

        /// <summary>
        /// Shows main window
        /// </summary>
        public static void ShowWindow()
        {
            MainWindowViewModel.GetInstance()?.ShowWindow();
        }

        /// <summary>
        /// Hides main window
        /// </summary>
        public static void HideWindow()
        {
            MainWindowViewModel.GetInstance()?.HideWindow();
        }

        /// <summary>
        /// Displays OK message
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        public static void DisplayOkMessage(string content, string title)
        {
            MainWindowViewModel.GetInstance()?.DisplayOkMessage(content, title);
        }

        /// <summary>
        /// Displays yes/no message -> asynchronous
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <param name="OnReturn">The Action, and bool representing the user's input, to be fires when the user returns input</param>
        public static void DisplayYesNoMessage(string content, string title, Action<bool> OnReturn)
        {
            MainWindowViewModel.GetInstance()?.DisplayYesNoMessage(content, title, OnReturn);
        }

        /// <summary>
        /// Displays yes/no message -> synchronous
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <returns>Bool representing the user's input</returns>
        public static bool DisplayYesNoMessageReturn(string content, string title)
        {
            Task<bool> t = MainWindowViewModel.GetInstance()?.DisplayYesNoMessageReturn(content, title);
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
            MainWindowViewModel.GetInstance()?.DisplayYesNoCancelMessage(message, caption, aux, OnReturn);
        }

        #endregion

        #region IMacroEngine

        public HashSet<string> GetRuntimes(string language = "")
        {
            return MacroEngine.GetRuntimes(language);
        }

        public Dictionary<Guid, MacroDeclaration> GetDeclarations()
        {
            return MacroEngine.GetDeclarations();
        }

        public MacroDeclaration GetDeclaration(Guid id)
        {
            return MacroEngine.GetDeclaration(id);
        }

        public void SetDeclaration(Guid id, MacroDeclaration md)
        {
            MacroEngine.SetDeclaration(id, md);
        }

        public Dictionary<Guid, IMacro> GetMacros()
        {
            return MacroEngine.GetMacros();
        }

        public IMacro GetMacro(Guid id)
        {
            return MacroEngine.GetMacro(id);
        }

        public Guid AddMacro(MacroDeclaration md, IMacro macro)
        {
            return MacroEngine.AddMacro(md, macro);
        }

        public void RemoveMacro(Guid id)
        {
            MacroEngine.RemoveMacro(id);
        }

        public void RenameMacro(Guid id, string newName)
        {
            MacroEngine.RenameMacro(id, newName);
        }

        public Guid GetIDFromRelativePath(string relativepath)
        {
            return MacroEngine.GetIDFromRelativePath(relativepath);
        }

        public string GetDefaultFileExtension()
        {
            return MacroEngine.GetDefaultFileExtension();
        }

        public HashSet<AssemblyDeclaration> GetAssemblies()
        {
            return MacroEngine.GetAssemblies();
        }

        public void AddAssembly(AssemblyDeclaration declaration)
        {
            MacroEngine.AddAssembly(declaration);
        }

        public void RemoveAssembly(AssemblyDeclaration declaration)
        {
            MacroEngine.RemoveAssembly(declaration);
        }

        public Guid GetActiveMacro()
        {
            return MacroEngine.GetActiveMacro();
        }

        public void SetActiveMacro(Guid id)
        {
            MacroEngine.SetActiveMacro(id);
        }

        public bool IsRibbonMacro(Guid id)
        {
            return MacroEngine.IsRibbonMacro(id);
        }

        public void AddRibbonMacro(Guid id)
        {
            MacroEngine.AddRibbonMacro(id);
        }

        public void RemoveRibbonMacro(Guid id)
        {
            MacroEngine.RemoveRibbonMacro(id);
        }

        public HashSet<Guid> RenameFolder(string oldDir, string newDir)
        {
            return MacroEngine.RenameFolder(oldDir, newDir);
        }

        public void DeleteFolder(string dir, Action<bool> OnReturn)
        {
            MacroEngine.DeleteFolder(dir, OnReturn);
        }

        #endregion
    }
}
