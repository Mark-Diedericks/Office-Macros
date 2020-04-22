/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.10
 * Primary view model for handling main window's views
 */

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_UI.Model;
using Macro_UI.Model.Base;
using Macro_UI.Routing;
using Macro_UI.Themes;
using Macro_UI.View;
using Macro_UI.ViewModel.Base;
using ICSharpCode.AvalonEdit.Document;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using System.Windows.Media;

namespace Macro_UI.ViewModel
{
    public class MainWindowViewModel : Base.ViewModel, IThemeManager
    {
        private static MainWindowViewModel s_Instance;

        /// <summary>
        /// Instantiation of MainWindowViewModel
        /// </summary>
        public MainWindowViewModel()
        {
            s_Instance = this;
            Model = new MainWindowModel();

            AddAccent("BaseAccent", new Uri("pack://application:,,,/Macro UI;component/Themes/BaseAccent.xaml"));

            AddTheme(new LightTheme());
            AddTheme(new DarkTheme());

            SetTheme(Properties.Settings.Default.Theme);
            SetAccent("BaseAccent");

            DockManager = new DockManagerViewModel(Properties.Settings.Default.OpenDocuments);

            SettingsMenu = new SettingsMenuViewModel();

            RuntimeItems = new ObservableCollection<ComboBoxItem>();

            if (MacroUI.GetInstance().IsLoaded())
                Initialize();
            else
                Events.SubscribeEvent("ApplicationLoaded", (Action)Initialize);

            Events.SubscribeEvent("ActiveMacroChanged", (Action)ActiveMacroChanged);
        }

        /// <summary>
        /// Get MainWindowViewModel instance
        /// </summary>
        /// <returns>MainWindowViewModel instance</returns>
        public static MainWindowViewModel GetInstance()
        {
            return s_Instance;
        }

        private Dispatcher Dispatcher
        {
            get
            {
                return MainWindow.GetInstance().Dispatcher;
            }
        }

        /// <summary>
        /// Invokes an action on the UI disptacher -> synchronous
        /// </summary>
        /// <param name="a">Action to be invoked</param>
        private void InvokeWindow(Action a)
        {
            if (MainWindow.GetInstance() != null)
                MainWindow.GetInstance().Dispatcher.Invoke(a);
        }
        
        /// <summary>
        /// Invokes an action on the UI disptacher -> asynchronous
        /// </summary>
        /// <param name="a">Action to be invoked</param>
        private void BeginInvokeWindow(Action a)
        {
            if (MainWindow.GetInstance() != null)
                MainWindow.GetInstance().Dispatcher.BeginInvoke(DispatcherPriority.Normal, a);
        }

        /// <summary>
        /// Populate drop down box with runtimes
        /// </summary>
        private void Initialize()
        {
            ObservableCollection<ComboBoxItem> items = new ObservableCollection<ComboBoxItem>();
            foreach (string runtime in MacroUI.GetInstance().GetRuntimes())
                items.Add(new ComboBoxItem() { Content = runtime, ToolTip = runtime, Tag = runtime });

            RuntimeItems = items;
            ActiveMacroChanged();
        }

        /// <summary>
        /// Change which runtime is selected
        /// </summary>
        private void ActiveMacroChanged()
        {
            Guid id = MacroUI.GetInstance().GetActiveMacro();

            if(id == Guid.Empty)
            {
                SelectedRuntime = RuntimeItems.FirstOrDefault<ComboBoxItem>();
                return;
            }

            IMacro macro = MacroUI.GetInstance().GetMacro(id);
            string runtime = macro.GetDefaultRuntime();
            foreach (ComboBoxItem item in RuntimeItems)
            {
                if(string.Equals((string)item.Tag, runtime, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedRuntime = item;
                    return;
                }
            }
        }

        #region Model

        private MainWindowModel m_Model;
        public MainWindowModel Model
        {
            get
            {
                return m_Model;
            }
            set
            {
                if(m_Model != value)
                {
                    m_Model = value;
                    OnPropertyChanged(nameof(Model));
                }
            }
        }

        #endregion

        #region DockManager

        public DockManagerViewModel DockManager
        {
            get
            {
                return Model.DockManager;
            }
            set
            {
                if(Model.DockManager != value)
                {
                    Model.DockManager = value;
                    OnPropertyChanged(nameof(DockManager));
                }
            }
        }

        #endregion

        #region IsShown

        public bool IsShown
        {
            get
            {
                return Model.IsShown;
            }
            set
            {
                if (Model.IsShown != value)
                {
                    if (value)
                            BeginInvokeWindow(() => MainWindow.GetInstance().Show());
                    else
                        BeginInvokeWindow(() => MainWindow.GetInstance().Hide());

                    Model.IsShown = value;
                    OnPropertyChanged(nameof(IsShown));
                }
            }
        }

        #endregion

        #region IsFocused

        public bool IsFocused
        {
            get
            {
                return Model.IsFocused;
            }
            set
            {
                if (Model.IsFocused != value)
                {
                    if (value)

                        BeginInvokeWindow(() => MainWindow.GetInstance().Focus());

                    Model.IsShown = value;
                    OnPropertyChanged(nameof(IsFocused));
                }
            }
        }

        #endregion

        #region IsClosing

        public bool IsClosing
        {
            get
            {
                return Model.IsClosing;
            }
            set
            {
                if(Model.IsClosing != value)
                {
                    Model.IsClosing = value;
                    OnPropertyChanged(nameof(IsClosing));
                }
            }
        }

        #endregion

        #region IsExecuting

        public bool IsExecuting
        {
            get
            {
                return Model.IsExecuting;
            }
            set
            {
                if(Model.IsExecuting != value)
                {
                    Model.IsExecuting = value;
                    OnPropertyChanged(nameof(IsExecuting));
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        #endregion

        #region IsEditing

        public bool IsEditing
        {
            get
            {
                return !IsExecuting;
            }
            set
            {
                if (IsExecuting == value)
                {
                    IsExecuting = !value;
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        #endregion

        #region RuntimeItems

        public ObservableCollection<ComboBoxItem> RuntimeItems
        {
            get
            {
                return Model.RuntimeItems;
            }
            set
            {
                if (Model.RuntimeItems != value)
                {
                    Model.RuntimeItems = value;
                    OnPropertyChanged(nameof(RuntimeItems));
                }
            }
        }

        #endregion

        #region SelectedRuntime

        public ComboBoxItem SelectedRuntime
        {
            get
            {
                return Model.SelectedRuntime;
            }
            set
            {
                if (Model.SelectedRuntime != value)
                {
                    Model.SelectedRuntime = value;
                    OnPropertyChanged(nameof(SelectedRuntime));
                }
            }
        }

        #endregion

        #region SettingsMenu

        public SettingsMenuViewModel SettingsMenu
        {
            get
            {
                return Model.SettingsMenu;
            }
            set
            {
                if(Model.SettingsMenu != value)
                {
                    Model.SettingsMenu = value;
                    OnPropertyChanged(nameof(SettingsMenu));
                }
            }
        }

        #endregion

        #region Accents

        public Dictionary<string, Uri> Accents
        {
            get
            {
                return Model.Accents;
            }
            set
            {
                if (Model.Accents != value)
                {
                    Model.Accents = value;
                    OnPropertyChanged(nameof(Accents));
                }
            }
        }

        #endregion

        #region Themes

        public ObservableCollection<ITheme> Themes
        {
            get
            {
                return Model.Themes;
            }
            set
            {
                if(Model.Themes != value)
                {
                    Model.Themes = value;
                    OnPropertyChanged(nameof(Themes));
                }
            }
        }

        #endregion

        #region ActiveTheme

        public ITheme ActiveTheme
        {
            get
            {
                return Model.ActiveTheme;
            }
            set
            {
                if(Model.ActiveTheme != value)
                {
                    Model.ActiveTheme = value;
                    OnPropertyChanged(nameof(ActiveTheme));
                }
            }
        }

        #endregion

        #region ActiveAccent

        public string ActiveAccent
        {
            get
            {
                return Model.ActiveAccent;
            }
            set
            {
                if (Model.ActiveAccent != value)
                {
                    Model.ActiveAccent = value;
                    OnPropertyChanged(nameof(ActiveAccent));
                }
            }
        }

        #endregion

        #region DocumentContextMenu

        public ContextMenu DocumentContextMenu
        {
            get
            {
                return Model.DocumentContextMenu;
            }
            set
            {
                if(Model.DocumentContextMenu != value)
                {
                    Model.DocumentContextMenu = value;
                    Model.DocumentContextMenu.ContextMenuOpening += DocumentContextMenu_ContextMenuOpening;
                    OnPropertyChanged(nameof(DocumentContextMenu));
                }
            }
        }

        #endregion

        #region AnchorableContextMenu

        public ContextMenu AnchorableContextMenu
        {
            get
            {
                return Model.AnchorableContextMenu;
            }
            set
            {
                if (Model.AnchorableContextMenu != value)
                {
                    Model.AnchorableContextMenu = value;
                    Model.AnchorableContextMenu.ContextMenuOpening += AnchorableContextMenu_ContextMenuOpening;
                    OnPropertyChanged(nameof(AnchorableContextMenu));
                }
            }
        }

        #endregion

        #region AsyncExecution

        public bool AsyncExecution
        {
            get
            {
                return Model.AsyncExecution;
            }
            set
            {
                if(Model.AsyncExecution != value)
                {
                    Model.AsyncExecution = value;
                    OnPropertyChanged(nameof(AsyncExecution));
                }
            }
        }

        #endregion


        #region Window Event Callbacks & Overrides

        /// <summary>
        /// OnClosing event override, prevent closing window
        /// </summary>
        /// <param name="e"></param>
        public void OnClosing(CancelEventArgs e)
        {
            SaveAll();
            IsShown = false;

            e.Cancel = !IsClosing;
        }

        /// <summary>
        /// DockManagerLoaded event callback, deserialize layout and load it
        /// </summary>
        public void DockManagerLoaded()
        {
            LoadAvalonDockLayout();
        }

        /// <summary>
        /// DockManagerUnloaded event callback, serialize loadout and save it
        /// </summary>
        public void DockManagerUnloaded()
        {
            SaveAvalonDockLayout();
        }

        #endregion

        #region Saving & Loading Settings

        /// <summary>
        /// SaveAll window based settings
        /// </summary>
        public void SaveAll()
        {
            SaveAvalonDockLayout();
            Properties.Settings.Default.OpenDocuments = DockManager.GetVisibleDocuments();
            Properties.Settings.Default.Theme = ActiveTheme.Name;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Save avalondock's layout, serialize it as xml and save it
        /// </summary>
        private void SaveAvalonDockLayout()
        {
            if (MainWindow.GetInstance() == null)
                return;

            XmlLayoutSerializer serializer = new XmlLayoutSerializer(MainWindow.GetInstance().GetDockingManager());
            StringWriter stringWriter = new StringWriter();
            XmlWriter xmlWriter = XmlWriter.Create(stringWriter);

            serializer.Serialize(xmlWriter);

            xmlWriter.Flush();
            stringWriter.Flush();

            string layout = stringWriter.ToString();

            xmlWriter.Close();
            stringWriter.Close();

            Properties.Settings.Default.AvalonLayout = layout;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Load avalondock layout, desearlize it and apply it
        /// </summary>
        private void LoadAvalonDockLayout()
        {
            if (MainWindow.GetInstance() == null)
                return;

            XmlLayoutSerializer serializer = new XmlLayoutSerializer(MainWindow.GetInstance().GetDockingManager());
            serializer.LayoutSerializationCallback += (s, args) => { args.Content = args.Content; };

            string layout = Properties.Settings.Default.AvalonLayout;

            if (String.IsNullOrEmpty(layout.Trim()))
                return;

            StringReader stringReader = new StringReader(layout);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            serializer.Deserialize(xmlReader);

            xmlReader.Close();
            stringReader.Close();
        }

        #endregion

        #region EventManager Event Function Callbacks

        /// <summary>
        /// Show the window
        /// </summary>
        public void ShowWindow()
        {
            IsShown = true;
            IsFocused = true;
            BeginInvokeWindow(() => MainWindow.GetInstance().Activate());
        }

        /// <summary>
        /// Hide the window
        /// </summary>
        public void HideWindow()
        {
            SaveAll();
            IsShown = false;
        }

        /// <summary>
        /// Close the window
        /// </summary>
        public void CloseWindow()
        {
            IsClosing = true;
            BeginInvokeWindow(() => MainWindow.GetInstance().Close());
        }

        /// <summary>
        /// Focus the window
        /// </summary>
        public void TryFocus()
        {
            IsFocused = true;
        }

        /// <summary>
        /// Displays an ok message
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The message's header</param>
        public void DisplayOkMessage(string message, string caption)
        {
            if (IsShown && MainWindow.GetInstance() != null)
                BeginInvokeWindow(async () => await MainWindow.GetInstance().ShowMessageAsync(caption, message, MessageDialogStyle.Affirmative, new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Ok",
                    CustomResourceDictionary = MainWindow.GetInstance().ThemeDictionary,
                }));
            else
                System.Windows.Forms.MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.OK);
        }

        /// <summary>
        /// Displays a yes/no message
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The message's header</param>
        /// <param name="OnReturn">The Action, and bool representation of the user's input, to be fired when the user provides input</param>
        public void DisplayYesNoMessage(string message, string caption, Action<bool> OnReturn)
        {
            if (IsShown && MainWindow.GetInstance() != null)
                BeginInvokeWindow(async () =>
                {
                    bool result = (await MainWindow.GetInstance().ShowMessageAsync(caption, message, MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Yes",
                        NegativeButtonText = "No",
                        CustomResourceDictionary = MainWindow.GetInstance().ThemeDictionary,
                    })) == MessageDialogResult.Affirmative;
                    OnReturn?.Invoke(result);
                });
            else
                OnReturn?.Invoke(System.Windows.Forms.MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes);
        }

        /// <summary>
        /// Displays a yes/no/cancel message
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The message's header</param>
        /// <param name="aux">The content of the 3rd button</param>
        /// <param name="OnReturn">The Action, and MessageDialogResult representation of the user's input, to be fired when the user provides input</param>
        public void DisplayYesNoCancelMessage(string message, string caption, string aux, Action<MessageDialogResult> OnReturn)
        {
            if (IsShown && MainWindow.GetInstance() != null)
                BeginInvokeWindow(async () =>
                {
                    MessageDialogResult result = (await MainWindow.GetInstance().ShowMessageAsync(caption, message, MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Yes",
                        NegativeButtonText = "Cancel",
                        FirstAuxiliaryButtonText = aux,
                        CustomResourceDictionary = MainWindow.GetInstance().ThemeDictionary,
                    }));
                    OnReturn?.Invoke(result);
                });
            else
                OnReturn?.Invoke(ConvertResult(System.Windows.Forms.MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.YesNoCancel)));
        }

        /// <summary>
        /// Converts a WinForms DialogResult into a MahApps.Metro MessageDialogResult
        /// </summary>
        /// <param name="result">WinForms DialogResult</param>
        /// <returns>MahApps.Metro MessageDialogResult</returns>
        private MessageDialogResult ConvertResult(System.Windows.Forms.DialogResult result)
        {
            switch (result)
            {
                case System.Windows.Forms.DialogResult.Yes:
                    return MessageDialogResult.Affirmative;
                case System.Windows.Forms.DialogResult.No:
                    return MessageDialogResult.Negative;
                case System.Windows.Forms.DialogResult.Cancel:
                    return MessageDialogResult.Canceled;
                default:
                    return MessageDialogResult.Canceled;
            }
        }

        /// <summary>
        /// Displays a yes/no message as an awaitable task
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        /// <param name="caption">The message's header</param>
        /// <returns></returns>
        public async Task<bool> DisplayYesNoMessageReturn(string message, string caption)
        {
            if (IsShown && MainWindow.GetInstance() != null)
                return await Dispatcher.Invoke(async () =>
                {
                    return (await MainWindow.GetInstance().ShowMessageAsync(caption, message, MessageDialogStyle.AffirmativeAndNegative)) == MessageDialogResult.Affirmative;
                });
            else
                return (System.Windows.Forms.MessageBox.Show(message, caption, System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes);
        }

        #endregion

        #region Themes & Styles

        /// <summary>
        /// Adds a theme to the registry
        /// </summary>
        /// <param name="theme">The theme to be added</param>
        /// <returns>Whether the the theme is already registered</returns>
        public bool AddTheme(ITheme theme)
        {
            if (Themes.Contains(theme))
                return false;

            Themes.Add(theme);
            return true;
        }

        /// <summary>
        /// Set the currently active theme
        /// </summary>
        /// <param name="name">The name of the theme</param>
        /// <returns>If the operation was successful</returns>
        public bool SetTheme(string name)
        {
            foreach (ITheme theme in Themes)
            {
                if (theme.Name.Trim().ToLower() == name.Trim().ToLower())
                {
                    ActiveTheme = theme;

                    InvokeWindow(() =>
                    {
                        MainWindow.GetInstance().ThemeDictionary.MergedDictionaries.Clear();

                        foreach (Uri uri in ActiveTheme.UriList)
                            MainWindow.GetInstance().ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
                    });

                    if (Properties.Settings.Default.Theme.Trim().ToLower() != name.Trim().ToLower())
                    {
                        Properties.Settings.Default.Theme = name.Trim();
                        Properties.Settings.Default.Save();
                    }

                    //Routing.EventManager.ThemeChanged();
                    Events.InvokeEvent("ThemeChanged");

                    InvokeWindow(() => ContextMenuThemeChange());

                    return true;
                }
            }

            return false;
        }

        public bool AddAccent(string name, Uri resource)
        {
            Accents.Add(name, resource);
            return true;
        }

        public bool SetAccent(string name)
        {
            if (!Accents.ContainsKey(name))
                return false;

            Uri accent = Accents[name];
            InvokeWindow(() => MainWindow.GetInstance().UpdateThemeManager(accent, ActiveTheme));

            foreach (ITheme theme in Themes)
                theme.SetAccent(accent);

            return SetTheme(ActiveTheme.Name);
        }

        #endregion

        #region Context Menus

        /// <summary>
        /// Updates contextmenus to adhere to a changed theme
        /// </summary>
        private void ContextMenuThemeChange()
        {
            if (DocumentContextMenu == null || AnchorableContextMenu == null)
                return;

            if (MainWindow.GetInstance() == null)
                return;

            Style ContextMenuStyle = MainWindow.GetInstance().GetResource("MetroContextMenuStyle") as Style;
            Style MenuItemStyle = MainWindow.GetInstance().GetResource("MetroMenuItemStyle") as Style;

            DocumentContextMenu.Resources.MergedDictionaries.Add(MainWindow.GetInstance().Resources);
            DocumentContextMenu.Style = ContextMenuStyle;
            DocumentContextMenu.ItemContainerStyle = MenuItemStyle;

            foreach (MenuItem item in DocumentContextMenu.Items)
                item.Style = MenuItemStyle;

            MainWindow.GetInstance().GetDockingManager().DocumentContextMenu = null;
            MainWindow.GetInstance().GetDockingManager().DocumentContextMenu = DocumentContextMenu;

            AnchorableContextMenu.Resources.MergedDictionaries.Add(MainWindow.GetInstance().Resources);
            AnchorableContextMenu.Style = ContextMenuStyle;
            AnchorableContextMenu.ItemContainerStyle = MenuItemStyle;

            foreach (MenuItem item in AnchorableContextMenu.Items)
                item.Style = MenuItemStyle;

            MainWindow.GetInstance().GetDockingManager().AnchorableContextMenu = null;
            MainWindow.GetInstance().GetDockingManager().AnchorableContextMenu = AnchorableContextMenu;
        }

        /// <summary>
        /// Open the styled document context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DocumentContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            LayoutDocumentItem document = ((ContextMenu)sender).DataContext as LayoutDocumentItem;

            if (document != null)
            {
                DocumentViewModel model = document.Model as DocumentViewModel;

                if (model != null && model != MainWindow.GetInstance().GetDockingManager().ActiveContent)
                    MainWindow.GetInstance().GetDockingManager().ActiveContent = model;

                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        /// <summary>
        /// Open the styled anchorable context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnchorableContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            LayoutAnchorableItem tool = ((ContextMenu)sender).DataContext as LayoutAnchorableItem;

            if (tool != null)
            {
                ToolViewModel model = tool.Model as ToolViewModel;

                if (model != null && model != MainWindow.GetInstance().GetDockingManager().ActiveContent)
                    MainWindow.GetInstance().GetDockingManager().ActiveContent = model;

                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        #endregion

        #region Active Documents
        
        /// <summary>
        /// Set the currently active document
        /// </summary>
        /// <param name="document">The document's DocumentViewModel</param>
        private void ChangeActiveDocument(DocumentViewModel document)
        {
            if (DockManager.Documents.Contains(document))
                DockManager.ActiveContent = document;
        }

        #endregion

        #region Toolbar & MenuBar Event Callbacks

        #region NewClick

        private ICommand m_NewClick;
        public ICommand NewClick
        {
            get
            {
                if (m_NewClick == null)
                    m_NewClick = new RelayCommand(call => NewEvent());
                return m_NewClick;
            }
        }

        /// <summary>
        /// Create a new macro
        /// </summary>
        private void NewEvent()
        {
            DockManager.Explorer.CreateMacro();
        }

        #endregion

        #region OpenClick

        private ICommand m_OpenClick;
        public ICommand OpenClick
        {
            get
            {
                if (m_OpenClick == null)
                    m_OpenClick = new RelayCommand(call => OpenEvent());
                return m_OpenClick;
            }
        }

        /// <summary>
        /// Import a macro
        /// </summary>
        private void OpenEvent()
        {
            DockManager.Explorer.ImportMacro();
        }

        #endregion

        #region ExportClick

        private ICommand m_ExportClick;
        public ICommand ExportClick
        {
            get
            {
                if (m_ExportClick == null)
                    m_ExportClick = new RelayCommand(call => ExportEvent());
                return m_ExportClick;
            }
        }

        /// <summary>
        /// Export a macro
        /// </summary>
        private void ExportEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            IMacro macro = MacroUI.GetInstance().GetMacro(DockManager.ActiveDocument.Macro);

            if (macro == null)
                return;

            macro.Export();
        }

        #endregion

        #region ExitClick

        private ICommand m_ExitClick;
        public ICommand ExitClick
        {
            get
            {
                if (m_ExitClick == null)
                    m_ExitClick = new RelayCommand(call => ExitEvent());
                return m_ExitClick;
            }
        }

        /// <summary>
        /// Close the window (hide)
        /// </summary>
        private void ExitEvent()
        {
            HideWindow();
        }

        #endregion

        #region SaveClick

        private ICommand m_SaveClick;
        public ICommand SaveClick
        {
            get
            {
                if (m_SaveClick == null)
                    m_SaveClick = new RelayCommand(call => SaveEvent());
                return m_SaveClick;
            }
        }

        /// <summary>
        /// Save the active document's macro
        /// </summary>
        private void SaveEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.SaveCommand.CanExecute(null))
                DockManager.ActiveDocument.SaveCommand.Execute(null);
        }

        #endregion

        #region SaveAllClick

        private ICommand m_SaveAllClick;
        public ICommand SaveAllClick
        {
            get
            {
                if (m_SaveAllClick == null)
                    m_SaveAllClick = new RelayCommand(call => SaveAllEvent());
                return m_SaveAllClick;
            }
        }

        /// <summary>
        /// Save all open documents' macro
        /// </summary>
        private void SaveAllEvent()
        {
            foreach (DocumentViewModel document in DockManager.Documents)
            {
                if (document.SaveCommand.CanExecute(null))
                    document.SaveCommand.Execute(null);
            }
        }

        #endregion

        #region UndoClick

        private ICommand m_UndoClick;
        public ICommand UndoClick
        {
            get
            {
                if (m_UndoClick == null)
                    m_UndoClick = new RelayCommand(call => UndoEvent());
                return m_UndoClick;
            }
        }

        /// <summary>
        /// Undo an editor event
        /// </summary>
        private void UndoEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.UndoCommand.CanExecute(null))
                DockManager.ActiveDocument.UndoCommand.Execute(null);
        }

        #endregion

        #region RedoClick

        private ICommand m_RedoClick;
        public ICommand RedoClick
        {
            get
            {
                if (m_RedoClick == null)
                    m_RedoClick = new RelayCommand(call => RedoEvent());
                return m_RedoClick;
            }
        }

        /// <summary>
        /// Redo an editor event
        /// </summary>
        private void RedoEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.RedoCommand.CanExecute(null))
                DockManager.ActiveDocument.RedoCommand.Execute(null);
        }

        #endregion

        #region CopyClick

        private ICommand m_CopyClick;
        public ICommand CopyClick
        {
            get
            {
                if (m_CopyClick == null)
                    m_CopyClick = new RelayCommand(call => CopyEvent());
                return m_CopyClick;
            }
        }

        /// <summary>
        /// Invoke editor copy event
        /// </summary>
        private void CopyEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.CopyCommand.CanExecute(null))
                DockManager.ActiveDocument.CopyCommand.Execute(null);
        }

        #endregion

        #region CutClick

        private ICommand m_CutClick;
        public ICommand CutClick
        {
            get
            {
                if (m_CutClick == null)
                    m_CutClick = new RelayCommand(call => CutEvent());
                return m_CutClick;
            }
        }

        /// <summary>
        /// Invoke editor cut event
        /// </summary>
        private void CutEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.CutCommand.CanExecute(null))
                DockManager.ActiveDocument.CutCommand.Execute(null);
        }

        #endregion

        #region PasteClick

        private ICommand m_PasteClick;
        public ICommand PasteClick
        {
            get
            {
                if (m_PasteClick == null)
                    m_PasteClick = new RelayCommand(call => PasteEvent());
                return m_PasteClick;
            }
        }

        /// <summary>
        /// Invoke editor paste event
        /// </summary>
        private void PasteEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.PasteCommand.CanExecute(null))
                DockManager.ActiveDocument.PasteCommand.Execute(null);
        }

        #endregion

        #region RunClick

        private ICommand m_RunClick;
        public ICommand RunClick
        {
            get
            {
                if (m_RunClick == null)
                    m_RunClick = new RelayCommand(call => RunEvent());
                return m_RunClick;
            }
        }

        /// <summary>
        /// Execute the active document's macro
        /// </summary>
        private void RunEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.StartCommand.CanExecute(null))
            {
                IsExecuting = true;
                DockManager.ActiveDocument.StartCommand.Execute(new Action(() => IsExecuting = false));
            }
        }

        #endregion

        #region StopClick

        private ICommand m_StopClick;
        public ICommand StopClick
        {
            get
            {
                if (m_StopClick == null)
                    m_StopClick = new RelayCommand(call => StopEvent());
                return m_StopClick;
            }
        }

        /// <summary>
        /// Terminate the execution of any actively executing macros
        /// </summary>
        private void StopEvent()
        {
            if (DockManager.ActiveDocument == null)
                return;

            if (DockManager.ActiveDocument.StopCommand.CanExecute(null))
                DockManager.ActiveDocument.StopCommand.Execute(new Action(() => IsExecuting = false));
        }

        #endregion

        #endregion

        #region Menu Bar Event Callbacks

        #region AppStyleClick

        private ICommand m_AppStyleClick;
        public ICommand AppStyleClick
        {
            get
            {
                if (m_AppStyleClick == null)
                    m_AppStyleClick = new RelayCommand(call => AppStyleClickEvent());
                return m_AppStyleClick;
            }
        }

        /// <summary>
        /// Open the style tab of the settings menu
        /// </summary>
        private void AppStyleClickEvent()
        {
            SettingsMenu.IsOpen = true;
            SettingsMenu.AppStyleActive = true;
        }

        #endregion

        #region LibraryClick

        private ICommand m_EnvironmentClick;
        public ICommand EnvironmentClick
        {
            get
            {
                if (m_EnvironmentClick == null)
                    m_EnvironmentClick = new RelayCommand(call => EnvironmentClickEvent());
                return m_EnvironmentClick;
            }
        }

        /// <summary>
        /// Open the libraries tab of the settings menu
        /// </summary>
        private void EnvironmentClickEvent()
        {
            SettingsMenu.IsOpen = true;
            SettingsMenu.EnvironmentActive = true;
        }

        #endregion

        #region MacrosClick

        private ICommand m_MacrosClick;
        public ICommand MacrosClick
        {
            get
            {
                if (m_MacrosClick == null)
                    m_MacrosClick = new RelayCommand(call => MacrosClickEvent());
                return m_MacrosClick;
            }
        }

        /// <summary>
        /// Open the macros tab of the settings menu
        /// </summary>
        private void MacrosClickEvent()
        {
            SettingsMenu.IsOpen = true;
            SettingsMenu.MacrosActive = true;
        }

        #endregion

        #region ExplorerClick

        private ICommand m_ExplorerClick;
        public ICommand ExplorerClick
        {
            get
            {
                if (m_ExplorerClick == null)
                    m_ExplorerClick = new RelayCommand(call => ExplorerClickEvent());
                return m_ExplorerClick;
            }
        }

        /// <summary>
        /// Open the explorer anchorable, if not already open
        /// </summary>
        private void ExplorerClickEvent()
        {
            ShowAnchorable(DockManager.Explorer.ContentId);
        }

        #endregion

        #region ConsoleClick

        private ICommand m_ConsoleClick;
        public ICommand ConsoleClick
        {
            get
            {
                if (m_ConsoleClick == null)
                    m_ConsoleClick = new RelayCommand(call => ConsoleClickEvent());
                return m_ConsoleClick;
            }
        }

        /// <summary>
        /// Open the console anchorable, if not already open
        /// </summary>
        private void ConsoleClickEvent()
        {
            ShowAnchorable(DockManager.Console.ContentId);
        }

        #endregion

        /// <summary>
        /// Show an anchorable, if not already visible
        /// </summary>
        /// <param name="ContentId">The contendID of the anchorable to be made visible</param>
        private void ShowAnchorable(string ContentId)
        {
            if (MainWindow.GetInstance() == null)
                return;

            foreach (ILayoutElement le in MainWindow.GetInstance().GetDockingManager().Layout.Children)
            {
                if (le is LayoutAnchorable)
                {
                    LayoutAnchorable la = le as LayoutAnchorable;

                    if (la.ContentId == ContentId)
                    {
                        la.Show();
                        return;
                    }
                }
            }
        }

        #endregion

        #region Macro Related Actions

        /// <summary>
        /// Close the document associated with a macro ID
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void CloseMacro(Guid id)
        {
            DocumentViewModel dvm = DockManager.GetDocument(id);
            if (dvm != null)
                dvm.IsClosed = true;
        }

        /// <summary>
        /// Create a new macro
        /// </summary>
        /// <param name="relativepath">The relativepath of the macro</param>
        /// <param name="OnReturn">The Action, and Guid of the new macro, which is fired when the task is completed</param>
        /// <returns>Guid of the macro</returns>
        public void CreateMacro(string relativepath, Action<Guid> OnReturn)
        {
            //return FileManager.CreateMacro(relativepath);
            //Events.InvokeEvent("CreateMacro", OnReturn, relativepath);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Import a macro into the local workspace
        /// </summary>
        /// <param name="relativepath">The relativepath to which the macro will be copied</param>
        /// <param name="OnReturn">The Action, and Guid of the new macro, which is fired when the task is completed</param>
        public void ImportMacro(string relativepath, Action<Guid> OnReturn)
        {
            //FileManager.ImportMacro(relativepath, OnReturn);
            //Events.InvokeEvent("ImportMacro", OnReturn, relativepath);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renames a folder and applies changes to UI
        /// </summary>
        /// <param name="olddir">The current relativepath of the folder</param>
        /// <param name="newdir">The desired relativepath of the folder</param>
        public void RenameFolder(string olddir, string newdir)
        {
            foreach (Guid id in MacroUI.GetInstance().RenameFolder(olddir, newdir))
            {
                DocumentViewModel dvm = DockManager.GetDocument(id);
                if (dvm != null)
                {
                    string p = MacroUI.GetInstance().GetDeclaration(id).RelativePath;
                    dvm.ToolTip = p;
                    dvm.ContentId = p;
                }
            }
        }

        /// <summary>
        /// Renames a macro and applies changes to UI
        /// </summary>
        /// <param name="id">The macro's id</param>
        /// <param name="newName">The new name of the macro</param>
        public void RenameMacro(Guid id, string newName)
        {
            MacroUI.GetInstance().RenameMacro(id, newName);

            DocumentViewModel dvm = DockManager.GetDocument(id);
            if (dvm != null)
            {
                MacroDeclaration md = MacroUI.GetInstance().GetDeclaration(id);
                dvm.Title = md.Name;
                dvm.ContentId = md.RelativePath;
            }
        }

        /// <summary>
        /// Opens a document for editing from the id of a macro
        /// </summary>
        /// <param name="id">The macro's id</param>
        public void OpenMacroForEditing(Guid id)
        {
            if (id == Guid.Empty)
                return;

            DocumentViewModel dvm = DockManager.GetDocument(id);
            if (dvm != null)
            {
                DockManager.ActiveContent = dvm;
                return;
            }

            MacroUI.GetInstance().SetActiveMacro(id);
            if (id != Guid.Empty)
            {
                DocumentModel model = DocumentModel.Create(id);

                if (model != null)
                {
                    DocumentViewModel viewModel = DocumentViewModel.Create(model);
                    DockManager.AddDocument(viewModel);
                    ChangeActiveDocument(viewModel);
                }
            }
        }

        /// <summary>
        /// Executes the active document's macro, forwards to RunClick command
        /// </summary>
        /// <param name="async">Whether or not the execution should be asynchronous or not (synchronous)</param>
        public void ExecuteMacro(bool async)
        {
            AsyncExecution = async;

            if(RunClick.CanExecute(null))
                RunClick.Execute(null);
        }

        /// <summary>
        /// Asynchronously creates a new macro, forwards to NewClick command
        /// </summary>
        public void CreateMacroAsync()
        {
            if (NewClick.CanExecute(null))
                NewClick.Execute(null);
        }
        
        /// <summary>
        /// Asynchronously imports a macro, forwards to OpenClick command
        /// </summary>
        public void ImportMacroAsync()
        {
            if (OpenClick.CanExecute(null))
                OpenClick.Execute(null);
        }

        #endregion

    }
}
