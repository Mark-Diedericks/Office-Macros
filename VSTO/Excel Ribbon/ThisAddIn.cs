using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using System.Windows.Threading;
using System.Threading;

using Macro_Engine;
using Macro_UI;
using Macro_Engine.Interop;
using System.Threading.Tasks;
using System.Reflection;

namespace Excel_Ribbon
{
    public partial class ThisAddIn
    {
        private Thread m_Thread;
        private MacroEngine m_Engine;
        private MacroUI m_UI;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            Executor executor = new Executor()
            {
                InvokeExecute = new Action<System.Action>((a) =>
                {
                    dispatcher.BeginInvoke(DispatcherPriority.Normal, a);
                }),
            };


            m_Thread = new Thread(() => {
                m_Engine = MacroEngine.CreateApplicationInstance(executor);
                m_UI = MacroUI.CreateApplicationInstance(m_Engine, new string[] { });

                AssemblyDeclaration Interop_Assembly = new AssemblyDeclaration("Microsoft.Office.Interop.Excel", "./", true);
                m_UI.AddAssembly(Interop_Assembly);

                m_UI.SetExecutionValue("HOSTNAME", Application.Name);
                m_UI.SetExecutionValue("Excel", (Excel.ApplicationClass)Application.Application);

                m_UI.AddAccent("ExcelAccent", new Uri("pack://application:,,,/Excel Ribbon;component/Resources/ExcelAccent.xaml"));
                m_UI.SetAccent("ExcelAccent");
                
                Events.InvokeEvent("ApplicationLoaded");
                m_UI.ShowWindow();

                m_UI.Run();
            });

            m_Thread.SetApartmentState(ApartmentState.STA);
            m_Thread.Start();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            try
            {
                m_UI.MainWindow.Dispatcher.Invoke(new System.Action(() => 
                {
                    m_UI.Destroy();
                }));

                m_Thread.Join();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
