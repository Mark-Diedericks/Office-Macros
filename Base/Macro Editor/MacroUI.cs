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
using Macro_Editor.View;
using Macro_Editor.Model;
using System.IO;
using MahApps.Metro.Controls.Dialogs;
using Macro_Editor.ViewModel.Base;
using Macro_Editor.ViewModel;
using Macro_Engine.Interop;

namespace Macro_Editor
{
    public class MacroUI : IMacroEngine
    {
        #region Instance & Instance Variables

        public delegate void InputMessageEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type, Action<object> OnResult);
        public event InputMessageEvent DisplayInputMessageEvent;

        public delegate Task<object> InputMessageReturnEvent(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type);
        public event InputMessageReturnEvent DisplayInputMessageReturnEvent;

        private static MacroUI s_Instance;
        private bool m_IsLoaded;
        private bool m_IsRibbonLoaded;

        public IMacroEngine MacroEngine { get; private set; }
        public MainWindow MainWindow { get; private set; }

        /// <summary>
        /// Instiantiation of EventManager
        /// </summary>
        private MacroUI(IMacroEngine engine)
        {
            s_Instance = this;
            m_IsLoaded = false;

            MacroEngine = engine;
        }

        /// <summary>
        /// Gets instance of EventManager
        /// </summary>
        /// <returns></returns>
        public static MacroUI GetInstance()
        {
            return s_Instance;
        }

        #endregion

        #region Creating/Destroying Instance

        public void Instantiate(HostState state)
        {
            Events.SubscribeEvent("OnFocused", (Action)FocusWindow);
            Events.SubscribeEvent("OnShown", (Action)ShowWindow);
            Events.SubscribeEvent("OnHidden", (Action)HideWindow);


            Messages.DisplayOkMessageEvent += DisplayOkMessage;
            Messages.DisplayYesNoMessageReturnEvent += DisplayYesNoMessageReturn;
            Messages.DisplayInputMessageReturnEvent += DisplayInputMessageReturn;
        }

        public void Destroy()
        {
            FileDeclaration activeDoc = MainWindowViewModel.GetInstance().DockManager.GetActiveDocumentDeclaration();
            if (activeDoc != null)
                Properties.Settings.Default.ActiveDocument = activeDoc.Info.FullName;

            Properties.Settings.Default.IncludedLibraries = GetAssemblies().ToArray<AssemblyDeclaration>();

            if (MainWindowViewModel.GetInstance() != null)
            {
                MainWindowViewModel.GetInstance().SaveAll();
                List<DocumentViewModel> unsaved = MainWindowViewModel.GetInstance().DockManager.GetUnsavedDocuments();

                if (unsaved.Count > 0)
                {
                    Task<bool> save = GetInstance().DisplayYesNoMessageReturn("You have unsaved documents. Would you like to save them?", "Unsaved Documents");

                    save.Wait();
                    if (save.Result)
                        foreach (DocumentViewModel document in unsaved)
                            if (document is TextualEditorViewModel)
                                document.Save(null);
                }
            }
        }

        #endregion

        #region Loading & Entry 

        /// <summary>
        /// Returns whether or not the application has been loaded
        /// </summary>
        /// <returns>Whether or not the application is loaded</returns>
        public bool IsLoaded()
        {
            return m_IsLoaded;
        }

        /// <summary>
        /// Returns whether or not the ribbon tab has been loaded
        /// </summary>
        /// <returns>Whether or not the application is loaded</returns>
        public bool IsRibbonLoaded()
        {
            return m_IsRibbonLoaded;
        }

        /// <summary>
        /// Initializes the UI and Interop and binds events to the AddIn.
        /// </summary>
        /// <param name="application">Excel Application</param>
        /// <param name="dispatcher">Excel UI Dispatcher</param>
        /// <param name="RibbonMacros">A serialized list of ribbon accessible macros</param>
        public static MacroUI CreateApplicationInstance(IMacroEngine engine)
        {
            MacroUI ui = new MacroUI(engine);
            string[] workspaces = new string[] { Path.GetFullPath(Files.AssemblyDirectory + "/Macros/") };
            HostState state = new HostState(workspaces, Properties.Settings.Default.ActiveDocument, Properties.Settings.Default.IncludedLibraries);

            engine.Instantiate(state);
            ui.Instantiate(state);


            ui.MainWindow = new MainWindow() { DataContext = new MainWindowViewModel() };
            ((MainWindowViewModel)ui.MainWindow.DataContext).SetTheme(Macro_Editor.Properties.Settings.Default.Theme);

            if (ui.m_IsRibbonLoaded)
                Events.InvokeEvent("LoadRibbonMacros");

            ui.m_IsLoaded = true;

            return ui;
        }

        #endregion

        #region Main to UI to Excel Events

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
        private static Task<object> DisplayInputMessageReturn(string message, object title, object def, object left, object top, object helpFile, object helpContextID, object type)
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

        public void AddAccent(string name, Uri resource)
        {
            MainWindowViewModel.GetInstance()?.AddAccent(name, resource);
        }

        public void SetAccent(string name)
        {
            MainWindowViewModel.GetInstance()?.SetAccent(name);
        }

        #endregion

        #region Main to UI
        
        /// <summary>
        /// Focuses main window
        /// </summary>
        public void FocusWindow()
        {
            MainWindowViewModel.GetInstance()?.TryFocus();
        }

        /// <summary>
        /// Shows main window
        /// </summary>
        public void ShowWindow()
        {
            MainWindowViewModel.GetInstance()?.ShowWindow();
        }

        /// <summary>
        /// Hides main window
        /// </summary>
        public void HideWindow()
        {
            MainWindowViewModel.GetInstance()?.HideWindow();
        }

        /// <summary>
        /// Displays OK message
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        public void DisplayOkMessage(string content, string title)
        {
            MainWindowViewModel.GetInstance()?.DisplayOkMessage(content, title);
        }

        /// <summary>
        /// Displays yes/no message -> synchronous
        /// </summary>
        /// <param name="content">The message to be displayed</param>
        /// <param name="title">The message's header</param>
        /// <returns>Bool representing the user's input</returns>
        public Task<bool> DisplayYesNoMessageReturn(string content, string title)
        {
            return MainWindowViewModel.GetInstance()?.DisplayYesNoMessageReturn(content, title);
        }

        /// <summary>
        /// Displays yes/no message -> synchronous
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The message's header</param>
        /// <param name="aux">The text in the 3rd button</param>
        /// <param name="OnReturn">The Action, and MessageDialogResult of the user's input, to be fired when the user provides input</param>
        public void DisplayYesNoCancelMessage(string message, string caption, string aux, Action<MessageDialogResult> OnReturn)
        {
            MainWindowViewModel.GetInstance()?.DisplayYesNoCancelMessage(message, caption, aux, OnReturn);
        }

        #endregion

        #region IMacroEngine

        public HashSet<string> GetRuntimes(string language = "")
        {
            return MacroEngine.GetRuntimes(language);
        }

        public HashSet<FileDeclaration> GetFileDeclarations()
        {
            return MacroEngine.GetFileDeclarations();
        }

        public void AddFile(FileDeclaration d)
        {
            MacroEngine.AddFile(d);
        }

        public void RemoveFile(FileDeclaration d)
        {
            MacroEngine.RemoveFile(d);
        }

        public void RenameFile(FileDeclaration d, string newName)
        {
            MacroEngine.RenameFile(d, newName);
        }

        public FileDeclaration GetDeclarationFromFullname(string fullname)
        {
            return MacroEngine.GetDeclarationFromFullname(fullname);
        }

        public string GetDefaultFileExtension()
        {
            return MacroEngine.GetDefaultFileExtension();
        }
        public string GetDefaultRuntime(FileDeclaration d)
        {
            return MacroEngine.GetDefaultRuntime(d);
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

        public HashSet<FileDeclaration> RenameFolder(DirectoryInfo info, string newDir)
        {
            return MacroEngine.RenameFolder(info, newDir);
        }

        public async Task<bool> DeleteFolder(DirectoryInfo info)
        {
            return await MacroEngine.DeleteFolder(info);
        } 

        public void SetExecutionValue(string name, object value)
        {
            MacroEngine.SetExecutionValue(name, value);
        }

        public void RemoveExecutionValue(string name)
        {
            MacroEngine.RemoveExecutionValue(name);
        }

        public async Task<bool> TryExecuteFile(FileDeclaration d, bool async, string runtime = "")
        {
            return await MacroEngine.TryExecuteFile(d, async, runtime);
        }

        #endregion
    }
}
