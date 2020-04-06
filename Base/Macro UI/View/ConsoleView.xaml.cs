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
                txtOutput.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => txtOutput.Clear()));
            }));

            this.DataContextChanged += ConsoleView_DataContextChanged;
        }

        private void ConsoleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ConsoleViewModel)
            {
                ConsoleViewModel model = (ConsoleViewModel)DataContext;
                model.Output = new TextBoxWriter(txtOutput);
                model.Error = new TextBoxWriter(txtOutput);
                model.Input = new TextBoxReader(txtOutput);

                //Routing.EventManager.ChangeIO(String.Empty, ConsoleModel.GetInstance().Output, ConsoleModel.GetInstance().Error, null);
                Events.InvokeEvent("SetIO", String.Empty, model.Output, model.Error, model.Input);
            }
        }

        /// <summary>
        /// ClearAll event callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => txtOutput.Clear()));
        }
    }
}
