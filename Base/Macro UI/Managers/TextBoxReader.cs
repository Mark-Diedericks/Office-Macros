using Macro_Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Macro_UI.Utilities
{
    public class TextBoxReader : TextReader
    {
        private TextBox m_TextBox;
        private int m_StartOfLine;

        private int m_Index;
        private string m_Input;
        private bool m_HasInput;

        private bool m_Abort;

        /// <summary>
        /// Instantiate a TextBoxReader
        /// </summary>
        /// <param name="textBox">The TextBox bound to</param>
        public TextBoxReader(TextBox textBox)
        {
            m_TextBox = textBox;
            m_StartOfLine = 0;

            m_TextBox.TextChanged += (s, e) => m_TextBox.CaretIndex = m_TextBox.Text.Length;

            bool isDirty = true;
            m_TextBox.SelectionChanged += (s, e) =>
            {
                if (isDirty)
                {
                    isDirty = false;
                    m_TextBox.CaretIndex = m_TextBox.Text.Length;
                }
                else
                {
                    isDirty = true;
                }
            };

            Events.SubscribeEvent("OnDestroyed", new Action(() => m_Abort = true));
            Events.SubscribeEvent("OnTerminateExecution", new Action(() => m_Abort = true));

            m_Index = 0;
            m_Input = String.Empty;
            m_HasInput = false;
            m_Abort = false;
        }

        private void GetLineInput(bool incStart, string endLine)
        {
            Task<string> task = GetLineAsync(incStart, endLine);

            try
            {
                task.Wait();
                m_Input = task.Result;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                m_Input = endLine;
            }

            m_Index = 0;
            m_HasInput = true;
        }

        private Task<string> GetLineAsync(bool incStart, string endLine)
        {
            m_Abort = false;

            return Task.Run(new Func<string>(() =>
            {
                try
                {
                    bool completed = false;

                    m_TextBox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                        m_StartOfLine = m_TextBox.GetLineText(m_TextBox.LineCount - 1).Length;
                        int lenStart = m_TextBox.Text.Length;

                        m_TextBox.IsReadOnly = false;
                        m_TextBox.Focus();
                        Keyboard.Focus(m_TextBox);

                        m_TextBox.PreviewKeyDown += (s, e) => {
                            switch(e.Key)
                            {
                                case Key.Enter:
                                    completed = true;
                                    break;
                                case Key.Escape:
                                    m_Abort = true;
                                    break;
                                case Key.Back:
                                    e.Handled = m_TextBox.Text.Length <= lenStart;
                                    break;
                                default:
                                    break;
                            }
                        };
                    }));

                    while (!completed)
                        if (m_Abort)
                            return endLine;

                    return (String)m_TextBox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Func<string>(() => {
                        m_TextBox.IsReadOnly = true;

                        string line = m_TextBox.GetLineText(m_TextBox.LineCount - 1);
                        string subLine = "";

                        if (line.Length > m_StartOfLine)
                            subLine = line.Substring(m_StartOfLine, line.Length - m_StartOfLine);

                        return (incStart ? line : subLine) + endLine;
                    }));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return endLine;
                }
            }));
        }


        #region Sync ReadLine/ReadToEnd

        public override string ReadLine()
        {
            if (!m_HasInput)
                GetLineInput(false, "\n");
            m_HasInput = false;

            return m_Input;
        }

        public override string ReadToEnd()
        {
            if (!m_HasInput)
                GetLineInput(true, "\n");
            m_HasInput = false;

            return m_Input;
        }

        #endregion

        #region Async ReadLine/ReadToEnd

        public override Task<string> ReadLineAsync()
        {
            return GetLineAsync(false, "\n");
        }

        public override Task<string> ReadToEndAsync()
        {
            return GetLineAsync(true, "\n");
        }

        #endregion

        #region Read/Peek

        public override int Read()
        {
            if (!m_HasInput)
                GetLineInput(false, "\n");

            if (m_Index < m_Input.Length)
                return (int)m_Input[m_Index++];

            m_HasInput = false;
            return -1;
        }

        public override int Peek()
        {
            if (!m_HasInput)
                GetLineInput(false, "\n");

            if (m_Index + 1 < m_Input.Length)
                return (int)m_Input[m_Index + 1];

            m_HasInput = false;
            return -1;
        }

        #endregion
    }
}
