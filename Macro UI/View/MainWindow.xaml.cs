/*
 * Mark Diedericks
 * 02/08/2018
 * Version 1.0.12
 * The main window, hosting all the UI
 */

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_UI.Model;
using Macro_UI.Model.Base;
using Macro_UI.Themes;
using Macro_UI.Utilities;
using Macro_UI.ViewModel;
using Macro_UI.ViewModel.Base;
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
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Macro_UI.View
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

            System.Diagnostics.Debug.WriteLine(">>>> >>>> >>>> >>>> mw 1");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine(">>>> >>>> >>>> >>>> mw 2");

            ThemeManager.AddAccent("ExcelAccent", new Uri("pack://application:,,,/Macro UI;component/Themes/ExcelAccent.xaml"));
            ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent("ExcelAccent"), ThemeManager.GetAppTheme("BaseLight"));

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
        public Xceed.Wpf.AvalonDock.DockingManager GetDockingManager()
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
    }
}
