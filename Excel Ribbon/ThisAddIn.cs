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

namespace Excel_Ribbon
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            MacroEngine.Instantiate(Dispatcher.CurrentDispatcher, new HostState(), () =>
            {
                string code = "print('hello')";



                IExecutionEngine ipy = MacroEngine.GetExecutionEngine("IronPython");
                System.Diagnostics.Debug.WriteLine(ipy);
                if(ipy != null)
                    if(Application.ActiveSheet != null)
                        Application.ActiveSheet.Cells(1, 1).Value = ipy.GetLabel();

                Macro mipy = new Macro("IronPython", code);
                mipy.Execute(() => { if (Application.ActiveSheet != null) Application.ActiveSheet.Cells(2, 1).Value = "YAY"; }, false);

                

                IExecutionEngine py = MacroEngine.GetExecutionEngine("Python");
                System.Diagnostics.Debug.WriteLine(py);
                if (py != null)
                    if (Application.ActiveSheet != null)
                        Application.ActiveSheet.Cells(4, 1).Value = py.GetLabel();

                Macro mpy = new Macro("Python", code);
                mpy.Execute(() => { if (Application.ActiveSheet != null) Application.ActiveSheet.Cells(5, 1).Value = "YAY"; }, false);
            });            
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            MacroEngine.Destroy();
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
