using Macro_Engine;
using Macro_Engine.Interop;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

using Excel = Microsoft.Office.Interop.Excel;

namespace Macro_Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static App s_Instance;
        public static App GetInstance()
        {
            return s_Instance;
        }


        private MacroEngine m_Engine;
        private MacroUI m_UI;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            s_Instance = this;

            Executor executor = null;

            m_Engine = MacroEngine.CreateApplicationInstance(executor);
            m_UI = MacroUI.CreateApplicationInstance(m_Engine);


            AssemblyDeclaration Interop_Assembly = new AssemblyDeclaration("Microsoft.Office.Interop.Excel", "./", true);
            m_UI.AddAssembly(Interop_Assembly);

            Excel.Application oExcelApp = GetExcel();

            m_UI.SetExecutionValue("HOSTNAME", "Standalone App");
            m_UI.SetExecutionValue("Excel", oExcelApp.Application);
            m_UI.SetExecutionValue("MISSING", Type.Missing);

            m_UI.AddAccent("ExcelAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/ExcelAccent.xaml"));
            m_UI.AddAccent("WordAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/WordAccent.xaml"));
            m_UI.AddAccent("PowerPointAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/PowerPointAccent.xaml"));
            m_UI.AddAccent("OneNoteAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/OneNoteAccent.xaml"));
            m_UI.AddAccent("AccessAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/AccessAccent.xaml"));
            m_UI.AddAccent("OutlookAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/OutlookAccent.xaml"));
            m_UI.AddAccent("PublisherAccent", new Uri("pack://application:,,,/Macro Editor;component/Themes/Accents/PublisherAccent.xaml"));

            m_UI.SetAccent("ExcelAccent");

            Events.InvokeEvent("ApplicationLoaded");

            MainWindow = m_UI.MainWindow;
            MainWindow.Show();
        }

        public void Destroy()
        {
            m_UI?.Destroy();
            m_Engine?.Destroy();
        }


        #region Excel

        public Excel.Application GetExcel()
        {
            try
            {
                return (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch(COMException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        #endregion

    }
}
