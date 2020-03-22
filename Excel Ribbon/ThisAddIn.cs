using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;
using System.Windows.Threading;

using Macro_Engine;
using Macro_Engine.Macros;
using Macro_Engine.Engine;
using System.Threading;

namespace Excel_Ribbon
{
    public partial class ThisAddIn
    {
        private Thread m_Thread;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            m_Thread = new Thread(() =>
            {
                Macro_UI.Routing.EventManager.CreateApplicationInstance(dispatcher, new string[] { });
            });

            Macro_UI.Routing.EventManager.ApplicationLoadedEvent += delegate () {
                Macro_UI.Routing.EventManager.WindowShowEvent();
            };

            m_Thread.SetApartmentState(ApartmentState.STA);
            m_Thread.IsBackground = true;
            m_Thread.Start();

            /*if (Application.ActiveSheet == null)
                Application.Workbooks.Add();

            MacroEngine.Instantiate(Dispatcher.CurrentDispatcher, new HostState(), () =>
            {
                string code = "print('hello')";


                IExecutionEngine ipy = MacroEngine.GetExecutionEngine("IronPython 2.7.9.0");
                System.Diagnostics.Debug.WriteLine(ipy);
                if(ipy != null)
                    if(Application.ActiveSheet != null)
                        Application.ActiveSheet.Cells(1, 1).Value = ipy.GetLabel();

                Macro mipy = new Macro("IronPython", code);
                mipy.Execute(() => { if (Application.ActiveSheet != null) Application.ActiveSheet.Cells(2, 1).Value = "YAY"; }, false);

                

                IExecutionEngine py = MacroEngine.GetExecutionEngine("PythonNET 3.5.0");
                System.Diagnostics.Debug.WriteLine(py);
                if (py != null)
                    if (Application.ActiveSheet != null)
                        Application.ActiveSheet.Cells(4, 1).Value = py.GetLabel();

                Macro mpy = new Macro("Python", code);
                mpy.Execute(() => { if (Application.ActiveSheet != null) Application.ActiveSheet.Cells(5, 1).Value = "YAY"; }, false);
            });*/   
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            Macro_UI.Routing.EventManager.GetInstance().Shutdown();

            try
            {
                m_Thread.Join();
                m_Thread.Abort();
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
