﻿using Macro_Engine;
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
        public Form1()
        {
            InitializeComponent();

            Executor executor = new Executor(new Action<Action>((a) => a.Invoke()));
            MacroEngine engine = MacroEngine.CreateApplicationInstance(executor);

            engine.Instantiate(new HostState());
            {
                string code = "print('hello')";


                IExecutionEngine ipy = engine.GetExecutionEngine("IronPython 2.7.9.0");
                if (ipy != null)
                {
                    System.Diagnostics.Debug.WriteLine(ipy);
                    label1.Invoke(new Action(() => label1.Text += ipy.GetLabel() + '\n'));

                    Macro mipy = new Macro("IronPython", code);
                    mipy.Execute(() => {
                        label1.Invoke(new Action(() => label1.Text += "DONE IRONPYTHON\n"));
                    }, false);
                }



                IExecutionEngine py = engine.GetExecutionEngine("PythonNET 3.5.0");
                if (py != null)
                {
                    System.Diagnostics.Debug.WriteLine(py);
                    label1.Invoke(new Action(() => label1.Text += py.GetLabel() + '\n'));

                    Macro mpy = new Macro("Python", code);
                    mpy.Execute(() => {
                        label1.Invoke(new Action(() => label1.Text += "DONE PYHONNET\n"));
                    }, false, "");
                }
            }

            this.FormClosing += new FormClosingEventHandler(delegate (object s, FormClosingEventArgs e) { engine.Destroy(); });
        }
    }
}