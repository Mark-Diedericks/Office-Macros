using Macro_UI.View;
using Macro_UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Macro_UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Entry point of the application, instantiates the main window and its respective view model, as well as setting the saved theme
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow() { DataContext = new MainWindowViewModel() };
            ((MainWindowViewModel)MainWindow.DataContext).SetTheme(Macro_UI.Properties.Settings.Default.Theme);

            System.Diagnostics.Debug.WriteLine(">>>> >>>> >>>> >>>> " + MainWindow.DataContext);
        }
    }
}
