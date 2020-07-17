using Macro_Engine;
using Macro_Engine.Interop;
using Macro_UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Excel = Microsoft.Office.Interop.Excel;

namespace Office_Interop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private Thread m_Thread;
        private MacroEngine m_Engine;
        private MacroUI m_UI;
        private bool m_Loaded = false;


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            Executor executor = new Executor()
            {
                InvokeExecute = new Func<Func<bool>, Task<bool>>(async (a) =>
                {
                    bool result = false;
                    await dispatcher.BeginInvoke(DispatcherPriority.Normal, new System.Action(() => result = a.Invoke()));

                    return result;
                }),
            };


            m_Thread = new Thread(() => {
                m_Engine = MacroEngine.CreateApplicationInstance(executor);
                m_UI = MacroUI.CreateApplicationInstance(m_Engine, new string[] { });

                AssemblyDeclaration Interop_Assembly = new AssemblyDeclaration("Microsoft.Office.Interop.Excel", "./", true);
                m_UI.AddAssembly(Interop_Assembly);

                Excel.Application oExcelApp = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");

                m_UI.SetExecutionValue("HOSTNAME", "Standalone App");
                m_UI.SetExecutionValue("Excel", oExcelApp.Application);
                m_UI.SetExecutionValue("MISSING", Type.Missing);

                //m_UI.AddAccent("ExcelAccent", new Uri("pack://application:,,,/Excel Ribbon;component/Resources/ExcelAccent.xaml"));
                //m_UI.SetAccent("ExcelAccent");

                Events.InvokeEvent("ApplicationLoaded");
                m_UI.ShowWindow();
                m_Loaded = true;

                m_UI.MainWindow.Deactivated += (o, ex) => { Application_Exit(null, null); };

                m_UI.Run();
            });

            m_Thread.SetApartmentState(ApartmentState.STA);
            m_Thread.Start();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                int MaxSleep = 10000;
                while (!m_Loaded)
                {
                    if (MaxSleep <= 0)
                        break;

                    Thread.Sleep(100);
                    MaxSleep -= 100;
                }

                m_UI.MainWindow.Dispatcher.Invoke(() => m_UI.Destroy());

                if (!m_Thread.Join(5000))
                    m_Thread.Abort();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
