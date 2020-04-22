using Macro_Engine;
using Macro_Engine.Engine;
using Macro_Engine.Macros;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestExecution
{
    public partial class Form1 : Form
    {
        private MacroEngine engine;

        public Form1()
        {
            InitializeComponent();

            Executor executor = new Executor()
            {
                InvokeExecute = new Func<Func<bool>, Task<bool>>((a) =>
                {
                    return (Task<bool>)this.Invoke(a);
                }),
            };

            engine = MacroEngine.CreateApplicationInstance(executor);
            engine.Instantiate(new HostState());

            this.Shown += Form1_Shown;
            this.FormClosing += new FormClosingEventHandler(delegate (object s, FormClosingEventArgs e) { engine.Destroy(); });
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            {
                string code = "print('hello')";

                IExecutionEngine ipy = engine.GetExecutionEngine("IronPython 2.7.9.0");
                if (ipy != null)
                {
                    System.Diagnostics.Debug.WriteLine(ipy);
                    label1.Invoke(new Action(() => label1.Text += ipy.GetLabel() + '\n'));

                    /*Macro mipy = new Macro("IronPython", code);
                    mipy.Execute(false, () => {
                        label1.Invoke(new Action(() => label1.Text += "DONE IRONPYTHON\n"));
                    });*/

                    FileDeclaration d = new FileDeclaration(null, code);
                    Task<bool> result = engine.TryExecuteFile(d, false, "IronPython");
                    result.Wait();

                    label1.Text += "DONE IRONPYTHON\n";
                }



                IExecutionEngine py = engine.GetExecutionEngine("PythonNET 3.5.0");
                if (py != null)
                {
                    System.Diagnostics.Debug.WriteLine(py);
                    label1.Invoke(new Action(() => label1.Text += py.GetLabel() + '\n'));

                    /*Macro mpy = new Macro("Python", code);
                    mpy.Execute(false, () => {
                        label1.Invoke(new Action(() => label1.Text += "DONE PYHONNET\n"));
                    });*/

                    FileDeclaration d = new FileDeclaration(null, code);
                    Task<bool> result = engine.TryExecuteFile(d, false, "Python");
                    result.Wait();

                    label1.Text += "DONE IRONPYTHON\n";
                }
            }
        }
    }
}
