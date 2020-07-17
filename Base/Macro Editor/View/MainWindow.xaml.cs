/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.12
 * The main window, hosting all the UI
 */

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_Editor.Model;
using Macro_Editor.Model.Base;
using Macro_Editor.Themes;
using Macro_Editor.Utilities;
using Macro_Editor.ViewModel;
using Macro_Editor.ViewModel.Base;
using MahApps.Metro;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace Macro_Editor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static MainWindow s_Instance;

        /// <summary>
        /// Instantiation of MainWindow
        /// </summary>
        public MainWindow()
        {
            s_Instance = this;

            InitializeComponent();
            Events.SubscribeEvent("ThemeChanged", (Action)ThemeChangedEvent);

            ThemeManager.AddAccent("BaseAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/BaseAccent.xaml"));
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent("BaseAccent"), ThemeManager.GetAppTheme("BaseLight"));

            this.DataContextChanged += MainWindow_DataContextChanged;

            CommandBinding findCommand = new CommandBinding(ApplicationCommands.Find, (s, e) => 
            {
                if (((MainWindowViewModel)DataContext).DockManager.ActiveDocument is TextualEditorViewModel)
                    FindReplaceDialog.ShowForFind((((MainWindowViewModel)DataContext).DockManager.ActiveDocument as TextualEditorViewModel).GetTextEditor());
            });
            CommandBindings.Add(findCommand);

            CommandBinding searchCommand = new CommandBinding(NavigationCommands.Search, (s, e) =>
            {
                if (((MainWindowViewModel)DataContext).DockManager.ActiveDocument is TextualEditorViewModel)
                    FindReplaceDialog.ShowForFind((((MainWindowViewModel)DataContext).DockManager.ActiveDocument as TextualEditorViewModel).GetTextEditor());
            });
            CommandBindings.Add(searchCommand);

            CommandBinding replaceCommand = new CommandBinding(ApplicationCommands.Replace, (s, e) =>
            {
                if (((MainWindowViewModel)DataContext).DockManager.ActiveDocument is TextualEditorViewModel)
                    FindReplaceDialog.ShowForReplace((((MainWindowViewModel)DataContext).DockManager.ActiveDocument as TextualEditorViewModel).GetTextEditor());
            });
            CommandBindings.Add(replaceCommand);
        }

        #region Events

        /// <summary>
        /// OnClosing override, prevents the disposal of the winodw when it is closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            ((MainWindowViewModel)DataContext).OnClosing(e);
        }
        
        /// <summary>
        /// DockManagerLoaded event callback, deserializes the layout and loads it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockManagerLoaded(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DockManagerLoaded();
        }

        /// <summary>
        /// DockManagerUnloaded event callback, serializes the layout and saves it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockManagerUnloaded(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DockManagerUnloaded();
        }

        /// <summary>
        /// DataContextChanged event callback, sets Anchorable and Document contextmenus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).DocumentContextMenu = DockingManager_DockManager.DocumentContextMenu;
            ((MainWindowViewModel)DataContext).AnchorableContextMenu = DockingManager_DockManager.AnchorableContextMenu;
        }

        #endregion

        #region Getters

        /// <summary>
        /// Gets instance of Main Window
        /// </summary>
        /// <returns>MainWindow instance</returns>
        public static MainWindow GetInstance()
        {
            return s_Instance;
        }

        /// <summary>
        /// Gets DockingManager UI element
        /// </summary>
        /// <returns>DockingManager UI element</returns>
        public AvalonDock.DockingManager GetDockingManager()
        {
            return DockingManager_DockManager;
        }

        public ResourceDictionary ThemeDictionary
        {
            get
            {
                return Resources.MergedDictionaries[1];
            }
        }

        /// <summary>
        /// Gets the specified UI resource
        /// </summary>
        /// <param name="resource">The name of the resource</param>
        /// <returns>The resource object</returns>
        public object GetResource(string resource)
        {
            return Resources[resource];
        }

        /// <summary>
        /// Gets the window's resource dictionary
        /// </summary>
        /// <returns>ResourceDictionary of the winodw</returns>
        public ResourceDictionary GetResources()
        {
            return Resources;
        }

        #endregion

        #region Custom Accent & Themes
        public void UpdateThemeManager(Uri accent, ITheme theme)
        {
            ThemeManager.AddAccent("BaseAccent", accent);
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent("BaseAccent"), ThemeManager.GetAppTheme("Base" + theme.Name));
        }

        /// <summary>
        /// ThemeChanged event callback, changes the theme
        /// </summary>
        private void ThemeChangedEvent()
        {

        }

        #endregion
     }
}
