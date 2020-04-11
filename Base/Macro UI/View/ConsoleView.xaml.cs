/*
 * Mark Diedericks
 * 23/07/2018
 * Version 1.0.1
 * Console view
 */

using Macro_Engine;
using Macro_UI.Model;
using Macro_UI.Utilities;
using Macro_UI.ViewModel;
using System;
using System.Collections.Generic;
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

namespace Macro_UI.View
{
    /// <summary>
    /// Interaction logic for ConsoleView.xaml
    /// </summary>
    public partial class ConsoleView : UserControl
    {
        /// <summary>
        /// Instantiate ConsoleView
        /// </summary>
        public ConsoleView()
        {
            InitializeComponent();

            //Routing.EventManager.GetInstance().ClearAllIOEvent += () =>
            Events.SubscribeEvent("ClearAllIO", new Action(() =>
            {
                if (DataContext is ConsoleViewModel)
                    ((ConsoleViewModel)DataContext).TextLines = "";
            }));

            this.DataContextChanged += ConsoleView_DataContextChanged;

            #region TextBox Events for inputting

            txtOutput.TextChanged += (s, e) => txtOutput.CaretIndex = txtOutput.Text.Length;

            bool isDirty = true;
            txtOutput.SelectionChanged += (s, e) =>
            {
                isDirty = !isDirty;
                if (!isDirty)
                    txtOutput.CaretIndex = txtOutput.Text.Length;
            };

            txtOutput.PreviewKeyDown += (s, e) => {
                if (!(DataContext is ConsoleViewModel))
                    return;

                ConsoleViewModel vm = (ConsoleViewModel)DataContext;

                switch (e.Key)
                {
                    case Key.Enter:
                        vm.EndInput(txtOutput.Text);
                        break;
                    case Key.Escape:
                        vm.EndInput(txtOutput.Text);
                        break;
                    case Key.Back:
                        e.Handled = txtOutput.Text.Length <= vm.InputStart;
                        break;
                    default:
                        break;
                }
            };

            #endregion
        }

        private void ConsoleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ConsoleViewModel)
            {
                ConsoleViewModel vm = (ConsoleViewModel)DataContext;

                TextBoxWriter Output = new TextBoxWriter(vm);
                TextBoxWriter Error = new TextBoxWriter(vm);
                TextBoxReader Input = new TextBoxReader(vm);

                vm.FocusEvent = () =>
                {
                    txtOutput.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        txtOutput.Focus();
                        Keyboard.Focus(txtOutput);
                    }));
                };

                //Routing.EventManager.ChangeIO(String.Empty, ConsoleModel.GetInstance().Output, ConsoleModel.GetInstance().Error, null);
                Events.InvokeEvent("SetIO", String.Empty, Output, Error, Input);
            }
        }

        /// <summary>
        /// ClearAll event callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ConsoleViewModel)
                ((ConsoleViewModel)DataContext).TextLines = "";
        }
    }
}
