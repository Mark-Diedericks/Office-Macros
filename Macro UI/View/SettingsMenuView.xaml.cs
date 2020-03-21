/*
 * Mark Diedericks
 * 31/07/2018
 * Version 1.0.6
 * Settings menu basic view logic
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Macro_Engine;
using Macro_Engine.Macros;
using Macro_UI.Model;
using Macro_UI.ViewModel;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Macro_UI.View
{
    /// <summary>
    /// Interaction logic for SettingsMenuView.xaml
    /// </summary>
    public partial class SettingsMenuView : Flyout
    {
        /// <summary>
        /// Instantiation of SettingsMenuView
        /// </summary>
        public SettingsMenuView()
        {
            InitializeComponent();

            DataContextChanged += SettingsMenuView_DataContextChanged;
            Routing.EventManager.ThemeChangedEvent += ThemeChangedEvent;
        }


        /// <summary>
        /// ThemeChanged event callback, changes the theme
        /// </summary>
        private void ThemeChangedEvent()
        {
            ThemeDictionary.MergedDictionaries.Clear();

            foreach (Uri uri in MainWindowViewModel.GetInstance().ActiveTheme.UriList)
                ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });

            Theme = FlyoutTheme.Accent;
            Theme = FlyoutTheme.Adapt;
        }

        /// <summary>
        /// DataContextChanged event callback, changes the theme setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsMenuView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((SettingsMenuViewModel)DataContext).LightTheme = Properties.Settings.Default.Theme == "Light";
        }

        private ResourceDictionary ThemeDictionary
        {
            get
            {
                return Resources.MergedDictionaries[1];
            }
        }
    }
}
