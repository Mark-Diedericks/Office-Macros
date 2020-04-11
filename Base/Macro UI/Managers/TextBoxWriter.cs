/*
 * Mark Diedericks
 * 22/07/2018
 * Version 1.0.0
 * TextBox Text Writer
 */
using Macro_UI.Model;
using Macro_UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Macro_UI.Utilities
{
    public class TextBoxWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        private readonly ConsoleViewModel Model;

        /// <summary>
        /// Instantiate a TextBoxWriter
        /// </summary>
        /// <param name="model">The ConsoleViewModel it is writing to</param>
        public TextBoxWriter(ConsoleViewModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="value"></param>
        public override void Write(char value)
        {
            Model.TextLines += value.ToString();
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="value"></param>
        public override void Write(string value)
        {
            Model.TextLines += value.ToString();
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public override void Write(char[] buffer, int index, int count)
        {
            Model.TextLines += new string(buffer);
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="value"></param>
        public override void WriteLine(string value)
        {
            Model.TextLines += value.ToString() + "\n";
        }
    }
}
