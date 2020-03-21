/*
 * Mark Diedericks
 * 22/07/2018
 * Version 1.0.0
 * TextBox Text Writer
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        private TextBox m_TextBox;

        /// <summary>
        /// Instantiate a TextBoxWriter
        /// </summary>
        /// <param name="textBox">The TextBox bound to</param>
        public TextBoxWriter(TextBox textBox)
        {
            m_TextBox = textBox;
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="value"></param>
        public override void Write(char value)
        {
            m_TextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => m_TextBox.Text += value.ToString()));
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="value"></param>
        public override void Write(string value)
        {
            m_TextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => m_TextBox.Text += value.ToString()));
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public override void Write(char[] buffer, int index, int count)
        {
            m_TextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => m_TextBox.Text += new string(buffer)));
        }

        /// <summary>
        /// Override method
        /// </summary>
        /// <param name="value"></param>
        public override void WriteLine(string value)
        {
            m_TextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (Action)(() => m_TextBox.Text += value.ToString() + '\n'));
        }
    }
}
