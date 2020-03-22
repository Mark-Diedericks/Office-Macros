/*
 * Mark Diedericks
 * 30/07/2018
 * Version 1.0.2
 * File Explorer UI Control
 */

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_UI.Model;
using Macro_UI.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Macro_UI.View
{

    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : UserControl
    {
        /// <summary>
        /// Instantiate ExplorerView
        /// </summary>
        public ExplorerView()
        {
            InitializeComponent();

            ThemeChanged();

            Events.SubscribeEvent("ThemeChanged", (Action)ThemeChanged);
            //Routing.EventManager.ThemeChangedEvent += ThemeChanged;
        }
        
        private ResourceDictionary ThemeDictionary
        {
            get
            {
                return Resources.MergedDictionaries[1];
            }
        }

        /// <summary>
        /// ThemeChanged event callback, changes theme
        /// </summary>
        private void ThemeChanged()
        {
            ThemeDictionary.MergedDictionaries.Clear();

            foreach (Uri uri in MainWindowViewModel.GetInstance().ActiveTheme.UriList)
                ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        }

        /// <summary>
        /// DataContextChanged event callback, sets events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExplorerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ExplorerViewModel)DataContext).FocusEvent = delegate () { tvMacroView.Focus(); };
        }

        /// <summary>
        /// TreeView RightButtonDown event callback, opens contextmenu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvMacroView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            tvMacroView.ContextMenu = ((ExplorerViewModel)DataContext).CreateTreeViewContextMenu();
            tvMacroView.ContextMenu.IsOpen = true;
        }

        /// <summary>
        /// TreeViewItem Selected event callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            DisplayableTreeViewItem data = item.DataContext as DisplayableTreeViewItem;

            if (data != null)
                data.SelectedEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// TreeViewItem DoubleClick event callback, opens the document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            DisplayableTreeViewItem data = item.DataContext as DisplayableTreeViewItem;

            if (data != null)
                data.DoubleClickEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// TreeViewItem RightButtonaDown event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            DisplayableTreeViewItem data = item.DataContext as DisplayableTreeViewItem;

            if (data != null)
                data.RightClickEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// TextBox LostFocus event callback, finishes input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox item = sender as TextBox;
            DisplayableTreeViewItem data = item.DataContext as DisplayableTreeViewItem;

            if (data != null)
                data.FocusLostEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// TextBox KeyUp event callback, forwards event for Escape/Return key events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox item = sender as TextBox;
            DisplayableTreeViewItem data = item.DataContext as DisplayableTreeViewItem;

            if (data != null)
                data.KeyUpEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// InputBox IsVisibleChanged event callback, focus on input box UI element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement uie = sender as UIElement;
            uie.Focus();
        }
    }
}
